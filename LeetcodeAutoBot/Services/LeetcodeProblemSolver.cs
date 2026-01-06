using System.Text.Json;
using System.Text.RegularExpressions;
using LeetcodeAutoBot.Helper;
using Microsoft.Playwright;

namespace LeetcodeAutoBot.Services;

public interface ILeetcodeProblemSolver
{
    Task SolveProblemAsync(string problemUrl);
}

public partial class LeetcodeProblemSolver(IPage page, ILogger<LeetcodeProblemSolver> logger)
    : ILeetcodeProblemSolver,
        IDisposable,
        IAsyncDisposable
{
    public string solutionLanguage = "";
    public string problemSlug      = "";
    public string solutionSlug     = "";
    public string solutionTopicId  = "";
    public string solutionUrl      = "";

    public async Task SolveProblemAsync(string problemUrl)
    {
        var parts = problemUrl.TrimEnd('/').Split('/');
        problemSlug = parts.Last();

        logger.LogInformation("problemSlug: {problemSlug}", problemSlug);

        var response = await page.APIRequest.PostAsync(
            "https://leetcode.cn/graphql/",
            new APIRequestContextOptions
            {
                DataObject = new
                {
                    query = """
                            query questionTopicsList($questionSlug: String!, $skip: Int, $first: Int, $orderBy: SolutionArticleOrderBy, $userInput: String, $tagSlugs: [String!]) {
                              questionSolutionArticles(
                                questionSlug: $questionSlug
                                skip: $skip
                                first: $first
                                orderBy: $orderBy
                                userInput: $userInput
                                tagSlugs: $tagSlugs
                              ) {
                                totalNum
                                edges {
                                  node {
                                    rewardEnabled
                                    canEditReward
                                    uuid
                                    title
                                    slug
                                    sunk
                                    chargeType
                                    status
                                    identifier
                                    canEdit
                                    canSee
                                    reactionType
                                    hasVideo
                                    favoriteCount
                                    upvoteCount
                                    reactionsV2 {
                                      count
                                      reactionType
                                    }
                                    tags {
                                      name
                                      nameTranslated
                                      slug
                                      tagType
                                    }
                                    createdAt
                                    thumbnail
                                    author {
                                      username
                                      certificationLevel
                                      profile {
                                        userAvatar
                                        userSlug
                                        realName
                                        reputation
                                      }
                                    }
                                    summary
                                    topic {
                                      id
                                      commentCount
                                      viewCount
                                      pinned
                                    }
                                    byLeetcode
                                    isMyFavorite
                                    isMostPopular
                                    isEditorsPick
                                    hitCount
                                    videosInfo {
                                      videoId
                                      coverUrl
                                      duration
                                    }
                                  }
                                }
                              }
                            }
                            """,
                    variables = new
                    {
                        questionSlug = problemSlug,
                        skip         = 0,
                        first        = 5,
                        orderBy      = "DEFAULT",
                        userInput    = "",
                        tagSlugs     = Array.Empty<string>(),
                    },
                    operationName = "questionTopicsList",
                },
            }
        );

        var responseBody = await response.JsonAsync();

        if (responseBody is null)
        {
            throw new InvalidOperationException("GraphQL response body is null.");
        }

        var edges = responseBody
            .Value.GetProperty("data")
            .GetProperty("questionSolutionArticles")
            .GetProperty("edges")
            .EnumerateArray()
            .ToArray();

        var officialEdge = edges.FirstOrDefault(edge => {
            var level = edge.GetProperty("node")
                .GetProperty("author")
                .GetProperty("certificationLevel")
                .GetString();
            return level == "OFFICIAL";
        });

        if (officialEdge.ValueKind == System.Text.Json.JsonValueKind.Undefined)
        {
            throw new InvalidOperationException(
                "No OFFICIAL solution article found for this problem."
            );
        }

        var firstOfficialSolution = officialEdge.GetProperty("node");

        solutionSlug = firstOfficialSolution.GetProperty("slug").GetString() ?? "";
        solutionTopicId = firstOfficialSolution
            .GetProperty("topic")
            .GetProperty("id")
            .GetInt32()
            .ToString();

        await page.ShowNotificationAsync("正在获取题解并提交代码...", 2000);

        logger.LogInformation("solutionSlug: {solutionSlug}", solutionSlug);
        logger.LogInformation("solutionTopicId: {solutionTopicId}", solutionTopicId);

        solutionUrl =
            $"https://leetcode.cn/problems/{problemSlug}/solutions/{solutionTopicId}/{solutionSlug}";

        await page.GotoAsync(
            solutionUrl,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 }
        );

        // 尝试等待一会儿让页面加载
        await Task.Delay(3000);

        // 再次检查登录状态，如果未登录尝试刷新
        var loginBtn = await page.QuerySelectorAsync("#navbar_sign_in_button");
        if (loginBtn != null && await loginBtn.IsVisibleAsync())
        {
            logger.LogWarning("检测到题解页面未登录，尝试刷新页面...");
            await page.ReloadAsync(
                new PageReloadOptions { WaitUntil = WaitUntilState.DOMContentLoaded }
            );
            await Task.Delay(3000);
        }

        _ = page.ShowNotificationAsync("正在解析题解代码...");

        var codeBlockSelector = "div.border-gray-3.mb-6.rounded-lg";
        try
        {
            await page.WaitForSelectorAsync(codeBlockSelector);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to find code block selector.");
            return;
        }

        var       codeBlocks = page.Locator(codeBlockSelector);
        var       count      = await codeBlocks.CountAsync();
        ILocator? codeBlock  = null;
        ILocator? tabs       = null;

        // 从后往前找，第一个有多个语言选项的代码块, 通常是最完整的解法
        for (int i = count; i >= 0 ; i--)
        {
            var block       = codeBlocks.Nth(i);
            var tabsInBlock = block.Locator("div.flex.select-none.overflow-x-auto > div");

            // var txt         = string.Join("\n", await block.AllInnerTextsAsync(), '\n');
            // var tabsTxt     = string.Join(',', await tabsInBlock.AllInnerTextsAsync());
            // var txtCombined = $"Code Block {i + 1}/{count} with tabs [{tabsTxt}]:\n{txt}";
            // logger.LogInformation(txtCombined);

            if (await tabsInBlock.CountAsync() > 1)
            {
                codeBlock = block;
                tabs = tabsInBlock;
                break;
            }
        }

        codeBlock ??= codeBlocks.First;
        tabs      ??= codeBlock.Locator("div.flex.select-none.overflow-x-auto > div");
        
        var pythonTab = tabs.Filter(new LocatorFilterOptions { HasText = "python" });

        if (await pythonTab.CountAsync() > 0)
        {
            await pythonTab.ClickAsync();
            await page.WaitForTimeoutAsync(500);
        }

        var activeTab = codeBlock.Locator(
            "div.flex.select-none.overflow-x-auto > div.text-label-1"
        );
        
        if (await activeTab.CountAsync() > 0)
        {
            solutionLanguage = await activeTab.InnerTextAsync();
        }
        else
        {
            var codeLocatorForClass = codeBlock.Locator("pre > code");
            var classAttr           = await codeLocatorForClass.GetAttributeAsync("class");
            if (!string.IsNullOrEmpty(classAttr))
            {
                var match = ProgramLanguageRegex().Match(classAttr);
                solutionLanguage = match.Success ? match.Groups[1].Value : "Unknown";
            }
            else
            {
                solutionLanguage = "Unknown";
            }
        }

        var codeLocator = codeBlock.Locator("pre > code");
        var code        = await codeLocator.InnerTextAsync();

        await page.ShowNotificationAsync($"答案语言: {solutionLanguage}", 3000);
        await page.ShowNotificationAsync($"答案代码共 {code.Length} 个字符", 3000);
        await Task.Delay(2000);

        if (solutionLanguage == "Unknown")
        {
            logger.LogWarning("Could not determine solution language. Skipping submission.");
            return;
        }

        // 1. Go to description page
        var descriptionUrl = $"https://leetcode.cn/problems/{problemSlug}/description/";
        await page.GotoAsync(descriptionUrl);
        await page.WaitForTimeoutAsync(2000);

        _ = page.ShowNotificationAsync("跳转到题目描述页", 2000);

        // Wait for editor to load
        await page.WaitForSelectorAsync(".monaco-editor");

        // 2. Select Language
        // Try to find the language dropdown button.
        // We look for a button that likely contains the current language name.
        var knownLanguages = new[]
        {
            "C++",
            "Java",
            "Python",
            "Python3",
            "C",
            "C#",
            "JavaScript",
            "TypeScript",
            "PHP",
            "Swift",
            "Kotlin",
            "Dart",
            "Go",
            "Ruby",
            "Scala",
            "Rust",
            "Racket",
            "Erlang",
            "Elixir",
        };
        var langButtonPattern = new Regex(
            $"^({string.Join("|", knownLanguages.Select(Regex.Escape))})$"
        );

        var langDropdown = page.Locator("button")
            .Filter(new LocatorFilterOptions { HasTextRegex = langButtonPattern })
            .First;

        if (await langDropdown.CountAsync() > 0)
        {
            var currentLang = await langDropdown.InnerTextAsync();
            if (!currentLang.Contains(solutionLanguage, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation(
                    "Switching language from {currentLang} to {solutionLanguage}",
                    currentLang,
                    solutionLanguage
                );
                await langDropdown.ClickAsync();

                // Wait for menu to open
                await page.WaitForTimeoutAsync(500);

                // User provided selector hint: #radix-\:r2c\: > div > div > div
                // The ID is dynamic, so we use [id^='radix-'] to match the container.
                var langOption = page.Locator("div[id^='radix-'] > div > div > div")
                    .Filter(
                        new() { HasTextRegex = new Regex($"^{Regex.Escape(solutionLanguage)}$") }
                    )
                    .First;

                // Fallback: if the specific structure doesn't match, try finding the text in any visible div
                if (await langOption.CountAsync() == 0)
                {
                    langOption = page.Locator("div")
                        .Filter(
                            new()
                            {
                                HasTextRegex = new Regex($"^{Regex.Escape(solutionLanguage)}$"),
                            }
                        )
                        .Locator("visible=true")
                        .Last;
                }

                if (await langOption.CountAsync() > 0)
                {
                    await langOption.ClickAsync();

                    // Handle confirmation dialog if it appears (e.g. "Reset code?")
                    var confirmBtn = page.Locator("button").Filter(new() { HasText = "Confirm" });
                    // Note: The text might be "确认" in Chinese or "Confirm" in English depending on locale.
                    // LeetCode CN usually uses Chinese. "确认" or "确定".
                    // Let's try to match both or just wait a bit and see if a dialog appears.
                    try
                    {
                        var confirmDialogBtn = page.Locator("button.is-danger");// Often the confirm button is red/danger
                        if (await confirmDialogBtn.IsVisibleAsync())
                        {
                            await confirmDialogBtn.ClickAsync();
                        }
                    }
                    catch {}
                }
                else
                {
                    logger.LogWarning($"Could not find language option for {solutionLanguage}");
                }
            }
        }
        else
        {
            logger.LogWarning("Could not find language dropdown button.");
        }

        // 3. Enter Code
        logger.LogInformation("Entering code...");
        _ = page.ShowNotificationAsync("解答题目");

        // Try to set code via Monaco Editor API first to avoid formatting issues (staircase indentation, extra braces)
        var codeSet = await page.EvaluateAsync<bool>(
            """
            (code) => {
                        try {
                            if (window.monaco && window.monaco.editor) {
                                var models = window.monaco.editor.getModels();
                                if (models.length > 0) {
                                    // Use the last model as it is often the visible one if multiple exist
                                    // or iterate to find the one with uri starting with 'file:///' or similar if needed.
                                    // For LeetCode, usually models[0] works, but let's try to be safe.
                                    models[0].setValue(code);
                                    return true;
                                }
                            }
                        } catch (e) {
                            return false;
                        }
                        return false;
                    }
            """,
            code
        );

        if (!codeSet)
        {
            logger.LogWarning("Monaco API not available, falling back to keyboard input.");
            var editorInput = page.GetByRole(
                AriaRole.Textbox,
                new() { NameRegex = new Regex("Editor content", RegexOptions.IgnoreCase) }
            );
            await editorInput.ClickAsync();
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.PressAsync("Backspace");
            await page.Keyboard.InsertTextAsync(code);
        }

        // 4. Submit
        var submitBtn = page.Locator("[data-e2e-locator=\"console-submit-button\"]");
        logger.LogInformation("Submitting solution...");
        _ = page.ShowNotificationAsync("提交代码...");
        await submitBtn.ClickAsync();
        await Task.Delay(5000);

        // 5. Check Result

        logger.LogInformation("Waiting for submission result...");
        _ = page.ShowNotificationAsync("等待结果...", 10000);

        // Wait 1 seconds before first check
        await Task.Delay(1000);

        int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var submissionResponse = await page.APIRequest.PostAsync(
                    "https://leetcode.cn/graphql/",
                    new APIRequestContextOptions
                    {
                        DataObject = new
                        {
                            query = """
                                    query submissionList($offset: Int!, $limit: Int!, $lastKey: String, $questionSlug: String!, $lang: String, $status: SubmissionStatusEnum) {
                                      submissionList(
                                        offset: $offset
                                        limit: $limit
                                        lastKey: $lastKey
                                        questionSlug: $questionSlug
                                        lang: $lang
                                        status: $status
                                      ) {
                                        lastKey
                                        hasNext
                                        submissions {
                                          id
                                          title
                                          status
                                          statusDisplay
                                          lang
                                          langName: langVerboseName
                                          runtime
                                          timestamp
                                          url
                                          isPending
                                          memory
                                          frontendId
                                          submissionComment {
                                            comment
                                            flagType
                                          }
                                        }
                                      }
                                    }
                                    """,
                            variables = new
                            {
                                questionSlug = problemSlug,
                                offset       = 0,
                                limit        = 20,
                                lastKey      = (string?)null,
                                status       = (string?)null,
                            },
                            operationName = "submissionList",
                        },
                    }
                );

                var submissionResponseBody = await submissionResponse.JsonAsync();

                if (submissionResponseBody is null)
                {
                    logger.LogWarning("GraphQL response body is null.");
                    await Task.Delay(2000);
                    continue;
                }

                var data           = submissionResponseBody.Value.GetProperty("data");
                var submissionList = data.GetProperty("submissionList");
                var submissions    = submissionList.GetProperty("submissions");

                if (submissions.GetArrayLength() == 0)
                {
                    logger.LogWarning("No submissions found.");
                    await Task.Delay(2000);
                    continue;
                }

                var latestSubmission = submissions[0];
                var statusDisplay    = latestSubmission.GetProperty("statusDisplay").GetString();
                var isPending        = latestSubmission.GetProperty("isPending").GetString();

                logger.LogInformation(
                    "Submission status: {statusDisplay}, isPending: {isPending}",
                    statusDisplay,
                    isPending
                );

                if (isPending != "Not Pending")
                {
                    logger.LogInformation("Submission is pending...");
                    _ = page.ShowNotificationAsync("判题中...", 2000);
                    await Task.Delay(2000);
                    continue;
                }

                logger.LogInformation("Submission Result: {statusDisplay}", statusDisplay);
                _ = page.ShowNotificationAsync($"提交结果: {statusDisplay} 拜拜~~", 5000);

                await Task.Delay(2000);
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking submission result.");
                await Task.Delay(2000);
            }
        }
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await page.CloseAsync();
    }

    [GeneratedRegex(@"language-(\w+)")]
    private static partial Regex ProgramLanguageRegex();
}