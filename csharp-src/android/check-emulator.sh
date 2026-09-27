#!/usr/bin/env bash
set -euo pipefail
adb install csharp-src/android/app/build/outputs/apk/debug/app-debug.apk
adb logcat -c
adb shell am start -n org.mobileforge.samplegame/.MainActivity
for ((i=0;i<40;i++)); do
  if adb logcat -d -s MobileForge:I | grep -q GAME_READY; then
    echo 'C# game rendered in WebView'
    exit 0
  fi
  if adb logcat -d -s MobileForge:E | grep -q GAME_ERROR; then
    adb logcat -d -s MobileForge:D
    exit 1
  fi
  sleep 3
done
adb logcat -d -s MobileForge:D
exit 1
