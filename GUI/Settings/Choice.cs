using ModSettings.AttributeUtils;
using System;
using System.Reflection;
using UnityEngine;

namespace ModSettings {
	public static class ChoiceExtensions {
		public static Choice AddChoiceSetting(this VirtualGUIBuilder guiBuilder, string nameText, string descriptionText, string[] choiceNames) {
			return AddChoiceSetting(guiBuilder, nameText, false, descriptionText, false, choiceNames, false);
		}
		public static Choice AddChoiceSetting(this VirtualGUIBuilder guiBuilder, string nameText, string[] choiceNames) {
			return AddChoiceSetting(guiBuilder, nameText, false, null, false, choiceNames, false);
		}
		public static Choice AddChoiceSetting(this VirtualGUIBuilder guiBuilder, string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize, string[] choiceNames, bool choiceLocalize) {
			Choice c = new Choice(nameText, nameLocalize, descriptionText, descriptionLocalize, choiceNames, choiceLocalize);
			guiBuilder.AddSetting(c);
			return c;
		}

		internal static void AddChoiceSetting(this VirtualGUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field) {
			AttributeScraper.GetAttributes(field, out NameAttribute name, out DescriptionAttribute description, out ChoiceAttribute attr);
			Choice choice;

			if (attr != null) {
				choice = guiBuilder.AddChoiceSetting(name, description, attr);
			} else if (field.FieldType.IsEnum) {
				choice = guiBuilder.AddChoiceSetting(name, description, ChoiceAttribute.ForEnumType(field.FieldType));
			} else if (field.FieldType == typeof(bool)) {
				choice = guiBuilder.AddChoiceSetting(name, description, ChoiceAttribute.YesNoAttribute);
			} else {
				throw new InvalidOperationException("Invalid choice state - should have been caught during verification");
			}

			choice.OnChange = new Action(() => UpdateChoiceValue(guiBuilder, modSettings, field, choice.SelectedIndex));
			modSettings.AddRefreshAction(() => choice.SelectedIndex = Convert.ToInt32(field.GetValue(modSettings)));
		}

		internal static Choice AddChoiceSetting(this VirtualGUIBuilder guiBuilder, NameAttribute name, DescriptionAttribute description, ChoiceAttribute choice) {
			return AddChoiceSetting(guiBuilder, name?.Name, name?.Localize ?? false, description?.Description, description?.Localize ?? false, choice.Names, choice.Localize);
		}

		private static void UpdateChoiceValue(GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, int selectedIndex) {
			Type fieldType = field.FieldType;
			if (fieldType.IsEnum) {
				fieldType = Enum.GetUnderlyingType(fieldType);
			}

			guiBuilder.SetSettingsField(modSettings, field, Convert.ChangeType(selectedIndex, fieldType, null));
		}
	}

	public class Choice : Setting {

		private string[] choiceNames;
		private bool choiceLocalize;

		private ConsoleComboBox comboBox;
		private int selectedIndex;

		public int SelectedIndex {
			get { return selectedIndex; }
			set {
				selectedIndex = value;
				if (IsBuilt) comboBox.value = comboBox.items[value];
			}
		}

		public Choice(string nameText, bool nameLocalize, string descriptionText, bool descriptionLocalize, string[] choiceNames, bool choiceLocalize) {
			NameText = nameText;
			NameLocalize = nameLocalize;
			DescriptionText = descriptionText;
			DescriptionLocalize = descriptionLocalize;
			this.choiceNames = choiceNames;
			this.choiceLocalize = choiceLocalize;
		}

		protected override void DoBuild(GUIBuilder guiBuilder) {
			// Create menu item
			GameObject setting = guiBuilder.CreateSetting(NameText, NameLocalize, DescriptionText, DescriptionLocalize, ObjectPrefabs.comboBoxPrefab, "Label");
			comboBox = setting.GetComponent<ConsoleComboBox>();

			// Add selectable values
			comboBox.items.Clear();
			foreach (string choiceName in choiceNames) {
				comboBox.items.Add(choiceName);
			}
			comboBox.m_Localize = choiceLocalize;

			// Add listener and set default value
			EventDelegate.Set(comboBox.onChange, new Action(CallOnChange));

			// Control visibility
			guiBuilder.SetVisibilityListener(modSettings, field, setting, guiBuilder.lastHeader);
		}

		private void CallOnChange() {
			int oldSelectedIndex = selectedIndex;
			selectedIndex = comboBox.GetCurrentIndex();

			if (oldSelectedIndex != selectedIndex) {
				OnChange();
			}
		}
	}
}
