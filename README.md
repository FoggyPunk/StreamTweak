# StreamTweak
![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-blue.svg)
![Framework](https://img.shields.io/badge/Framework-.NET%208.0-purple.svg)
![Downloads](https://img.shields.io/github/downloads/foggypunk/StreamTweak/total?label=Downloads&color=orange)

**StreamTweak** is born to solve technical bottlenecks between Host and Client, offering an intelligent control center to manage your remote gaming PC with a single click.

## ✅ Compatibility
StreamTweak works seamlessly with [Moonlight](https://github.com/moonlight-stream/moonlight-qt), [Sunshine](https://github.com/LizardByte/Sunshine), [Apollo](https://github.com/ClassicOldSong/Apollo), [Vibeshine](https://github.com/Nonary/vibeshine), and [Vibepollo](https://github.com/Nonary/Vibepollo).

> ⚠️ **Browser Warning:** When downloading the installer, Edge or Chrome may show a security warning. This is a **false positive** caused by the lack of a paid code-signing certificate — common for open-source projects. Click **"Keep"** or **"Keep anyway"** to proceed. The source code is fully available here for inspection.

## ✨ What's New in Version 3.0.0 — The "Awareness Update"

- **Logs tab:** browse the last 10 speed-change sessions at a glance — trigger mode (Auto/Manual), duration, and original speed are all recorded automatically
- **Streaming app detection:** the Logs tab shows whether StreamTweak has correctly located the log folder of your streaming server (Sunshine, Apollo, Vibeshine or Vibepollo); click the path to open the folder directly in Explorer
- **About tab:** version info, link to the GitHub repository, GPL v3 license badge, and PayPal donation button in one dedicated panel
- **Tray Auto Mode toggle:** enable or disable Auto Streaming Mode directly from the system tray right-click menu — no need to open Settings
- **Tray speed readout:** the current link speed of the selected adapter is always visible in the tray context menu
- **Tray streaming status:** the context menu now shows whether a streaming session is currently active or inactive
- **Full UI revision:** centered layout across all panels, redesigned tab bar with Dark/Light Mode button integrated, improved spacing and visual balance throughout

## 📖 The Technical Story Behind This Project
This project was born out of a specific frustration in the cloud gaming community. When using game streaming software like **[Moonlight](https://github.com/moonlight-stream/moonlight-qt)** with **Sunshine** or **Apollo**, a known issue occurs if the host PC and the client have mismatched Ethernet link speeds.

Due to how UDP packet buffering works on network switches, this mismatch often leads to severe packet loss and "Slow connection to PC" errors. You can read more about this technical bottleneck on the **[Moonlight GitHub Issue #714](https://github.com/moonlight-stream/moonlight-qt/issues/714)** and in this highly discussed **[Reddit thread](https://www.reddit.com/r/MoonlightStreaming/comments/1m35zo7/fix_moonlight_streaming_issues_on_25gbps_lan_try/)**.

StreamTweak makes the workaround (throttling the Host PC's Ethernet adapter down to 1.0 Gbps) instantaneous and completely seamless — no interruptions, no prompts.

*Fun fact: This entire application, including the C# code, UI logic, and Inno Setup installer, was developed with the assistance of AI.*

## 🔥 Key Features
- **Settings Dashboard:** Sleek UI to manage physical adapters and speeds.
- **Logs Tab:** Session history for the last 10 speed changes — trigger mode, duration, and original speed recorded automatically.
- **Streaming App Detection:** StreamTweak locates the log folder of the active streaming server and lets you open it in Explorer with one click.
- **About Tab:** Version info, GitHub link, license badge, and donation button in a dedicated panel.
- **Smart Filtering:** Only shows real LAN adapters (no VPNs, Wi-Fi, or virtual adapters).
- **Driver Bypass:** Automatically detects and bypasses Realtek/Intel driver localization limitations.
- **Auto Streaming Mode:** Intelligently monitors Sunshine, Apollo, Vibeshine and Vibepollo logs for incoming Moonlight connections and auto-adjusts network speed. Works with any installation path.
- **Manual Streaming Control:** One-click activation to instantly throttle to 1Gbps with professional UI feedback.
- **Smart Notifications:** Non-intrusive on-screen alerts inform users before network changes occur.
- **Tray Control:** Right-click the tray icon to toggle Auto Mode, check current link speed, and monitor streaming session status — all without opening Settings.
- **Completely UAC-free:** A background Windows Service handles all privileged operations silently — no prompts during normal use.
- **Auto-Start:** Launches at Windows logon via a hidden Scheduled Task.

## 🏗️ Architecture

### UAC-free Speed Changes (v2.5.1+)
StreamTweak installs a lightweight background Windows Service (`StreamTweakService`) that runs as `LocalSystem` and listens on a Named Pipe. Whenever the app needs to change the network adapter speed — whether from Auto Mode, Manual Mode, or the tray menu — it sends a command through the pipe. The service executes the change with its existing elevated privileges. No UAC dialog ever appears.

```
StreamTweak.exe  ──(Named Pipe)──►  StreamTweakService  ──►  Network Adapter
 (unprivileged)                        (LocalSystem)
```

### Log Discovery (v2.5.2+)
StreamTweak automatically locates the streaming server log file at startup and re-checks every 10 seconds to handle late-appearing dynamic logs.

```
1. Registry lookup  →  InstallLocation from HKLM\SOFTWARE or Uninstall keys
2. Program Files scan  →  known names first (Vibepollo, Apollo, Vibeshine, Sunshine),
                          then broad scan of all subfolders
3. For each candidate folder  →  config\logs\sunshine-*.log (dynamic)
                               →  config\sunshine.log (static)
```

### Session Logging (v3.0.0+)
Every speed-change event — whether triggered automatically or manually — is recorded as a `SessionEntry` and persisted to `%LOCALAPPDATA%\StreamTweak\sessions.json`. The Logs tab reads this file and displays the last 10 sessions in real time.

```
SessionEntry {
  Id             →  short unique identifier
  StartTime      →  when the session began
  EndTime        →  when the original speed was restored (null if active)
  TriggerMode    →  "Auto" | "Manual"
  OriginalSpeed  →  the speed key that will be restored on session end
}
```

The same discovery pipeline used for log monitoring (`LogParser.FindStreamingAppInfo`) is surfaced in the Logs tab, so the user can verify at a glance which streaming server StreamTweak has detected and navigate directly to its log folder.

## 🎮 How It Works

### Auto Streaming Mode
1. Enable "Auto Streaming Mode" in the Settings
2. StreamTweak monitors your streaming server logs in real-time (Sunshine, Apollo, Vibeshine, Vibepollo)
3. When Moonlight connects from a client:
   - An on-screen alert appears (4-second delay for awareness)
   - Network speed automatically adjusts from current speed (e.g., 2.5Gbps) down to 1Gbps
4. **Important:** A brief disconnect will occur during the speed adjustment
   - You have **30 seconds to reconnect** in Moonlight
   - If you reconnect within this window, streaming continues normally
   - If no reconnection is detected within 30 seconds, the original speed is automatically restored
   - The inactivity timer prevents the app from reverting speed prematurely (avoiding reconnection loops)
5. When streaming ends, original speed is automatically restored

### Manual Streaming Mode
1. Click "Start Streaming Mode" button anytime
2. On-screen alert informs you of the network adjustment
3. Network throttles to 1Gbps immediately — no UAC prompt
4. Click "Stop Streaming Mode" to restore original speed

## 📝 Installation
1. Go to the **Releases** page of this repository.
2. Download the latest `StreamTweak_3.0.0_Installer.exe`
3. Run the installer and enjoy seamless streaming.

## 🙏 Support the Project
If this tool helped you fix your Moonlight stutters or made managing your PC easier, consider buying me a coffee! ☕

[![Donate with PayPal](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://paypal.me/foggypunk)

## 🤝 Acknowledgements
StreamTweak exists thanks to the outstanding work of the developers behind the tools it integrates with. A heartfelt thank you to:

- [**Moonlight**](https://github.com/moonlight-stream/moonlight-qt) — the open-source game streaming client that inspired this project and the community around it
- [**Sunshine**](https://github.com/LizardByte/Sunshine) — the game streaming host that started it all
- [**Apollo**](https://github.com/ClassicOldSong/Apollo) — the community-driven Sunshine fork
- [**Vibeshine**](https://github.com/Nonary/vibeshine) — the Sunshine fork with dynamic log support that drove the v2.5.2 compatibility work
- [**Vibepollo**](https://github.com/Nonary/Vibepollo) — the Apollo fork, also fully supported since v2.5.2

## License
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-green.svg)](https://www.gnu.org/licenses/gpl-3.0)
