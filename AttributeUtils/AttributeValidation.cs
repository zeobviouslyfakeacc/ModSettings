using System;
using System.Reflection;
using static ModSettings.AttributeUtils.AttributeFieldTypes;

namespace ModSettings.AttributeUtils {
	internal static class AttributeValidation {
		internal static void ValidateFields(ModSettingsBase modSettings) {
			foreach (FieldInfo field in modSettings.GetFields()) {
				ValidateFieldAttributes(modSettings, field);
			}
		}

		private static void ValidateFieldAttributes(ModSettingsBase modSettings, FieldInfo field) {
			if (AttributeScraper.HasAttribute<HideFromModSettingsAttribute>(field))
				return;

			AttributeScraper.GetAttributes(field, out SectionAttribute section, out NameAttribute name, out DescriptionAttribute description,
					out SliderAttribute slider, out ChoiceAttribute choice, out DisplayAttribute display, out TextBoxAttribute textBox);

			Type fieldType = field.FieldType;

			if (name == null) {
				throw new ArgumentException("[ModSettings] Mod settings contain field without a name attribute", field.Name);
			} else if (string.IsNullOrEmpty(name.Name)) {
				throw new ArgumentException("[ModSettings] Setting name attribute must have a non-empty value", field.Name);
			}

			ValidateAttributeCount(field);
			if (slider != null) {
				slider.ValidateFor(modSettings, field);
			} else if (choice != null) {
				choice.ValidateFor(modSettings, field);
			} else if (display != null) {
				return;//Display works on all types with any default values
			} else if (textBox != null) {
				textBox.ValidateFor(modSettings, field);
			} else if (!IsSupportedType(fieldType)) {
				throw new ArgumentException("[ModSettings] Field type " + fieldType.Name + " is not supported for that configuration", field.Name);
			}

			if (fieldType.IsEnum && fieldType != typeof(UnityEngine.KeyCode)) {
				ValidateEnum(field, fieldType);
			}
		}

		private static void ValidateEnum(FieldInfo field, Type enumType) {
			int count = Enum.GetValues(enumType).Length;
			Type underlyingType = Enum.GetUnderlyingType(enumType);

			for (int i = 0; i < count; ++i) {
				object enumValue = Convert.ChangeType(i, underlyingType);
				if (!Enum.IsDefined(enumType, enumValue))
					throw new ArgumentException("[ModSettings] Enum fields must have consecutive values starting at 0", field.Name);
			}
		}

		private static void ValidateAttributeCount(FieldInfo field) {
			int count = 0;
			object[] attributes = field.GetCustomAttributes(true);
			foreach (object attribute in attributes)
				if (attribute is SettingAttribute) count++;

			if (count > 1)
				throw new ArgumentException("[ModSettings] Field cannot be annotated with more than one setting type attribute", field.Name);
		}
	}
}
