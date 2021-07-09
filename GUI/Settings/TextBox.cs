using ModSettings.AttributeUtils;
using ModSettings.Extentions;
using ModSettings.Scripts;
using System;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	public static class TextBox {
		public static void AddTextBoxSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field) {
			AttributeScraper.GetAttributes<TextBoxAttribute>(field, out NameAttribute name, out DescriptionAttribute description, out TextBoxAttribute attr);
			AddTextBoxSetting(guiBuilder, modSettings, field, name, description);
		}
		public static void AddTextBoxSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description) {
			AddTextBoxSetting(guiBuilder, modSettings, field, name?.Name, name?.Localize ?? false, description?.Description, description?.Localize ?? false);
		}
		public static void AddTextBoxSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText) {
			AddTextBoxSetting(guiBuilder, modSettings, field, nameText, false, null, false);
		}
		public static void AddTextBoxSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText) {
			AddTextBoxSetting(guiBuilder, modSettings, field, nameText, false, descriptionText, false);
		}
		public static void AddTextBoxSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize) {
			GameObject setting = guiBuilder.CreateSetting(nameText, nameLocalize, descriptionText, descriptionLocalize, ObjectPrefabs.textEntryPrefab, "Label");

			GameObject textBoxObject = setting?.GetChild("Text_Box");

			UILabel uiLabel = textBoxObject?.GetComponent<UILabel>();
			TextInputField textInputField = textBoxObject?.GetComponent<TextInputField>();
			UIInput uiInput = textBoxObject?.GetComponent<UIInput>();
			TextBoxHandler customInput = textBoxObject.AddComponent<TextBoxHandler>();
			UIButton uiButton = setting?.GetComponent<UIButton>();


			customInput.onDeselect = new Action(() => SetFieldValue(guiBuilder, modSettings, field, textInputField.GetText()));
			EventDelegate.Set(uiButton.onClick, new System.Action(() => SelectTextBox(modSettings, field, uiInput, textInputField)));
			modSettings.AddRefreshAction(() => UpdateLabel(modSettings, field, uiInput));
			UpdateLabel(modSettings, field, uiInput);

			// Control visibility
			guiBuilder.SetVisibilityListener(modSettings, field, setting, guiBuilder.lastHeader);
		}

		private static void SelectTextBox(ModSettingsBase modSettings, FieldInfo field, UIInput uiInput, TextInputField textInputField) {
			if (!uiInput.isSelected) {
				UpdateLabel(modSettings, field, uiInput);
				textInputField.Select();
			}
		}

		private static void UpdateLabel(ModSettingsBase modSettings, FieldInfo field, UIInput uiInput) {
			string value = Convert.ToString(field.GetValue(modSettings));
			uiInput.value = value ?? "";
		}

		private static void SetFieldValue(GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string value) {
			guiBuilder.SetSettingsField(modSettings, field, value);
		}
	}
}
