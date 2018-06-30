using System;
using System.Reflection;

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
				Type fieldType = field.FieldType;

				if (fieldType.IsEnum) {
					if (Enum.GetUnderlyingType(fieldType) != typeof(int))
						throw new ArgumentException("[ModSettings] Only enums with an underlying type of int are currently supported", field.Name);
				} else if (fieldType != typeof(int) && fieldType != typeof(bool) && fieldType != typeof(float)) {
					throw new ArgumentException("[ModSettings] Only int, float, bool, and enum field types are currently supported", field.Name);
				}

				ValidateFieldAttributes(field, fieldType, modSettings);
			}
		}

		private static void ValidateFieldAttributes(FieldInfo field, Type fieldType, ModSettingsBase modSettings) {
			GetAttributes(field, out SectionAttribute section, out NameAttribute name,
					out DescriptionAttribute description, out SliderAttribute slider, out ChoiceAttribute choice);

			if (name == null) {
				throw new ArgumentException("[ModSettings] Mod settings contain field without a name attribute", field.Name);
			} else if (string.IsNullOrEmpty(name.Name)) {
				throw new ArgumentException("[ModSettings] Setting name attribute must have a non-empty value", field.Name);
			}

			if (section != null && string.IsNullOrEmpty(section.Title))
				throw new ArgumentException("[ModSettings] Section title attribute must have a non-empty value", field.Name);

			if (slider != null && choice != null)
				throw new ArgumentException("[ModSettings] Field cannot be annotated with both 'Slider' and 'Choice' attributes", field.Name);

			if (slider != null) {
				if (fieldType != typeof(int) && fieldType != typeof(float))
					throw new ArgumentException("[ModSettings] 'Slider' attribute can only be used on fields with int or float type", field.Name);

				float max = Math.Max(slider.From, slider.To);
				float min = Math.Min(slider.From, slider.To);
				float defaultValue = Convert.ToSingle(field.GetValue(modSettings));
				if (max == min) {
					throw new ArgumentException("[ModSettings] 'Slider' must have different 'From' and 'To' values", field.Name);
				} else if (defaultValue < min || defaultValue > max) {
					throw new ArgumentException("[ModSettings] 'Slider' default value must be between 'From' and 'To'", field.Name);
				}

				if (slider.NumberFormat != null) {
					try {
						if (fieldType == typeof(int)) {
							string.Format(slider.NumberFormat, 0);
						} else if (fieldType == typeof(float)) {
							string.Format(slider.NumberFormat, 0f);
						}
					} catch (FormatException fe) {
						throw new ArgumentException("[ModSettings] Invalid 'Slider' number format", field.Name, fe);
					}
				}
			}

			if (choice != null) {
				if (fieldType != typeof(int) && !fieldType.IsEnum)
					throw new ArgumentException("[ModSettings] 'Choice' attribute can only be used on fields with int or enum type", field.Name);

				if (choice.Names == null || choice.Names.Length == 0)
					throw new ArgumentException("[ModSettings] 'Choice' attribute must contain non-empty array of non-empty strings", field.Name);

				foreach (string choiceName in choice.Names) {
					if (string.IsNullOrEmpty(choiceName))
						throw new ArgumentException("[ModSettings] 'Choice' attribute must contain non-empty array of non-empty strings", field.Name);
				}

				int defaultValue = (int) field.GetValue(modSettings);
				if (defaultValue < 0 || defaultValue >= choice.Names.Length) {
					throw new ArgumentException("[ModSettings] Default value out of range for 'Choice' attribute", field.Name);
				}
			} else if (fieldType.IsEnum) {
				Array values = Enum.GetValues(fieldType);
				for (int i = 0; i < values.Length; ++i) {
					if ((int) values.GetValue(i) != i)
						throw new ArgumentException("[ModSettings] Enum fields without 'Choice' attribute must have consecutive values starting at 0", field.Name);
				}
			}
		}
	}
}
