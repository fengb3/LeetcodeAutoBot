using System.Globalization;
using System.Text.RegularExpressions;

namespace LeetcodeAutoBot.Helper;

public static partial class StringHelper
{
	#region Url

	public static string[] GetUrls(this string str)
	{
		return UrlRegex()
		      .Matches(str)
		      .Select(m => m.Value)
		      .Where(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
		      .ToArray();
	}

	[GeneratedRegex(@"https?://\S+")]
	private static partial Regex UrlRegex();
	
	public static string? TryFetchSkuId(this string url, out bool success)
	{
		var match = JDSkuIdRegex().Match(url);
		
		string? result = null;
		success = match.Success;
		
		if (match.Success)
		{
			result = match.Groups[1].Value;
		}

		return result;
	}
	
	[GeneratedRegex(@"/product/(\d+)\.html")]
	private static partial Regex JDSkuIdRegex();

	#endregion
	
	#region Date

	public static DateTime? GetStartDate(this string input, out string err)
	{
		err = "";
		var parts = input.Split(new[] { "至" }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 2)
		{
			err = ("格式错误：字符串应包含开始和结束时间");
			
			return null;
		}

		var startStr = parts[0].Trim();
		// string endStr   = parts[1].Trim(); // TODO: 处理结束时间, 有可能不包含完整日期内容, 例如 "20:00". 目前不着急
        
		// 使用中文文化确保解析正确
		var culture = CultureInfo.GetCultureInfo("zh-CN");
		DateTime    startDate;
		try
		{
			startDate = DateTime.ParseExact(startStr, "M'月'd'日'H:mm", culture);
			//endDate   = DateTime.ParseExact(endStr, "M'月'd'日'H:mm", culture);
		}
		catch (FormatException)
		{
			err ="日期格式无效";
			return null;
		}
        
		// 假设时间为本地时区，或手动指定时区偏移（例如UTC+8）
		// 方式1：使用本地时区偏移
		var startDto = new DateTimeOffset(startDate, TimeZoneInfo.Local.GetUtcOffset(startDate));
		var startDt = new DateTime(startDate.Ticks, DateTimeKind.Local);
		//DateTimeOffset endDto   = new DateTimeOffset(endDate, TimeZoneInfo.Local.GetUtcOffset(endDate));
		
		return startDt;
	}

    #endregion
}