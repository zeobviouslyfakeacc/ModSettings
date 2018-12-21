using System;
using Harmony;
using UnityEngine;

namespace ModSettings {
	internal static class ModSettingsPatches {

		private const int MOD_SETTINGS_ID = 0x4d53; // "MS" in hex

		[HarmonyPatch(typeof(Panel_OptionsMenu), "ConfigureMenu", new Type[0])]
		private static class AddModSettingsButton {
			private static void Postfix(Panel_OptionsMenu __instance) {
				if (!ModSettingsMenu.HasVisibleModSettings(isMainMenu: InterfaceManager.IsMainMenuActive()))
					return;

				BasicMenu basicMenu = (BasicMenu) AccessTools.Field(typeof(Panel_OptionsMenu), "m_BasicMenu").GetValue(__instance);
				if (basicMenu == null)
					return;

				int itemIndex = basicMenu.GetItemCount();
				basicMenu.AddItem("ModSettings", MOD_SETTINGS_ID, itemIndex, "Mod Settings", "Change the configuration of your mods", null, () => ShowModSettings(__instance));
			}

			private static void ShowModSettings(Panel_OptionsMenu __instance) {
				ModSettingsGUI settings = GetModSettingsGUI(__instance);
				settings.Enable(__instance);
			}
		}

		[HarmonyPatch(typeof(Panel_OptionsMenu), "Update", new Type[0])]
		private static class DoModSettingsTabUpdate {
			private static void Postfix(Panel_OptionsMenu __instance) {
				GameObject settingsTab = GetSettingsTab(__instance);
				if (!settingsTab.activeInHierarchy)
					return;

				if (InputManager.GetEscapePressed(__instance)) {
					__instance.OnCancel();
				}
			}
		}

		[HarmonyPatch(typeof(Panel_OptionsMenu), "MainMenuTabOnEnable", new Type[0])]
		private static class DisableModSettingsWhenBackPressed {
			private static void Prefix(Panel_OptionsMenu __instance) {
				GameObject settingsTab = GetSettingsTab(__instance);
				settingsTab.SetActive(false);
			}
		}

		[HarmonyPatch(typeof(Panel_OptionsMenu), "OnConfirmSettings", new Type[0])]
		private static class CallModConfirmWhenButtonPressed {
			private static bool Prefix(Panel_OptionsMenu __instance) {
				ModSettingsGUI gui = GetModSettingsGUI(__instance);
				if (!gui.gameObject.activeInHierarchy)
					return true;

				GameAudioManager.PlayGuiConfirm();
				gui.CallOnConfirm();

				return false;
			}
		}

		private static ModSettingsGUI GetModSettingsGUI(Panel_OptionsMenu panel) {
			Transform panelTransform = panel.transform.Find("Pages/ModSettings");
			return panelTransform.GetComponent<ModSettingsGUI>();
		}

		private static GameObject GetSettingsTab(Panel_OptionsMenu panel) {
			return panel.transform.Find("Pages/ModSettings").gameObject;
		}
	}
}
