using System;
using System.Reflection;
using static ModSettings.AttributeFieldTypes;

namespace ModSettings {
	internal static class Attributes {

		internal static void GetAttributes(FieldInfo field, out SectionAttribute section, out NameAttribute name,
										   out DescriptionAttribute description, out SliderAttribute slider, out ChoiceAttribute choice) {
			// Must be assigned at least once, so assign to null first
			section = null;
			name = null;
			description = null;
			slider = null;
			choice = null;

			object[] attributes = field.GetCustomAttributes(true);
			foreach (object attribute in attributes) {
				if (attribute is SectionAttribute sectionAttribute)
					section = sectionAttribute;
				else if (attribute is NameAttribute nameAttribute)
					name = nameAttribute;
				else if (attribute is DescriptionAttribute descriptionAttribute)
					description = descriptionAttribute;
				else if (attribute is SliderAttribute sliderAttribute)
					slider = sliderAttribute;
				else if (attribute is ChoiceAttribute choiceAttribute)
					choice = choiceAttribute;
			}
		}

		internal static void ValidateFields(ModSettingsBase modSettings) {
			foreach (FieldInfo field in modSettings.GetFields()) {
				ValidateFieldAttributes(modSettings, field);
			}
		}

		private static void ValidateFieldAttributes(ModSettingsBase modSettings, FieldInfo field) {
			GetAttributes(field, out SectionAttribute section, out NameAttribute name,
					out DescriptionAttribute description, out SliderAttribute slider, out ChoiceAttribute choice);

			Type fieldType = field.FieldType;

			if (name == null) {
				throw new ArgumentException("[ModSettings] Mod settings contain field without a name attribute", field.Name);
			} else if (string.IsNullOrEmpty(name.Name)) {
				throw new ArgumentException("[ModSettings] Setting name attribute must have a non-empty value", field.Name);
			}

			if (section != null && string.IsNullOrEmpty(section.Title))
				throw new ArgumentException("[ModSettings] Section title attribute must have a non-empty value", field.Name);

			if (slider != null && choice != null) {
				throw new ArgumentException("[ModSettings] Field cannot be annotated with both 'Slider' and 'Choice' attributes", field.Name);
			} else if (slider != null) {
				slider.ValidateFor(modSettings, field);
			} else if (choice != null) {
				choice.ValidateFor(modSettings, field);
			} else if (!IsSupportedType(fieldType)) {
				throw new ArgumentException("[ModSettings] Field type " + fieldType.Name + " is not supported", field.Name);
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
	}
}
