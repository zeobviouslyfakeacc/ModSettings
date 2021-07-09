using ModSettings.AttributeUtils;
using System;
using System.Reflection;
using UnityEngine;
using static ModSettings.AttributeUtils.AttributeFieldTypes;

namespace ModSettings {
	public static class Slider {
		public static void AddSliderSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field) {
			AttributeScraper.GetAttributes<SliderAttribute>(field, out NameAttribute name, out DescriptionAttribute description, out SliderAttribute attr);
			if (attr != null)
				AddSliderSetting(guiBuilder, modSettings, field, name, description, attr);
			else if (AttributeFieldTypes.IsFloatType(field.FieldType))
				guiBuilder.AddSliderSetting(modSettings, field, name, description, SliderAttribute.DefaultFloatRange);
			else if (AttributeFieldTypes.IsIntegerType(field.FieldType))
				guiBuilder.AddSliderSetting(modSettings, field, name, description, SliderAttribute.DefaultIntRange);
		}
		public static void AddSliderSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, NameAttribute name, DescriptionAttribute description, SliderAttribute range) {
			AddSliderSetting(guiBuilder, modSettings, field, name?.Name, name?.Localize ?? false, description?.Description, description?.Localize ?? false, range.From, range.To, range.NumberOfSteps, range.NumberFormat);
		}
		public static void AddSliderSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText, float sliderFrom, float sliderTo) {
			AddSliderSetting(guiBuilder, modSettings, field, nameText, false, descriptionText, false, sliderFrom, sliderTo, -1, null);
		}
		public static void AddSliderSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText, float sliderFrom, float sliderTo, int numberOfSteps) {
			AddSliderSetting(guiBuilder, modSettings, field, nameText, false, descriptionText, false, sliderFrom, sliderTo, numberOfSteps, null);
		}
		public static void AddSliderSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, string descriptionText, float sliderFrom, float sliderTo, int numberOfSteps, string numberFormat) {
			AddSliderSetting(guiBuilder, modSettings, field, nameText, false, descriptionText, false, sliderFrom, sliderTo, numberOfSteps, numberFormat);
		}
		public static void AddSliderSetting(this GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, string nameText, bool nameLocalize,
			string descriptionText, bool descriptionLocalize, float sliderFrom, float sliderTo, int numberOfSteps, string numberFormat) {
			// Create menu
			GameObject setting = guiBuilder.CreateSetting(nameText, nameLocalize, descriptionText, descriptionLocalize, ObjectPrefabs.sliderPrefab, "Label_FOV");
			ConsoleSlider slider = setting.GetComponent<ConsoleSlider>();
			UILabel uiLabel = slider.m_SliderObject.GetComponentInChildren<UILabel>();
			UISlider uiSlider = slider.m_SliderObject.GetComponentInChildren<UISlider>();

			// Sanitize user values, especially if the field type is int
			bool isFloat = IsFloatType(field.FieldType);
			float from = isFloat ? sliderFrom : Mathf.Round(sliderFrom);
			float to = isFloat ? sliderTo : Mathf.Round(sliderTo);
			if (numberOfSteps < 0) {
				numberOfSteps = isFloat ? 1 : Mathf.RoundToInt(Mathf.Abs(from - to)) + 1;
			}
			if (string.IsNullOrEmpty(numberFormat)) {
				numberFormat = isFloat ? SliderAttribute.DefaultFloatFormat : SliderAttribute.DefaultIntFormat;
			}

			// Add listeners to update setting value
			EventDelegate.Callback callback = new Action(() => UpdateSliderValue(guiBuilder, modSettings, field, uiSlider, uiLabel, from, to, numberFormat));
			EventDelegate.Set(slider.onChange, callback);
			EventDelegate.Set(uiSlider.onChange, callback);
			modSettings.AddRefreshAction(() => UpdateSlider(modSettings, field, uiSlider, uiLabel, from, to, numberFormat));

			// Set default value and number of steps
			float defaultValue = Convert.ToSingle(field.GetValue(modSettings));
			uiSlider.value = (defaultValue - from) / (to - from);
			uiSlider.numberOfSteps = numberOfSteps;
			UpdateSliderLabel(field, uiLabel, defaultValue, numberFormat);

			// Control visibility
			guiBuilder.SetVisibilityListener(modSettings, field, setting, guiBuilder.lastHeader);
		}

		private static void UpdateSliderValue(GUIBuilder guiBuilder, ModSettingsBase modSettings, FieldInfo field, UISlider slider, UILabel label, float from, float to, string numberFormat) {
			float sliderValue = from + slider.value * (to - from);
			if (SliderMatchesField(modSettings, field, sliderValue)) return;
			if (IsIntegerType(field.FieldType)) {
				sliderValue = Mathf.Round(sliderValue);
			}

			UpdateSliderLabel(field, label, sliderValue, numberFormat);
			guiBuilder.SetSettingsField(modSettings, field, Convert.ChangeType(sliderValue, field.FieldType, null));

			if (modSettings.IsVisible() && slider.numberOfSteps > 1) {
				GameAudioManager.PlayGUISlider();
			}
		}

		private static void UpdateSlider(ModSettingsBase modSettings, FieldInfo field, UISlider slider, UILabel label, float from, float to, string numberFormat) {
			float sliderValue = from + slider.value * (to - from);
			if (SliderMatchesField(modSettings, field, sliderValue)) return;

			float value = Convert.ToSingle(field.GetValue(modSettings));
			slider.value = (value - from) / (to - from);
			UpdateSliderLabel(field, label, value, numberFormat);
		}

		private static bool SliderMatchesField(ModSettingsBase modSettings, FieldInfo field, float sliderValue) {
			if (IsFloatType(field.FieldType)) {
				float oldValue = Convert.ToSingle(field.GetValue(modSettings));
				return sliderValue == oldValue;
			} else {
				long oldValue = Convert.ToInt64(field.GetValue(modSettings));
				long longValue = (long)Mathf.Round(sliderValue);
				return oldValue == longValue;
			}
		}

		private static void UpdateSliderLabel(FieldInfo field, UILabel label, float value, string numberFormat) {
			if (IsFloatType(field.FieldType)) {
				label.text = string.Format(numberFormat, value);
			} else {
				long longValue = (long)Mathf.Round(value);
				label.text = string.Format(numberFormat, longValue);
			}
		}
	}
}
