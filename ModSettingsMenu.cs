using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModSettings {
	internal static class ModSettingsMenu {

		private static readonly HashSet<ModSettingsBase> settings = new HashSet<ModSettingsBase>();
		private static readonly SortedDictionary<string, List<ModSettingsBase>> settingsByModName = new SortedDictionary<string, List<ModSettingsBase>>();
		private static readonly Dictionary<ModSettingsBase, MenuType> menuTypes = new Dictionary<ModSettingsBase, MenuType>();
		private static bool guiBuilt = false;

		public static void RegisterSettings(ModSettingsBase modSettings, string modName, MenuType menuType) {
			if (modSettings == null) {
				throw new ArgumentNullException("modSettings");
			} else if (string.IsNullOrEmpty(modName)) {
				throw new ArgumentException("[ModSettings] Mod name must be a non-empty string", "modName");
			} else if (settings.Contains(modSettings)) {
				throw new ArgumentException("[ModSettings] Cannot add the same settings object multiple times", "modSettings");
			} else if (guiBuilt) {
				throw new InvalidOperationException("[ModSettings] RegisterSettings called after the GUI has been built.\n"
						+ "Call this method before Panel_CustomXPSetup::Awake, preferably from your mod's OnLoad method");
			}

			settings.Add(modSettings);
			menuTypes.Add(modSettings, menuType);
			if (settingsByModName.TryGetValue(modName, out List<ModSettingsBase> settingsList)) {
				settingsList.Add(modSettings);
			} else {
				settingsList = new List<ModSettingsBase> { modSettings };
				settingsByModName.Add(modName, settingsList);
			}
		}

		internal static void BuildGUI() {
			guiBuilt = true;

			GameObject modSettingsTab = ModSettingsGUIBuilder.CreateModSettingsTab();
			ModSettingsGUI modSettingsGUI = modSettingsTab.AddComponent<ModSettingsGUI>();

			foreach (KeyValuePair<string, List<ModSettingsBase>> entry in settingsByModName) {
				ModSettingsGUIBuilder guiBuilder = new ModSettingsGUIBuilder(entry.Key, modSettingsGUI);
				foreach (ModSettingsBase modSettings in entry.Value) {
					guiBuilder.AddSettings(modSettings);
				}
			}
		}

		internal static void SetSettingsVisible(MenuType menuType) {
			foreach (KeyValuePair<ModSettingsBase, MenuType> entry in menuTypes) {
				entry.Key.OverrideVisible(entry.Value == MenuType.Both || entry.Value == menuType);
			}
		}

		internal static void SetAllSettingsInvisible() {
			foreach (ModSettingsBase setting in menuTypes.Keys) {
				setting.OverrideVisible(false);
			}
		}

		internal static bool HasVisibleModSettings() {
			foreach (ModSettingsBase modSettings in settings) {
				if (modSettings.IsVisible())
					return true;
			}
			return false;
		}
	}
}
