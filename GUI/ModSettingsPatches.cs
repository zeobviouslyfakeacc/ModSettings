using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace ModSettings {

	internal static class ModSettingsPatches {

		private const int MOD_SETTINGS_ID = 0x4d53; // "MS" in hex

		[HarmonyPatch(typeof(Panel_OptionsMenu), "InitializeAutosaveMenuItems", new Type[0])]
		private static class BuildModSettingsGUIPatch {
			private static void Postfix(Panel_OptionsMenu __instance) {
				InterfaceManager.m_Panel_OptionsMenu = __instance;
				ObjectPrefabs.Initialize(__instance);

				DateTime tStart = DateTime.UtcNow;

				try {
					MelonLogger.Msg("Building Mod Settings GUI");
					ModSettingsMenu.BuildGUI();
				} catch (Exception e) {
					MelonLogger.Error("Exception while building Mod Settings GUI\n" + e.ToString());
					return;
				}
				try {
					MelonLogger.Msg("Building Custom Mode GUI");
					CustomModeMenu.BuildGUI();
				} catch (Exception e) {
					MelonLogger.Error("Exception while building Custom Mode GUI\n" + e.ToString());
					return;
				}

				long timeMillis = (long) (DateTime.UtcNow - tStart).TotalMilliseconds;
				MelonLogger.Msg("Done! Took " + timeMillis + " ms. Have a nice day!");
			}
		}

		[HarmonyPatch(typeof(Panel_OptionsMenu), "ConfigureMenu", new Type[0])]
		private static class AddModSettingsButton {
			private static void Postfix(Panel_OptionsMenu __instance) {
				if (!ModSettingsMenu.HasVisibleModSettings(isMainMenu: InterfaceManager.IsMainMenuEnabled()))
					return;

				BasicMenu basicMenu = __instance.m_BasicMenu;
				if (basicMenu == null)
					return;

				AddAnotherMenuItem(basicMenu); // We need one more than they have...
				BasicMenu.BasicMenuItemModel firstItem = basicMenu.m_ItemModelList[0];
				int itemIndex = basicMenu.GetItemCount();
				basicMenu.AddItem("ModSettings", MOD_SETTINGS_ID, itemIndex, "Mod Settings", "Change the configuration of your mods", null,
						new Action(() => ShowModSettings(__instance)), firstItem.m_NormalTint, firstItem.m_HighlightTint);
			}

			private static void ShowModSettings(Panel_OptionsMenu __instance) {
				ModSettingsGUI settings = GetModSettingsGUI(__instance);
				settings.Enable(__instance);
			}

			private static void AddAnotherMenuItem(BasicMenu basicMenu) {
				GameObject gameObject = NGUITools.AddChild(basicMenu.m_MenuGrid.gameObject, basicMenu.m_BasicMenuItemPrefab);
				gameObject.name = "ModSettings MenuItem";
				BasicMenuItem item = gameObject.GetComponent<BasicMenuItem>();
				BasicMenu.BasicMenuItemView view = item.m_View;
				int itemIndex = basicMenu.m_MenuItems.Count;
				EventDelegate onClick = new EventDelegate(new Action(() => basicMenu.OnItemClicked(itemIndex)));
				view.m_Button.onClick.Add(onClick);
				EventDelegate onDoubleClick = new EventDelegate(new Action(() => basicMenu.OnItemDoubleClicked(itemIndex)));
				view.m_DoubleClickButton.m_OnDoubleClick.Add(onDoubleClick);
				basicMenu.m_MenuItems.Add(view);
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
