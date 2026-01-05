# LeetcodeAutoBot

这是一个自动 LeetCode (leetcode.cn) 做题机器人，旨在帮助用户自动完成每日一题，获取积分以兑换 LeetCode 周边礼品。

## ✨ 功能特性

- 🤖 **自动登录**: 支持 LeetCode 中国站 (leetcode.cn) 自动登录。
- 📅 **每日一题**: 自动识别并完成当天的每日一题。
- 🍪 **Cookie 持久化**: 将登录状态保存至数据库，减少重复登录次数。
- ⏰ **定时任务**: 提供 PowerShell 脚本，轻松注册 Windows 任务计划程序。
- 🐳 **Docker 支持**: 支持容器化部署。

## 🛠️ 环境要求

- [Windows](https://www.microsoft.com/windows) (推荐) 或 Linux/macOS
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PowerShell 7+ (推荐)

## 🚀 快速开始

### 1. 克隆项目

```bash
git clone https://github.com/yourusername/LeetcodeAutoBot.git
cd LeetcodeAutoBot
```

### 2. 运行项目

直接运行项目即可，首次运行会自动安装所需的 Playwright 浏览器内核。

```bash
dotnet run --project LeetcodeAutoBot
```

### 3. 首次登录说明

1.  **首次运行**时，程序会启动一个浏览器窗口。
2.  请在浏览器中 **手动输入账号密码登录** LeetCode。
3.  登录成功后，程序会自动捕获并保存 Cookie。
4.  **后续运行**将自动使用保存的 Cookie，无需再次手动登录，也无需配置账号密码。

## 📅 定时任务 (Windows)

本项目提供了一个 PowerShell 脚本 `Register-Task.ps1`，可以将机器人注册为 Windows 定时任务，实现全自动挂机。

1. **以管理员身份** 打开 PowerShell。
2. 进入项目根目录。
3. 运行注册脚本：

```powershell
.\Register-Task.ps1
```

**脚本说明:**
- 默认运行时间为每天 **09:00**。
- 你可以用文本编辑器打开 `Register-Task.ps1` 修改 `$Time` 变量来调整时间。
- 任务名称为 `LeetcodeAutoBot`，可以在 Windows "任务计划程序" 中查看和管理。

## 🐳 Docker 部署

如果你更喜欢使用 Docker：

1. **构建镜像**

```bash
docker build -t leetcode-autobot -f LeetcodeAutoBot/Dockerfile .
```

2. **运行容器**

```bash
docker run -d \
  -e LEETCODE__USERNAME="你的账号" \
  -e LEETCODE__PASSWORD="你的密码" \
  --name leetcode-bot \
  leetcode-autobot
```

## ⚠️ 免责声明

本项目仅供技术学习和交流使用。
- 请勿用于任何商业用途。
- 请勿进行恶意刷题或攻击 LeetCode 服务器。
- 使用本项目产生的任何账号风险（如封号等）由用户自行承担。

---
Made with ❤️ by fengb3
