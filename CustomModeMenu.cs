using System;
using System.Collections.Generic;

namespace ModSettings {
	internal static class CustomModeMenu {

		private static readonly int numPositions = (Position.BelowAll.Index - Position.AboveAll.Index) + 1;
		private static readonly HashSet<ModSettingsBase> settings = new HashSet<ModSettingsBase>();
		private static readonly List<ModSettingsBase>[] settingsAtPosition = new List<ModSettingsBase>[numPositions];
		private static bool guiBuilt = false;

		static CustomModeMenu() {
			for (int i = 0; i < numPositions; ++i) {
				settingsAtPosition[i] = new List<ModSettingsBase>();
			}
		}

		internal static void RegisterSettings(ModSettingsBase modSettings, Position targetPosition) {
			if (targetPosition == null) {
				throw new ArgumentNullException("targetPosition");
			} else if (settings.Contains(modSettings)) {
				throw new ArgumentException("[ModSettings] Cannot add the same settings object multiple times", "modSettings");
			} else if (guiBuilt) {
				throw new InvalidOperationException("[ModSettings] RegisterSettings called after the GUI has been built.\n"
						+ "Call this method before Panel_CustomXPSetup::Awake, preferably from your mod's OnLoad method");
			}

			settings.Add(modSettings);
			settingsAtPosition[targetPosition.Index].Add(modSettings);
		}

		internal static void BuildGUI() {
			guiBuilt = true;
			CustomModeGUIBuilder guiBuilder = new CustomModeGUIBuilder(InterfaceManager.LoadPanel<Panel_CustomXPSetup>());

			for (int position = 0; position < numPositions; ++position) {
				List<ModSettingsBase> settingsAtCurrentPos = settingsAtPosition[position];

				foreach (ModSettingsBase modSettings in settingsAtCurrentPos) {
					guiBuilder.AddSettings(modSettings);
				}

				guiBuilder.NextSection();
			}

			guiBuilder.Finish();
		}

		internal static void SetSettingsVisible(bool enable) {
			foreach (ModSettingsBase setting in settings) {
				setting.SetMenuVisible(enable);
			}
		}

		internal static void CallOnConfirm() {
			foreach (ModSettingsBase settings in settings) {
				settings.CallOnConfirm();
			}
		}
	}
}
