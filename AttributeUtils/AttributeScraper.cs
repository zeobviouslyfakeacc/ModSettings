using System;
using System.Reflection;

namespace ModSettings.AttributeUtils {
	internal static class AttributeScraper {
		internal static void GetAttributes(FieldInfo field, out SectionAttribute section, out NameAttribute name, out DescriptionAttribute description,
			out SliderAttribute slider, out ChoiceAttribute choice, out DisplayAttribute display, out TextBoxAttribute textBox) {
			// Must be assigned at least once, so assign to null first
			section = null;
			name = null;
			description = null;
			slider = null;
			choice = null;
			display = null;
			textBox = null;

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
				else if (attribute is DisplayAttribute displayAttribute)
					display = displayAttribute;
				else if (attribute is TextBoxAttribute textBoxAttribute)
					textBox = textBoxAttribute;
			}
		}

		internal static void GetAttributes(FieldInfo field, out NameAttribute name, out DescriptionAttribute description) {
			// Must be assigned at least once, so assign to null first
			name = null;
			description = null;

			object[] attributes = field.GetCustomAttributes(true);
			foreach (object attribute in attributes) {
				if (attribute is NameAttribute nameAttribute)
					name = nameAttribute;
				else if (attribute is DescriptionAttribute descriptionAttribute)
					description = descriptionAttribute;
			}
		}

		internal static void GetAttributes<T>(FieldInfo field, out NameAttribute name, out DescriptionAttribute description, out T attr) where T : Attribute {
			// Must be assigned at least once, so assign to null first
			name = null;
			description = null;
			attr = null;

			object[] attributes = field.GetCustomAttributes(true);
			foreach (object attribute in attributes) {
				if (attribute is NameAttribute nameAttribute)
					name = nameAttribute;
				else if (attribute is DescriptionAttribute descriptionAttribute)
					description = descriptionAttribute;
				else if (attribute is T tAttribute)
					attr = tAttribute;
			}
		}

		internal static T GetAttribute<T>(FieldInfo field) where T : Attribute {
			object[] attributes = field.GetCustomAttributes(true);
			foreach (object attribute in attributes) {
				if (attribute is T attr)
					return attr;
			}
			return null;
		}

		internal static bool HasAttribute<T>(FieldInfo field) where T : Attribute {
			return GetAttribute<T>(field) != null;
		}
	}
}
