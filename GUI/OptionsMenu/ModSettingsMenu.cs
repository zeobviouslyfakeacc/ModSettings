using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModSettings {
	internal static class ModSettingsMenu {

		private static readonly HashSet<ModSettingsBase> mainMenuSettings = new HashSet<ModSettingsBase>();
		private static readonly HashSet<ModSettingsBase> inGameSettings = new HashSet<ModSettingsBase>();
		private static readonly SortedDictionary<string, List<ModSettingsBase>> settingsByModName = new SortedDictionary<string, List<ModSettingsBase>>();

		private static ModSettingsGUI modSettingsGUI = null;

		internal static bool disableMovementInput;

		internal static void RegisterSettings(ModSettingsBase modSettings, string modName, MenuType menuType) {
			if (string.IsNullOrEmpty(modName)) {
				throw new ArgumentException("[ModSettings] Mod name must be a non-empty string", "modName");
			} else if (mainMenuSettings.Contains(modSettings) || inGameSettings.Contains(modSettings)) {
				throw new ArgumentException("[ModSettings] Cannot add the same settings object multiple times", "modSettings");
			} else if (modSettingsGUI != null) {
				throw new InvalidOperationException("[ModSettings] RegisterSettings called after the GUI has been built.\n"
						+ "Call this method before Panel_CustomXPSetup::Awake, preferably from your mod's OnLoad method");
			}

			if (menuType != MenuType.InGameOnly)
				mainMenuSettings.Add(modSettings);
			if (menuType != MenuType.MainMenuOnly)
				inGameSettings.Add(modSettings);

			if (settingsByModName.TryGetValue(modName, out List<ModSettingsBase> settingsList)) {
				settingsList.Add(modSettings);
			} else {
				settingsList = new List<ModSettingsBase> { modSettings };
				settingsByModName.Add(modName, settingsList);
			}
		}

		internal static void BuildGUI() {
			GameObject modSettingsTab = CreateModSettingsTab();
			modSettingsGUI = modSettingsTab.AddComponent<ModSettingsGUI>();
			modSettingsGUI.Build();

			foreach (KeyValuePair<string, List<ModSettingsBase>> entry in settingsByModName) {
				ModSettingsGUIBuilder guiBuilder = new ModSettingsGUIBuilder(entry.Key, modSettingsGUI);
				foreach (ModSettingsBase modSettings in entry.Value) {
					guiBuilder.AddSettings(modSettings);
				}
			}
		}

		internal static GameObject CreateModSettingsTab() {
			Panel_OptionsMenu panel = InterfaceManager.m_Panel_OptionsMenu;
			Transform pages = panel.transform.Find("Pages");
			GameObject tab = UnityEngine.Object.Instantiate(panel.m_QualityTab, pages);
			tab.name = "ModSettings";

			Transform titleLabel = tab.transform.Find("TitleDisplay/Label");
			UnityEngine.Object.Destroy(titleLabel.GetComponent<UILocalize>());
			titleLabel.GetComponent<UILabel>().text = "Mod Settings";

			panel.m_MainMenuItemTabs.Add(tab);
			panel.m_Tabs.Add(tab);

			return tab;
		}

		internal static void SetSettingsVisible(bool isMainMenu, bool visible) {
			HashSet<ModSettingsBase> settings = isMainMenu ? mainMenuSettings : inGameSettings;
			foreach (ModSettingsBase modSettings in settings) {
				modSettings.SetMenuVisible(visible);
			}
		}

		internal static bool HasVisibleModSettings(bool isMainMenu) {
			HashSet<ModSettingsBase> settings = isMainMenu ? mainMenuSettings : inGameSettings;
			foreach (ModSettingsBase modSettings in settings) {
				if (modSettings.IsUserVisible())
					return true;
			}
			return false;
		}
	}
}
