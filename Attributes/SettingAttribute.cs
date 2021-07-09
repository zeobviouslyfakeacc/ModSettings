using System;

namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class SettingAttribute : Attribute { }
}
