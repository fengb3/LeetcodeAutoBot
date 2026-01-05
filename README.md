# LeetcodeAutoBot

这是一个自动 LeetCode (leetcode.cn) 做题机器人，旨在帮助用户自动完成每日一题，获取积分以兑换 LeetCode 周边礼品。

## ✨ 功能特性

- 🤖 **自动登录**: 支持 LeetCode 中国站 (leetcode.cn) 自动登录。
- 📅 **每日一题**: 自动识别并完成当天的每日一题。
- 🧩 **智能刷题**: 每日自动获取三道没有做过的题目并完成。
- 🍪 **Cookie 持久化**: 将登录状态保存至数据库，减少重复登录次数。

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

本项目通过环境变量 `CI` 来控制浏览器的运行模式：
- `CI=false` (默认): **有头模式**，浏览器窗口可见。用于首次登录。
- `CI=true`: **无头模式**，浏览器在后台运行。用于日常自动挂机。

#### 🟢 首次运行 (手动登录)

首次运行时，需要看到浏览器窗口以便手动输入账号密码。请使用默认配置运行：

```bash
# 默认配置 (CI=false)，会弹出浏览器窗口
dotnet run --project LeetcodeAutoBot
```

1.  程序启动后会自动打开一个浏览器窗口并跳转到 LeetCode。
2.  请在浏览器中 **手动输入账号密码登录**。
3.  登录成功后，程序会自动捕获 Cookie 并保存到数据库。
4.  看到控制台提示 "Cookie 保存成功" 后，即可关闭程序。

#### 🔵 后续运行 (自动挂机)

登录状态保存后，后续运行应使用 **无头模式**，以免打扰日常使用：

```bash
# CI 配置 (CI=true)，浏览器在后台静默运行
dotnet run --project LeetcodeAutoBot --launch-profile "LeetcodeAutoBot - CI"
```

程序会自动读取数据库中的 Cookie 进行登录并完成每日一题。

## 📅 定时任务

### Windows

本项目提供了一个 PowerShell 脚本 `Register-Task-Windows.ps1`，可以将机器人注册为 Windows 定时任务。

1. **以管理员身份** 打开 PowerShell。
2. 进入项目根目录。
3. 运行注册脚本：

```powershell
.\Register-Task-Windows.ps1
```

### macOS

本项目提供了一个 Shell 脚本 `register-task-mac.sh`，可以将机器人注册为 macOS Launch Agent。

1. 打开终端 (Terminal)。
2. 进入项目根目录。
3. 赋予脚本执行权限并运行：

```bash
chmod +x register-task-mac.sh
./register-task-mac.sh
```

**脚本说明:**
- 默认运行时间为每天 **09:00**。
- 任务名称为 `com.leetcodeautobot.daily`。
- 日志会输出到项目目录下的 `stdout.log` 和 `stderr.log`。

## 📄 开源协议 (License)

本项目采用 **非商业性教育许可 (Non-Commercial Educational License)**。

- ✅ **允许**: 个人学习、研究、非营利性教育使用。
- 🚫 **禁止**: 任何形式的商业用途（包括但不限于付费服务、售卖代码、集成到商业产品）。

详见 [LICENSE](LICENSE) 文件。

## ⚠️ 免责声明

本项目仅供技术学习和交流使用。
- 请勿用于任何商业用途。
- 请勿进行恶意刷题或攻击 LeetCode 服务器。
- 使用本项目产生的任何账号风险（如封号等）由用户自行承担。

---
Made with ❤️ by fengb3
