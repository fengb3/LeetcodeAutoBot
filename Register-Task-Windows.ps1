$ErrorActionPreference = "Stop"

# 配置部分
$TaskName = "LeetcodeAutoBot"
$Time = "09:00" # 每天早上 9 点运行
$ProjectDir = Join-Path $PSScriptRoot "LeetcodeAutoBot"

# 定义操作
# 使用 dotnet run
# Execute: dotnet
# Argument: run --launch-profile "LeetcodeAutoBot - CI"
# WorkingDirectory: 项目目录 (必须设置正确，否则找不到 csproj 或数据库)
$Action = New-ScheduledTaskAction -Execute "dotnet" -Argument "run --launch-profile ""LeetcodeAutoBot - CI""" -WorkingDirectory $ProjectDir

# 定义触发器 (每天运行)
$Trigger = New-ScheduledTaskTrigger -Daily -At $Time

# 定义设置 (允许在电池模式下运行，唤醒计算机运行等)
$Settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -WakeToRun

# 注册任务
# -Force 覆盖同名任务
Register-ScheduledTask -TaskName $TaskName -Action $Action -Trigger $Trigger -Settings $Settings -Description "自动完成 LeetCode 每日一题 (dotnet run CI profile)" -Force

Write-Host "✅ 成功注册定时任务: $TaskName" -ForegroundColor Green
Write-Host "📅 运行时间: 每天 $Time" -ForegroundColor Cyan
Write-Host "📂 工作目录: $ProjectDir" -ForegroundColor Gray
Write-Host "🚀 运行方式: dotnet run --launch-profile ""LeetcodeAutoBot - CI""" -ForegroundColor Magenta
Write-Host "你可以打开 '任务计划程序' (Task Scheduler) 查看或修改它。" -ForegroundColor Yellow
