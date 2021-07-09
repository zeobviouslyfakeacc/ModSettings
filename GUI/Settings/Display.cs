using ModSettings.AttributeUtils;
using ModSettings.Extentions;
using ModSettings.Scripts;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	public static class Display {
		public static void AddDisplaySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field) {
			AttributeScraper.GetAttributes<DisplayAttribute>(field, out NameAttribute name, out DescriptionAttribute description, out DisplayAttribute attr);
			AddDisplaySetting(guiBuilder, modSettings, field, name, description);
		}
		public static void AddDisplaySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description) {
			AddDisplaySetting(guiBuilder, modSettings, field, name?.Name, name?.Localize ?? false, description?.Description, description?.Localize ?? false);
		}
		public static void AddDisplaySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText) {
			AddDisplaySetting(guiBuilder, modSettings, field, nameText, false, null, false);
		}
		public static void AddDisplaySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText) {
			AddDisplaySetting(guiBuilder, modSettings, field, nameText, false, descriptionText, false);
		}
		public static void AddDisplaySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize) {
			// Create menu item
			GameObject setting = guiBuilder.CreateSetting(nameText, nameLocalize, descriptionText, descriptionLocalize, ObjectPrefabs.displayPrefab, "Label");
			UILabel uiLabel = setting.GetChild("Label_Value")?.GetComponent<UILabel>();
			DisplayBoxHandler displayBox = setting.AddComponent<DisplayBoxHandler>();

			// Add listener and set default value
			displayBox.Initialize(modSettings, field, uiLabel);
			displayBox.UpdateLabel();
			modSettings.AddRefreshAction(() => displayBox.UpdateLabel());

			// Control visibility
			guiBuilder.SetVisibilityListener(modSettings, field, setting, guiBuilder.lastHeader);
		}
	}
}
