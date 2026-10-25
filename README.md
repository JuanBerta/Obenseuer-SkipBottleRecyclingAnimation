# Obenseuer-SkipBottleRecyclingAnimation

BepInEx 6 mod for **Obenseuer** that skips the slow per-coin payout animation of the bottle recycling machine and pays the **full amount instantly**.

## Features

- Clicking **Get coins** now pays `bottleRecyclingMoney` in one lump sum.
- Keeps the vanilla sound, the `onGiveMoney` relay trigger and the UI refresh (`000000`).

## How it works

Vanilla: `BottleRecyclingPanel.GiveCoins()` → `RemoveMoney()` → `StartCoroutine(StartGiveMoney())`. `StartGiveMoney()` pays **1 mark every 0.1 s** (`Money.AddMoney(1f, false)`, `bottleRecyclingMoney--`, `MoneyToMachine(false)`), which is very slow for big amounts.

This mod Harmony-patches the **private `BottleRecyclingPanel.RemoveMoney()`** with a prefix that:

1. stops any running payout coroutine (same as vanilla `RemoveMoney`),
2. plays `audioSourceMoneyStart`,
3. triggers `currentBottleRecycling.onGiveMoney.triggerOutputs()`,
4. calls `Money.instance.AddMoney(total, false)` once,
5. sets `bottleRecyclingMoney = 0` and updates the panel texts to `000000`,
6. returns `false` to skip the original slow coroutine entirely.

## Installation

1. Install [BepInEx 6](https://github.com/BepInEx/BepInEx) into the game folder.
2. Drop `Obenseuer-SkipBottleRecyclingAnimation.dll` into `BepInEx/plugins`.
3. Launch the game.

## Configuration

| Config | Section | Default | Description |
| --- | --- | --- | --- |
| `SkipAnimation` | General | `true` | Master toggle for instant payout. Set to `false` to keep the vanilla slow animation. |

Config file: `BepInEx/config/JuanBerta.Obenseuer.SkipBottleRecyclingAnimation.cfg`

## Build

```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Obenseuer-SkipBottleRecyclingAnimation.csproj /p:Configuration=Release /p:Platform=x64
```

Output: `bin\Release\Obenseuer-SkipBottleRecyclingAnimation.dll`
