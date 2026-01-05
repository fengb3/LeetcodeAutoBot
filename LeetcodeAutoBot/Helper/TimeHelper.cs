namespace LeetcodeAutoBot.Helper;

public static class TimeHelper
{
    public static void WaitUntilTime(DateTime targetTime, CancellationToken ct = default)
    {
        Console.WriteLine($"等待时间到达: {DateTime.Now:HH:mm:ss.fff} -> {targetTime:HH:mm:ss.fff}");
        
        while (DateTime.Now < targetTime)
        {
            // 减少CPU占用：剩余时间较长时短暂休眠
            var remaining = targetTime - DateTime.Now;
            var remainingMilliseconds = remaining.TotalMilliseconds;

            // Console.WriteLine(remainingMilliseconds);
            
            if (remainingMilliseconds > 1000 * 60)
            {
                // wipe last line in console
                Console.Write("\r\033[K");
                Console.WriteLine($"等待时间到达: {DateTime.Now:HH:mm:ss.fff} -> {targetTime:HH:mm:ss.fff}");
                Thread.Sleep(9000);
                if (ct.IsCancellationRequested)
                {
                    // 取消请求
                    break;
                }

                // continue;
            }
            else if (remainingMilliseconds > 1000 * 10)
            {
                Console.Write("\r\033[K");
                Console.WriteLine($"等待时间到达: {DateTime.Now:HH:mm:ss.fff} -> {targetTime:HH:mm:ss.fff}");
                Thread.Sleep(900);
                if (ct.IsCancellationRequested)
                {
                    // 取消请求
                    break;
                }

                // continue;
            }
            else if (remainingMilliseconds > 1000)
            {
                // Console.WriteLine("还有1秒以上，休眠90毫秒");
                Thread.Sleep(90);
                // continue;
            }
            else
            {
                
                Thread.SpinWait(100); // 高精度等待
            }
        }
    }
}