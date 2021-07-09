using ModSettings.AttributeUtils;
using ModSettings.Scripts;
using System;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	public static class KeyBinding {
		public static void AddKeySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field) {
			AttributeScraper.GetAttributes(field, out NameAttribute name, out DescriptionAttribute description);
			AddKeySetting(guiBuilder, modSettings, field, name, description);
		}
		public static void AddKeySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description) {
			AddKeySetting(guiBuilder, modSettings, field, name?.Name, name?.Localize ?? false, description?.Description, description?.Localize ?? false);
		}
		public static void AddKeySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText) {
			AddKeySetting(guiBuilder, modSettings, field, nameText, false, null, false);
		}
		public static void AddKeySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText) {
			AddKeySetting(guiBuilder, modSettings, field, nameText, false, descriptionText, false);
		}
		public static void AddKeySetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize) {
			// Create menu item
			GameObject setting = guiBuilder.CreateSetting(nameText ?? "", nameLocalize, descriptionText ?? "", descriptionLocalize, ObjectPrefabs.keyEntryPrefab, "Label");
			GameObject keyButtonObject = setting.transform.FindChild("Keybinding_Button").gameObject;

			CustomKeybinding customKeybinding = setting.AddComponent<CustomKeybinding>();
			customKeybinding.keyRebindingButton = keyButtonObject.GetComponent<KeyRebindingButton>();
			customKeybinding.currentKeycodeSetting = (KeyCode)field.GetValue(modSettings);
			customKeybinding.RefreshLabelValue();

			UIButton uiButton = keyButtonObject.GetComponent<UIButton>();
			EventDelegate.Set(uiButton.onClick, new Action(customKeybinding.OnClick));
			customKeybinding.OnChange = new Action(() => UpdateKeyValue(guiBuilder, modSettings, field, customKeybinding));
			modSettings.AddRefreshAction(() => UpdateKeyChoice(modSettings, field, customKeybinding));

			// Control visibility
			guiBuilder.SetVisibilityListener(modSettings, field, setting, guiBuilder.lastHeader);
		}

		private static void UpdateKeyValue(GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, CustomKeybinding customKeybinding) {
			guiBuilder.SetSettingsField(modSettings, field, customKeybinding.currentKeycodeSetting);
		}

		private static void UpdateKeyChoice(ModSettingsBase modSettings, FieldInfo field, CustomKeybinding customKeybinding) {
			KeyCode keyCode = (KeyCode)field.GetValue(modSettings);
			customKeybinding.currentKeycodeSetting = keyCode;
			customKeybinding.keyRebindingButton.SetValueLabel(keyCode.ToString());
		}
	}
}
