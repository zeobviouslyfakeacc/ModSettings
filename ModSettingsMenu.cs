using Il2Cpp;
using UnityEngine;

namespace ModSettings {
	internal static class ModSettingsMenu {

		private static readonly HashSet<ModSettingsBase> mainMenuSettings = new HashSet<ModSettingsBase>();
		private static readonly HashSet<ModSettingsBase> inGameSettings = new HashSet<ModSettingsBase>();
		private static readonly SortedDictionary<string, List<ModSettingsBase>> settingsByModName = new SortedDictionary<string, List<ModSettingsBase>>();

		private static ModSettingsGUI? modSettingsGUI = null;

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

			if (settingsByModName.TryGetValue(modName, out List<ModSettingsBase>? settingsList)) {
				settingsList.Add(modSettings);
			} else {
				settingsList = new List<ModSettingsBase> { modSettings };
				settingsByModName.Add(modName, settingsList);
			}
		}

		internal static void BuildGUI(Panel_OptionsMenu panel) {
			GameObject modSettingsTab = ModSettingsGUIBuilder.CreateModSettingsTab(panel);
			modSettingsGUI = modSettingsTab.AddComponent<ModSettingsGUI>();

			foreach (KeyValuePair<string, List<ModSettingsBase>> entry in settingsByModName) {
				ModSettingsGUIBuilder guiBuilder = new ModSettingsGUIBuilder(entry.Key, modSettingsGUI);
				foreach (ModSettingsBase modSettings in entry.Value) {
					guiBuilder.AddSettings(modSettings);
				}
			}
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
