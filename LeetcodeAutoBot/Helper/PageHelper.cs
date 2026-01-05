using System.Diagnostics.CodeAnalysis;
using Microsoft.Playwright;

namespace LeetcodeAutoBot.Helper;

public static class PageExtension
{
    private static bool IsCi => Environment.GetEnvironmentVariable("CI") == "true";

	/// <summary>
	/// 在页面右上角显示一个通知消息
	/// </summary>
	/// <param name="page">Playwright Page对象</param>
	/// <param name="message">消息内容</param>
	/// <param name="duration">显示时长(毫秒), 0表示不自动消失</param>
	/// <param name="backgroundColor">背景颜色</param>
	/// <param name="textColor">文字颜色</param>
	public static async Task ShowNotificationAsync(this IPage page, string message, int duration = 5000, string backgroundColor = "#4caf50", string textColor = "#ffffff")
	{
        if (IsCi) return;

		await page.EvaluateAsync(@"
			({ message, duration, backgroundColor, textColor }) => {
				const containerId = 'leetcode-autobot-notification-container';
				let container = document.getElementById(containerId);
				if (!container) {
					container = document.createElement('div');
					container.id = containerId;
					container.style.position = 'fixed';
					container.style.top = '20px';
					container.style.right = '20px';
					container.style.zIndex = '2147483647'; // Max z-index
					container.style.display = 'flex';
					container.style.flexDirection = 'column';
					container.style.gap = '10px';
					container.style.pointerEvents = 'none'; // Allow clicking through container
					document.body.appendChild(container);
				}

				const notification = document.createElement('div');
				notification.innerText = message;
				notification.style.backgroundColor = backgroundColor;
				notification.style.color = textColor;
				notification.style.padding = '15px 20px';
				notification.style.borderRadius = '8px';
				notification.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
				notification.style.opacity = '0';
				notification.style.transform = 'translateX(20px)';
				notification.style.transition = 'all 0.3s cubic-bezier(0.68, -0.55, 0.27, 1.55)';
				notification.style.fontSize = '16px';
				notification.style.fontWeight = '500';
				notification.style.fontFamily = '-apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif';
				notification.style.pointerEvents = 'auto'; // Enable clicking on notification
				notification.style.display = 'flex';
				notification.style.alignItems = 'center';

				container.appendChild(notification);

				// Trigger reflow
				void notification.offsetWidth;

				notification.style.opacity = '1';
				notification.style.transform = 'translateX(0)';

				if (duration > 0) {
					setTimeout(() => {
						notification.style.opacity = '0';
						notification.style.transform = 'translateX(20px)';
						setTimeout(() => {
							notification.remove();
							if (container.childNodes.length === 0) {
								container.remove();
							}
						}, 300);
					}, duration);
				}
			}
		", new { message, duration, backgroundColor, textColor });
	}

	/// <summary>
	/// 在页面显示自定义HTML遮罩层
	/// </summary>
	/// <param name="page">Playwright Page对象</param>
	/// <param name="htmlContent">HTML内容</param>
	/// <param name="overlayId">遮罩层ID</param>
	public static async Task ShowHtmlOverlayAsync(this IPage page, string htmlContent, string overlayId = "leetcode-autobot-overlay")
	{
        if (IsCi) return;

		await page.EvaluateAsync(@"
			({ htmlContent, overlayId }) => {
				let overlay = document.getElementById(overlayId);
				if (overlay) overlay.remove();

				overlay = document.createElement('div');
				overlay.id = overlayId;
				overlay.style.position = 'fixed';
				overlay.style.top = '0';
				overlay.style.left = '0';
				overlay.style.width = '100vw';
				overlay.style.height = '100vh';
				overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.6)';
				overlay.style.zIndex = '2147483647';
				overlay.style.display = 'flex';
				overlay.style.justifyContent = 'center';
				overlay.style.alignItems = 'center';
				overlay.style.backdropFilter = 'blur(5px)';

				const content = document.createElement('div');
				content.style.backgroundColor = '#fff';
				content.style.padding = '30px';
				content.style.borderRadius = '12px';
				content.style.boxShadow = '0 10px 25px rgba(0,0,0,0.2)';
				content.style.maxWidth = '80%';
				content.style.maxHeight = '80%';
				content.style.overflow = 'auto';
				content.style.position = 'relative';
				content.innerHTML = htmlContent;

				overlay.appendChild(content);
				document.body.appendChild(overlay);
			}
		", new { htmlContent, overlayId });
	}

	/// <summary>
	/// 移除HTML遮罩层
	/// </summary>
	/// <param name="page">Playwright Page对象</param>
	/// <param name="overlayId">遮罩层ID</param>
	public static async Task RemoveHtmlOverlayAsync(this IPage page, string overlayId = "leetcode-autobot-overlay")
	{
        if (IsCi) return;

		await page.EvaluateAsync(@"
			({ overlayId }) => {
				const overlay = document.getElementById(overlayId);
				if (overlay) overlay.remove();
			}
		", new { overlayId });
	}

	/// <summary>
	/// 等待HTML遮罩层关闭
	/// </summary>
	/// <param name="page">Playwright Page对象</param>
	/// <param name="overlayId">遮罩层ID</param>
	/// <param name="timeout">超时时间(毫秒)</param>
	public static async Task WaitForHtmlOverlayToCloseAsync(this IPage page, string overlayId = "leetcode-autobot-overlay", float? timeout = null)
	{
        if (IsCi) return;

		await page.WaitForSelectorAsync($"#{overlayId}", new PageWaitForSelectorOptions
		{
			State = WaitForSelectorState.Detached,
			Timeout = timeout
		});
	}
}