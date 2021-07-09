using System;
using System.Reflection;

namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class TextBoxAttribute : SettingAttribute {
		internal void ValidateFor(ModSettingsBase modSettings, FieldInfo field) {
			Type fieldType = field.FieldType;

			if (fieldType != typeof(string))
				throw new ArgumentException("[ModSettings] Field type " + fieldType.Name + " is not supported for text boxes", field.Name);

			string defaultValue = Convert.ToString(field.GetValue(modSettings));

			if (string.IsNullOrEmpty(defaultValue))
				throw new ArgumentException("[ModSettings] Default value cannot be null or empty for 'TextBox' attribute", field.Name);
		}
	}
}
