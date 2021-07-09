using ModSettings.AttributeUtils;
using System;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	public static class Choice {
		public static void AddChoiceSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field) {
			AttributeScraper.GetAttributes<ChoiceAttribute>(field, out NameAttribute name, out DescriptionAttribute description, out ChoiceAttribute attr);

			if (attr != null)
				AddChoiceSetting(guiBuilder, modSettings, field, name, description, attr);
			else if (field.FieldType.IsEnum)
				guiBuilder.AddChoiceSetting(modSettings, field, name, description, ChoiceAttribute.ForEnumType(field.FieldType));
			else if (field.FieldType == typeof(bool))
				guiBuilder.AddChoiceSetting(modSettings, field, name, description, ChoiceAttribute.YesNoAttribute);
		}
		public static void AddChoiceSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, ChoiceAttribute choice) {
			AddChoiceSetting(guiBuilder, modSettings, field, name?.Name, name?.Localize ?? false, description?.Description, description?.Localize ?? false, choice.Names, choice.Localize);
		}
		public static void AddChoiceSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText, string[] choiceNames) {
			AddChoiceSetting(guiBuilder, modSettings, field, nameText, false, descriptionText, false, choiceNames, false);
		}
		public static void AddChoiceSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string[] choiceNames) {
			AddChoiceSetting(guiBuilder, modSettings, field, nameText, false, null, false, choiceNames, false);
		}
		public static void AddChoiceSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize, string[] choiceNames, bool choiceLocalize) {
			// Create menu item
			GameObject setting = guiBuilder.CreateSetting(nameText, nameLocalize, descriptionText, descriptionLocalize, ObjectPrefabs.comboBoxPrefab, "Label");
			ConsoleComboBox comboBox = setting.GetComponent<ConsoleComboBox>();

			// Add selectable values
			comboBox.items.Clear();
			foreach (string choiceName in choiceNames) {
				comboBox.items.Add(choiceName);
			}
			comboBox.m_Localize = choiceLocalize;

			// Add listener and set default value
			EventDelegate.Set(comboBox.onChange, new Action(() => UpdateChoiceValue(guiBuilder, modSettings, field, comboBox.GetCurrentIndex())));
			modSettings.AddRefreshAction(() => UpdateChoiceComboBox(modSettings, field, comboBox));

			// Control visibility
			guiBuilder.SetVisibilityListener(modSettings, field, setting, guiBuilder.lastHeader);
		}

		private static void UpdateChoiceValue(GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, int selectedIndex) {
			Type fieldType = field.FieldType;
			if (fieldType.IsEnum) {
				fieldType = Enum.GetUnderlyingType(fieldType);
			}

			guiBuilder.SetSettingsField(modSettings, field, Convert.ChangeType(selectedIndex, fieldType, null));
		}

		private static void UpdateChoiceComboBox(ModSettingsBase modSettings, FieldInfo field, ConsoleComboBox comboBox) {
			int value = Convert.ToInt32(field.GetValue(modSettings));
			comboBox.value = comboBox.items[value];
		}
	}
}
