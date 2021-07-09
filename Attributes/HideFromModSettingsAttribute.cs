using System;

namespace ModSettings {
	/// <summary>
	/// Hides the field from attribute validation and automatic setting creation in the default MakeGUIContents
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class HideFromModSettingsAttribute : Attribute { }
}
