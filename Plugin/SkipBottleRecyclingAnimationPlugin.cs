using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;

namespace SkipBottleRecyclingAnimationMod
{
	[BepInPlugin("JuanBerta.Obenseuer.SkipBottleRecyclingAnimation", "Skip Bottle Recycling Animation", "1.0.0")]
	public class SkipBottleRecyclingAnimationPlugin : BaseUnityPlugin
	{
		internal static ConfigEntry<bool> SkipAnimation;

		private void Awake()
		{
			SkipAnimation = Config.Bind("General", "SkipAnimation", true, "Skip the slow per-coin payout animation and pay the full bottle recycling amount instantly.");
			Harmony.CreateAndPatchAll(typeof(BottleRecyclingPanel_RemoveMoney_Patch));
			Logger.LogInfo("Skip Bottle Recycling Animation loaded.");
		}

		// The vanilla RemoveMoney() starts StartGiveMoney(), a coroutine that pays the player
		// ONE mark every 0.1 seconds (Money.AddMoney(1f) + bottleRecyclingMoney--).
		// This prefix replaces the whole coroutine with a single instant payout.
		[HarmonyPatch(typeof(BottleRecyclingPanel), "RemoveMoney")]
		internal static class BottleRecyclingPanel_RemoveMoney_Patch
		{
			internal static bool Prefix(BottleRecyclingPanel __instance)
			{
				if (!SkipAnimation.Value)
				{
					return true;
				}
				if (__instance == null)
				{
					return false;
				}

				// Stop any payout coroutine that is still running (same as vanilla RemoveMoney does).
				Coroutine running = Traverse.Create(__instance).Field("currentCoroutine").GetValue<Coroutine>();
				if (running != null)
				{
					__instance.StopCoroutine(running);
				}

				BottleRecycling machine = __instance.currentBottleRecycling;
				if (machine == null)
				{
					return false;
				}
				int total = machine.bottleRecyclingMoney;
				if (total <= 0)
				{
					return false;
				}

				if (__instance.recyclingPanelUI != null && __instance.recyclingPanelUI.audioSourceMoneyStart != null)
				{
					__instance.recyclingPanelUI.audioSourceMoneyStart.Play();
				}
				if (machine.onGiveMoney != null)
				{
					machine.onGiveMoney.triggerOutputs();
				}
				if (Money.instance != null)
				{
					Money.instance.AddMoney(total, false);
				}
				machine.bottleRecyclingMoney = 0;

				if (__instance.recyclingPanelUI != null)
				{
					__instance.recyclingPanelUI.bottleWorthText.text = "000000";
					__instance.recyclingPanelUI.bottleSumText.text = "000000";
				}
				return false; // skip the original slow coroutine
			}
		}
	}
}
