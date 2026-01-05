#!/bin/bash

# 获取当前脚本所在目录的绝对路径
# 假设脚本位于项目根目录，而实际的 C# 项目位于 LeetcodeAutoBot 子目录
BASE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_DIR="$BASE_DIR/LeetcodeAutoBot"
PLIST_NAME="com.leetcodeautobot.daily.plist"
PLIST_PATH="$HOME/Library/LaunchAgents/$PLIST_NAME"

# 尝试查找 dotnet 路径
DOTNET_PATH=$(which dotnet)

if [ -z "$DOTNET_PATH" ]; then
    # 常见的 dotnet 安装路径 fallback
    if [ -f "/usr/local/share/dotnet/dotnet" ]; then
        DOTNET_PATH="/usr/local/share/dotnet/dotnet"
    elif [ -f "/opt/homebrew/bin/dotnet" ]; then
        DOTNET_PATH="/opt/homebrew/bin/dotnet"
    else
        echo "Error: dotnet executable not found in PATH. Please ensure .NET SDK is installed."
        exit 1
    fi
fi

echo "Found dotnet at: $DOTNET_PATH"
echo "Project Directory: $PROJECT_DIR"

# 创建 plist 文件内容
# 注意：这里配置了每天 09:00 运行
cat <<EOF > "$PLIST_PATH"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.leetcodeautobot.daily</string>
    <key>ProgramArguments</key>
    <array>
        <string>$DOTNET_PATH</string>
        <string>run</string>
        <string>--launch-profile</string>
        <string>LeetcodeAutoBot - CI</string>
    </array>
    <key>WorkingDirectory</key>
    <string>$PROJECT_DIR</string>
    <key>EnvironmentVariables</key>
    <dict>
        <key>DOTNET_CLI_TELEMETRY_OPTOUT</key>
        <string>1</string>
    </dict>
    <key>StartCalendarInterval</key>
    <dict>
        <key>Hour</key>
        <integer>9</integer>
        <key>Minute</key>
        <integer>0</integer>
    </dict>
    <key>StandardOutPath</key>
    <string>$PROJECT_DIR/stdout.log</string>
    <key>StandardErrorPath</key>
    <string>$PROJECT_DIR/stderr.log</string>
</dict>
</plist>
EOF

# 卸载旧的任务（如果存在）以确保更新
launchctl unload "$PLIST_PATH" 2>/dev/null

# 加载新的任务
launchctl load "$PLIST_PATH"

echo "------------------------------------------------"
echo "✅ 成功注册定时任务: com.leetcodeautobot.daily"
echo "📅 运行时间: 每天 09:00"
echo "📂 工作目录: $PROJECT_DIR"
echo "📄 日志文件: $PROJECT_DIR/stdout.log"
echo "------------------------------------------------"
echo "如果需要卸载任务，请运行: launchctl unload $PLIST_PATH && rm $PLIST_PATH"
