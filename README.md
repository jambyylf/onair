# ONAIR — iPhone & Android screen mirroring for Windows

<p align="center"><img src="logo.png" width="120" alt="ONAIR"></p>

A premium, dark-first Windows desktop app that mirrors **iPhone (AirPlay)** and
**Android (scrcpy)** screens — side by side, with recording, screenshots,
annotation, phone control and more. Single-file C# WinForms, no installer.

> UI is fully localized in **Kazakh / Russian / English**.

## Features

- 📱 **iPhone** (AirPlay via UxPlay) + 🤖 **Android** (scrcpy/adb) — **both at once**, embedded side by side in one window
- ＋ Wireless Android pairing (adb), multiple saved phones, auto-reconnect
- 📸 Screenshot · 🔄 Rotate (landscape) · ⛶ Fullscreen · 📼 Recording (`.mp4`) with timer
- ✏️ **Annotation** — capture the current frame, draw with markers, save as PNG (great for teaching/demos)
- 🎮 Android navigation: Back / Home / Recents / Power / Volume · ⓘ Phone info (battery, model, Android version, IP)
- ⚙️ Settings: language (KZ/RU/EN), theme (dark/light), quality (resolution/fps/bitrate), always-on-top, screen-off
- ONAIR brand design: gradient logo, pill buttons, ambient glow

## Tech

| Part | Stack |
|------|-------|
| App | Single-file **C# WinForms** (`ONAIR.cs`), compiled with .NET Framework `csc` (built into Windows). No external build tools. |
| iPhone | **UxPlay 1.74** (AirPlay) + **GStreamer** (`d3d11videosink` / `wasapisink`) |
| Android | **scrcpy 4.0** + **adb** |
| Packaging | Portable folder: the app exe + bundled UxPlay/GStreamer/scrcpy runtime (~270 MB), copy-and-run, no install |

The app embeds the mirror windows via Win32 `SetParent`, launches helpers with
`CreateNoWindow` (so there is no console), and ships as `WinExe`.

## Build

The app compiles with the .NET Framework C# compiler that ships with Windows — no SDK needed.

```bat
build.bat
```

This runs:

```
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe ^
  /nologo /codepage:65001 /target:winexe /out:ONAIR.exe /win32icon:onair.ico ^
  /reference:System.Windows.Forms.dll /reference:System.Drawing.dll ^
  /reference:Microsoft.VisualBasic.dll ONAIR.cs
```

`ONAIR.exe` expects this portable runtime layout next to it:

```
ONAIR.exe
onair.ico, logo.png
bin\                          uxplay.exe + gst-launch-1.0.exe + GStreamer DLLs
lib\gstreamer-1.0\            GStreamer plugins
libexec\gstreamer-1.0\gst-plugin-scanner.exe
scrcpy\                       scrcpy.exe + adb.exe
```

- Build the iPhone runtime in **MSYS2 UCRT64**: install `mingw-w64-ucrt-x86_64-{cmake,gcc,ninja,libplist,gstreamer,gst-plugins-base,gst-plugins-good,gst-plugins-bad,gst-libav,openssl}`, then build [UxPlay](https://github.com/FDH2/UxPlay) (`cmake -G Ninja .. && ninja`).
- `closure.sh` computes the minimal set of GStreamer DLLs to bundle (run it in MSYS2 UCRT64).
- Get `scrcpy` (with `adb`) from the [scrcpy releases](https://github.com/Genymobile/scrcpy).

## Usage (short)

- **iPhone** → press `▶ iPhone`, then on the phone: Screen Mirroring → ONAIR.
- **Android** → enable Wireless debugging, pair via `＋`, then press `▶ Android` (or just USB with USB-debugging).
- Both phones can be mirrored at the same time.

## Credits

Built on the excellent open-source projects [UxPlay](https://github.com/FDH2/UxPlay) (GPL),
[scrcpy](https://github.com/Genymobile/scrcpy) (Apache-2.0) and
[GStreamer](https://gstreamer.freedesktop.org/) (LGPL). The ONAIR app code in this repo
is a personal project.
