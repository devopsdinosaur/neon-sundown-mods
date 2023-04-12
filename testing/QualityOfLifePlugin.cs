using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using TMPro;


[BepInPlugin("devopsdinosaur.neon_sundown.quality_of_life", "Quality of Life", "0.0.1")]
public class ActionSpeedPlugin : BaseUnityPlugin {

	private Harmony m_harmony = new Harmony("devopsdinosaur.neon_sundown.quality_of_life");
	public static ManualLogSource logger;

	private static ConfigEntry<bool> m_enabled;
	private static ConfigEntry<bool> m_invincibility;
	private static ConfigEntry<int> m_num_redraws;
	private static ConfigEntry<int> m_num_burns;
	private static ConfigEntry<float> m_xp_multiplier;
	private static ConfigEntry<bool> m_xp_healing;
	private static ConfigEntry<float> m_speed_multiplier;
	private static ConfigEntry<float> m_health_multiplier;
	
	private void Awake() {
		logger = this.Logger;
		try {
			m_enabled = this.Config.Bind<bool>("General", "Enabled", true, "Set to false to disable this mod.");
			m_invincibility = this.Config.Bind<bool>("General", "Invincibility", false, "Set to true to make your ship invincible.");
			m_num_redraws = this.Config.Bind<int>("General", "Redraws", 4, "Number of available card redraws (int, 0 - infinity)");
			m_num_burns = this.Config.Bind<int>("General", "Burns", 1, "Number of available card burns (int, 0 - infinity)");
			m_xp_multiplier = this.Config.Bind<float>("General", "XP Multiplier", 1f, "Multiplier applied to XP gains (float, applied after card multipliers).");
			m_xp_healing = this.Config.Bind<bool>("General", "XP Healing", false, "Set to true to enable XP healing (with or without the synergy).");
			m_speed_multiplier = this.Config.Bind<float>("General", "Speed Multiplier", 1f, "Multiplier applied to ship movement and dash speed (float, note that this is applied at the beginning of the arena/game and changes in ConfigManager UI will not take effect until the next game start).");
			m_health_multiplier = this.Config.Bind<float>("General", "Health Multiplier", 1f, "Multiplier applied to ship health (float, note that this is applied at the beginning of the arena/game and changes in ConfigManager UI will not take effect until the next game start).");
			if (m_enabled.Value) {
				this.m_harmony.PatchAll();
			}
			logger.LogInfo("devopsdinosaur.neon_sundown.quality_of_life v0.0.1 " + (!m_enabled.Value ? "(disabled by configuration option) " : " ") + "loaded.");
		} catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

	[HarmonyPatch(typeof(Enemy), "OnHitPlayer")]
	class HarmonyPatch_Enemy_OnHitPlayer {

		private static bool Prefix(Enemy __instance, bool ___seeded, bool ___immune) {
			try {
				if (!(m_enabled.Value && m_invincibility.Value)) {
					return true;
				}
				if (___seeded || ___immune) {
					return false;
				}
				if (__instance.dieOnCollision) {
					__instance.Destroy();
				}
				return false;
			} catch (Exception e) {
				logger.LogError("Enemy.OnHitPlayer_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(SaveSystem), "GetRedraws")]
	class HarmonyPatch_SaveSystem_GetRedraws {

		private static bool Prefix(ref int __result) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				__result = m_num_redraws.Value;
				return false;
			} catch (Exception e) {
				logger.LogError("SaveSystem.GetRedraws_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Dealer), "Start")]
	class HarmonyPatch_Dealer_OpenDealer {

		private static bool Prefix(ref int ___burnsLeft, TextMeshProUGUI ___burns) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				___burns.text = (___burnsLeft = m_num_burns.Value) + " REMAINING";
				return true;
			} catch (Exception e) {
				logger.LogError("Dealer.Start_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(XPReceiver), "AddXP")]
	class HarmonyPatch_XPReceiver_AddXP {

		private static bool Prefix(ref float amount, ref bool ___xpHealing) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				amount *= m_xp_multiplier.Value;
				if (m_xp_healing.Value) {
					___xpHealing = true;
				}
				return true;
			} catch (Exception e) {
				logger.LogError("XPReceiver.AddXP_Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Ship), "Setup")]
	class HarmonyPatch_Ship_Setup {

		private static bool Prefix(Ship __instance, ShipData data) {
			try {
				if (!m_enabled.Value) {
					return true;
				}
				data.playerSpeed *= m_speed_multiplier.Value;
				data.dashSpeed *= m_speed_multiplier.Value;
				data.startingHealth *= m_health_multiplier.Value;
				return true;
			} catch (Exception e) {
				logger.LogError("Ship.Setup_Prefix ERROR - " + e);
			}
			return true;
		}
	}
}