using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LeetcodeAutoBot.Helper;

public static class NetworkHelper
{
    /// <summary>
    /// 测量网络延迟（RTT）
    /// </summary>
    /// <param name="serverAddress"></param>
    /// <param name="pingCount"></param>
    /// <returns></returns>
    public static async Task<long> MeasureRttAsync(string serverAddress, int pingCount = 10, CancellationToken ct = default)
    {
        var ping = new Ping();
        var rtts = new List<long>();

        for (int i = 0; i < pingCount; i++)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }
            
            try
            {
                var reply = await ping.SendPingAsync(serverAddress, 1000); // 1秒超时
                if (reply.Status == IPStatus.Success)
                {
                    rtts.Add(reply.RoundtripTime);
                }
            }
            catch
            {
                /* 忽略失败请求 */
            }

            await Task.Delay(100, ct); // 避免密集请求
        }

        // 取中位数减少异常值影响
        rtts.Sort();
        return rtts.Count == 0 ? -1 : rtts[rtts.Count / 2];
    }


    public static DateTime GetNetworkTime(string ntpServer = "pool.ntp.org")
    {
        // NTP 协议数据包（48字节）
        byte[] ntpData = new byte[48];
        ntpData[0] = 0x1B; // LI=0, VN=3 (NTPv3), Mode=3 (Client)

        // 发送请求
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Connect(ntpServer, 123);
            socket.Send(ntpData);
            socket.Receive(ntpData);
        }

        // 解析服务器响应时间戳（位于第40-47字节）
        ulong intPart = BitConverter.ToUInt32(ntpData, 40).SwapEndian();
        ulong fracPart = BitConverter.ToUInt32(ntpData, 44).SwapEndian();

        // 转换为Unix时间戳（自1900年1月1日以来的毫秒数）
        ulong milliseconds = (intPart * 1000) + ((fracPart * 1000) / 0x100000000L);

        // 转换为DateTime（UTC时间）
        return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMilliseconds(milliseconds);
    }


// 辅助方法：转换字节序
    public static uint SwapEndian(this uint x)
    {
        return (uint)((x & 0x000000FF) << 24 |
                      (x & 0x0000FF00) << 8 |
                      (x & 0x00FF0000) >> 8 |
                      (x & 0xFF000000) >> 24);
    }
}