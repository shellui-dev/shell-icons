#!/usr/bin/env bash
# Runs the check app on an iOS Simulator: screenshots in light and dark mode, then a tap on the icon.
# Usage: run-ios-simulator.sh path/to/ShellIcons.Maui.Check.app   (screenshots go to $OUT, default ./screenshots)
# The tap needs idb (pip install fb-idb, brew install facebook/fb/idb-companion); without it the tap is skipped.
set -euo pipefail

APP=${1:?path to ShellIcons.Maui.Check.app}
OUT=${OUT:-screenshots}
BUNDLE=dev.shellicons.check
mkdir -p "$OUT"

# The newest iOS runtime's "Pro" iPhone, or its first iPhone.
udid=$(xcrun simctl list devices available -j | python3 -c '
import json, sys
devices = json.load(sys.stdin)["devices"]
def version(key): return [int(p) for p in key.rsplit("iOS-", 1)[1].split("-")]
for runtime in sorted((k for k in devices if ".iOS-" in k), key=version, reverse=True):
    phones = [d for d in devices[runtime] if d["name"].startswith("iPhone")]
    if phones:
        pick = next((d for d in phones if d["name"].endswith(" Pro")), phones[0])
        print(pick["udid"]); sys.stderr.write(pick["name"] + " on " + runtime.rsplit(".", 1)[1] + "\n"); break
')
[ -n "$udid" ] || { echo "No iPhone simulator found" >&2; exit 1; }

xcrun simctl boot "$udid" 2>/dev/null || true
xcrun simctl bootstatus "$udid" -b
xcrun simctl ui "$udid" appearance light
xcrun simctl install "$udid" "$APP"
xcrun simctl launch "$udid" "$BUNDLE"
sleep 20
xcrun simctl io "$udid" screenshot "$OUT/1-light.png"

# The icons take their color from a DynamicResource the app swaps when the theme changes.
xcrun simctl ui "$udid" appearance dark
sleep 5
xcrun simctl io "$udid" screenshot "$OUT/2-dark.png"
xcrun simctl ui "$udid" appearance light
sleep 3

if ! command -v idb >/dev/null; then
  echo "idb not found: skipping the tap check"
  exit 0
fi

idb connect "$udid" >/dev/null
idb ui describe-all --udid "$udid" > "$OUT/ui-before.json"

# Tap the middle of the X icon (accessibility label "Close"); fall back to 24 points left of the label.
read -r x y < <(python3 - "$OUT/ui-before.json" <<'PY'
import json, sys
items = json.load(open(sys.argv[1]))
def find(pred): return next((i for i in items if pred(i)), None)
icon = find(lambda i: i.get("AXLabel") == "Close" or i.get("AXUniqueId") == "TapIcon")
if icon:
    f = icon["frame"]; print(round(f["x"] + f["width"] / 2), round(f["y"] + f["height"] / 2))
else:
    label = find(lambda i: i.get("AXUniqueId") == "TapLabel" or i.get("AXLabel") == "Not tapped")
    f = label["frame"]; print(round(f["x"] - 24), round(f["y"] + f["height"] / 2))
PY
)
echo "Tapping the icon at $x,$y"
idb ui tap --udid "$udid" "$x" "$y"
sleep 2
xcrun simctl io "$udid" screenshot "$OUT/3-tapped.png"
idb ui describe-all --udid "$udid" > "$OUT/ui-after.json"

if grep -q '"Tapped 1"' "$OUT/ui-after.json"; then
  echo "Tap reached the border"
else
  echo "Tap did not reach the border" >&2
  exit 1
fi
