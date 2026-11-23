# ShellIcons.Maui check app

A one-page MAUI app for checking `ShellIcons.Maui` on a phone, emulator or simulator ([#4](https://github.com/shellui-dev/shell-icons/issues/4)). It references the package project, so it shows the current source.

The page covers:

- sizes 16, 24 and 48, and fine detail (`Fingerprint`, `Settings`) at 16
- icon color from a `DynamicResource`, swapped when the system theme changes
- the dispatcher, fixed colors, arcs and filled shapes
- former names (`Trash2`, `Filter`, `CircleHelp`)
- stroke bounds at 64 px
- a tap on an icon inside a `Border` with a `TapGestureRecognizer`: the label changes to "Tapped 1"

## iOS Simulator

The [MAUI iOS check](../../.github/workflows/maui-ios.yml) workflow runs this on a GitHub macOS runner. It builds the app, then `run-ios-simulator.sh` takes light and dark screenshots and taps the icon. The screenshots are in the run's `maui-ios-screenshots` artifact.

On a Mac:

```bash
dotnet build samples/ShellIcons.Maui.Check -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64
bash samples/ShellIcons.Maui.Check/run-ios-simulator.sh samples/ShellIcons.Maui.Check/bin/Debug/net10.0-ios/iossimulator-arm64/ShellIcons.Maui.Check.app
```

## Android

```bash
dotnet build samples/ShellIcons.Maui.Check -f net10.0-android -t:Run
```

with an emulator running or a phone connected.
