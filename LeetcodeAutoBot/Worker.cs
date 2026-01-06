using LeetcodeAutoBot.Database.Models;
using LeetcodeAutoBot.Helper;
using LeetcodeAutoBot.Services;
using Microsoft.Playwright;

namespace LeetcodeAutoBot;

public class Worker(ILogger<Worker> logger, IServiceProvider root) : BackgroundService
{
    public string              baseUrl     = "https://leetcode.cn";
    public ICollection<string> ProblemUrls = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var url = baseUrl;
        logger.LogInformation("Navigating to {url}", url);
        var scoped = root.CreateAsyncScope();

        var problemSolver = scoped.ServiceProvider.GetRequiredService<ILeetcodeProblemSolver>();

        var page = scoped.ServiceProvider.GetRequiredService<IPage>();
        await page.GotoAsync(
            url,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 }
        );

        await Task.Delay(5000, stoppingToken);// 额外等待以确保渲染

        // 打印当前 Cookie 数量，用于调试
        var currentCookies = await page.Context.CookiesAsync();
        logger.LogInformation("当前浏览器上下文包含 {Count} 个 Cookie", currentCookies.Count);

        #region 检查登录状态

        // 检查是否登录
        // 更加稳健的检查：等待登录按钮出现，或者确认用户头像出现
        // 这里我们简单地等待一下导航栏的关键元素
        logger.LogInformation("检查登录状态...");
        try
        {
            // 尝试等待登录按钮，如果 3 秒内没出现，可能就是已登录（或者页面加载慢，所以前面用了 NetworkIdle）
            await page.WaitForSelectorAsync(
                "#navbar_sign_in_button",
                new PageWaitForSelectorOptions { Timeout = 3000 }
            );

            // 如果没抛出异常，说明找到了登录按钮 -> 未登录
            var loginBtn = await page.QuerySelectorAsync("#navbar_sign_in_button");
            if (loginBtn != null && await loginBtn.IsVisibleAsync())
            {
                // 如果是 CI 环境，无法进行交互式登录，直接报错退出
                if (Environment.GetEnvironmentVariable("CI") == "true")
                {
                    logger.LogError(
                        "检测到未登录状态！CI 环境无法手动登录。请在本地运行程序完成登录，并将更新后的数据库文件(leetcodeautobot.db)提交到仓库。"
                    );
                    Environment.Exit(1);
                }

                // 显示第一次运行提示并等待用户确认
                await page.ShowNotificationAsync("请先登录 LeetCode。登录完成后，程序将自动继续。", 2000);
                await Task.Delay(2000);
                await loginBtn.ClickAsync();

                // 等待页面跳转回 url
                await page.WaitForURLAsync(
                    url,
                    new PageWaitForURLOptions { Timeout = TimeSpan.FromMinutes(10).Microseconds }
                );

                var session = scoped.ServiceProvider.GetRequiredService<AccountSession>();
                session.AccountCookies = (await page.Context.CookiesAsync()).ToModels();
                logger.LogInformation("登录成功，Cookie 已保存");
            }
        }
        catch (TimeoutException)
        {
            // 超时说明没找到登录按钮，大概率是已登录
            logger.LogInformation("未检测到登录按钮，判断为已登录状态");
        }
        await page.ShowNotificationAsync("已经成功登录leetcode 即将开始答题");

        #endregion

        #region 每日一题

        // 获取每日一题
        logger.LogInformation("获取每日一题");
        var calendarTaskScheduleRequest = await page.APIRequest.PostAsync(
            "https://leetcode.cn/graphql/",
            new APIRequestContextOptions
            {
                DataObject = new
                {
                    query = """
                            query CalendarTaskSchedule($days: Int!) {
                              calendarTaskSchedule(days: $days) {
                                dailyQuestions {
                                  id
                                  name
                                  slug
                                  progress
                                  link
                                  premiumOnly
                                }
                              }
                            }
                            """,
                    variables     = new { days = 0 },
                    operationName = "CalendarTaskSchedule",
                },
            }
        );

        var calendarTaskScheduleResponseBody = await calendarTaskScheduleRequest.JsonAsync();

        var dailyQuestionUrl = calendarTaskScheduleResponseBody!
            .Value.GetProperty("data")
            .GetProperty("calendarTaskSchedule")
            .GetProperty("dailyQuestions")[0]
            .GetProperty("link")
            .GetString();

        logger.LogInformation("每日一题链接: {dailyQuestionUrl}", dailyQuestionUrl);

        // var problemSolver = scoped.ServiceProvider.GetRequiredService<ILeetcodeProblemSolver>();
        // await problemSolver.SolveProblemAsync(dailyQuestionUrl!);
        if(dailyQuestionUrl != null)
            ProblemUrls.Add(dailyQuestionUrl);

        #endregion

        #region 三道未作答题目

        // 获取三道没做过的题目
        logger.LogInformation("获取三道未做过的题目");
        var request = await page.APIRequest.PostAsync(
            "https://leetcode.cn/graphql/",
            new APIRequestContextOptions
            {
                DataObject = new
                {
                    query = """
                            query problemsetQuestionListV2($filters: QuestionFilterInput, $limit: Int, $searchKeyword: String, $skip: Int, $sortBy: QuestionSortByInput, $categorySlug: String) {
                              problemsetQuestionListV2(
                                filters: $filters
                                limit: $limit
                                searchKeyword: $searchKeyword
                                skip: $skip
                                sortBy: $sortBy
                                categorySlug: $categorySlug
                              ) {
                                questions {
                                  id
                                  titleSlug
                                  title
                                  translatedTitle
                                  questionFrontendId
                                  paidOnly
                                  difficulty
                                  topicTags {
                                    name
                                    slug
                                    nameTranslated
                                  }
                                  status
                                  isInMyFavorites
                                  frequency
                                  acRate
                                  contestPoint
                                }
                                totalLength
                                finishedLength
                                hasMore
                              }
                            }
                            """,
                    variables = new
                    {
                        skip         = 0,
                        limit        = 3,
                        categorySlug = "all-code-essentials",
                        filters = new
                        {
                            filterCombineType = "ALL",
                            statusFilter = new
                            {
                                questionStatuses = new[] { "SOLVED" },
                                @operator        = "IS_NOT"
                            },
                            premiumFilter = new
                            {
                                premiumStatus = new[] { "NOT_PREMIUM" },
                                @operator     = "IS"
                            }
                        },
                    }

                },
            }
        );
        var responseBody = await request.JsonAsync();
        var slugs = responseBody!
            .Value.GetProperty("data")
            .GetProperty("problemsetQuestionListV2")
            .GetProperty("questions")
            .EnumerateArray()
            .Select(q => q.GetProperty("titleSlug").GetString()!)
            .ToList();

        var links = slugs.Select(slug => $"{baseUrl}/problems/{slug}").ToList();
        logger.LogInformation("待答题列表: {links}", string.Join(", ", links));
        ProblemUrls = ProblemUrls.Union(links).ToList();

        #endregion

        #region 做题

        foreach (var link in ProblemUrls)
        {
            logger.LogInformation("开始答题: {link}", link);
            await problemSolver.SolveProblemAsync(link);
        }

        #endregion

        #region 结束收尾

        logger.LogInformation("答题完成，保存登录状态并退出");
        var cookies        = await page.Context.CookiesAsync();
        var accountSession = scoped.ServiceProvider.GetRequiredService<AccountSession>();
        accountSession.AccountCookies = cookies.ToModels();

        // 保存 LocalStorage
        try
        {
            // // 尝试获取 sessionStorage
            // var sessionStorage = await page.EvaluateAsync<Dictionary<string, string>>(
            //     """
            //     () => { 
            //         const origin = {}; 
            //         for (let i = 0; i < sessionStorage.length; i++) { 
            //             const key = sessionStorage.key(i); 
            //             origin[key] = sessionStorage.getItem(key); 
            //         } 
            //         return origin; 
            //     }
            //     """
            // );
            // logger.LogInformation("SessionStorage 已获取，共 {Count} 条", sessionStorage.Count);

            // 尝试获取 localStorage
            var localStorage = await page.EvaluateAsync<Dictionary<string, string>>(
                """
                () => { 
                    const origin = {}; 
                    for (let i = 0; i < localStorage.length; i++) {
                        const key = localStorage.key(i); 
                        origin[key] = localStorage.getItem(key); 
                    } 
                    return origin; 
                }
                """
            );
            accountSession.LocalStorage = localStorage;
            logger.LogInformation("LocalStorage 已保存，共 {Count} 条", localStorage.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "保存 Storage 失败");
        }

        await page.CloseAsync();

        await Task.Delay(5000, stoppingToken);
        await scoped.DisposeAsync();

        // terminate the application
        logger.LogInformation("Exiting application in 5 seconds...");
        await Task.Delay(5000, stoppingToken);
        Environment.Exit(0);

        #endregion
    }
}