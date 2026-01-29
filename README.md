# 🦞 Moltbot Windows Hub

A Windows companion suite for [Moltbot](https://moltbot.com) - the AI-powered personal assistant.

## Projects

This monorepo contains three projects:

| Project | Description |
|---------|-------------|
| **Moltbot.Tray** | System tray application for quick access to Moltbot |
| **Moltbot.Shared** | Shared gateway client library |
| **Moltbot.CommandPalette** | PowerToys Command Palette extension |

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Windows 10/11
- PowerToys (for Command Palette extension)

### Build
```bash
dotnet build
```

### Run Tray App
```bash
dotnet run --project src/Moltbot.Tray
```

## 📦 Moltbot.Tray

Windows system tray companion that connects to your local Moltbot gateway.

### Features
- 🦞 Lobster icon in system tray (connected/disconnected states)
- 💬 Quick Send - Send messages via global hotkey (Ctrl+Alt+Shift+C)
- 🔄 Auto-updates from GitHub Releases
- 🌐 Web Chat - Embedded chat window
- 📊 Status Display - View sessions and channels
- 🔔 Toast Notifications - Clickable Windows notifications
- 🚀 Auto-start with Windows
- ⚙️ Settings management

### Mac Parity Status

| Feature | Mac | Windows |
|---------|-----|---------|
| System tray icon | ✅ | ✅ |
| Connection status | ✅ | ✅ |
| Quick send hotkey | ✅ | ✅ |
| Web chat window | ✅ | ✅ |
| Toast notifications | ✅ | ✅ |
| Auto-start | ✅ | ✅ |
| Session display | ✅ | ✅ |
| Channel health | ✅ | ✅ |
| Deep links | ✅ | 🔄 |

## 📦 Moltbot.CommandPalette

PowerToys Command Palette extension for quick Moltbot access.

### Commands
- **🦞 Open Dashboard** - Launch web dashboard
- **💬 Quick Send** - Send a message
- **📊 Full Status** - View gateway status
- **⚡ Sessions** - View active sessions
- **📡 Channels** - View channel health
- **🔄 Health Check** - Trigger health refresh

### Installation
1. Build the solution in Release mode
2. Deploy the MSIX package via Visual Studio
3. Open Command Palette (Win+Alt+Space)
4. Type "Moltbot" to see commands

## 📦 Moltbot.Shared

Shared library containing:
- `MoltbotGatewayClient` - WebSocket client for gateway protocol
- `IMoltbotLogger` - Logging interface
- Data models (SessionInfo, ChannelHealth, etc.)

## Development

### Project Structure
```
moltbot-windows-hub/
├── src/
│   ├── Moltbot.Shared/           # Shared gateway library
│   ├── Moltbot.Tray/             # System tray app
│   └── Moltbot.CommandPalette/   # PowerToys extension
├── moltbot-windows-hub.sln
├── README.md
├── LICENSE
└── .gitignore
```

### Configuration

Settings are stored in:
- Settings: `%APPDATA%\MoltbotTray\settings.json`
- Logs: `%LOCALAPPDATA%\MoltbotTray\moltbot-tray.log`

Default gateway: `ws://localhost:18789`

## License

MIT License - see [LICENSE](LICENSE)
