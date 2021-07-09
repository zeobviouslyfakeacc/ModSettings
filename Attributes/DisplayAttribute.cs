using System;

namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class DisplayAttribute : SettingAttribute { }
}
