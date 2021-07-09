using ModSettings.Extentions;
using System;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	public static class CustomChoice {
		public static void AddCustomChoiceSetting<T>(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, Func<T> leftFunc, Func<T> rightFunc) {
			AddCustomChoiceSetting<T>(guiBuilder, modSettings, field, nameText, false, null, false, leftFunc, rightFunc, null);
		}
		public static void AddCustomChoiceSetting<T>(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, Func<T> leftFunc, Func<T> rightFunc, Action postAction) {
			AddCustomChoiceSetting<T>(guiBuilder, modSettings, field, nameText, false, null, false, leftFunc, rightFunc, postAction);
		}
		public static void AddCustomChoiceSetting<T>(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText, Func<T> leftFunc, Func<T> rightFunc) {
			AddCustomChoiceSetting<T>(guiBuilder, modSettings, field, nameText, false, descriptionText, false, leftFunc, rightFunc, null);
		}
		public static void AddCustomChoiceSetting<T>(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText, Func<T> leftFunc, Func<T> rightFunc, Action postAction) {
			AddCustomChoiceSetting<T>(guiBuilder, modSettings, field, nameText, false, descriptionText, false, leftFunc, rightFunc, postAction);
		}
		public static void AddCustomChoiceSetting<T>(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize, Func<T> leftFunc, Func<T> rightFunc, Action postAction) {
			// Create menu item
			GameObject setting = guiBuilder.CreateSetting(nameText, nameLocalize, descriptionText, descriptionLocalize, ObjectPrefabs.customComboBoxPrefab, "Label");
			UILabel uiLabel = setting.GetChild("Label_Value")?.GetComponent<UILabel>();
			UIButton leftButton = setting.GetChild("Button_Decrease")?.GetComponent<UIButton>();
			UIButton rightButton = setting.GetChild("Button_Increase")?.GetComponent<UIButton>();

			// Add listener and set default value
			if (leftFunc != null) EventDelegate.Set(leftButton.onClick, new Action(() => SetFieldValue<T>(guiBuilder, modSettings, field, leftFunc, postAction, uiLabel)));
			if (rightFunc != null) EventDelegate.Set(rightButton.onClick, new Action(() => SetFieldValue<T>(guiBuilder, modSettings, field, rightFunc, postAction, uiLabel)));
			modSettings.AddRefreshAction(() => UpdateLabel(modSettings, field, uiLabel));
			UpdateLabel(modSettings, field, uiLabel);

			// Control visibility
			guiBuilder.SetVisibilityListener(modSettings, field, setting, guiBuilder.lastHeader);
		}

		private static void SetFieldValue<T>(GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, Func<T> call, Action postUpdate, UILabel uiLabel) {
			guiBuilder.SetSettingsField(modSettings, field, call.Invoke());
			UpdateLabel(modSettings, field, uiLabel);
			if (postUpdate != null) postUpdate.Invoke();
		}

		private static void UpdateLabel(ModSettingsBase modSettings, FieldInfo field, UILabel uiLabel) {
			string value = Convert.ToString(field.GetValue(modSettings));
			uiLabel.text = value ?? "";
		}
	}
}
