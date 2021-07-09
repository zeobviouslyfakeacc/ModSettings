using System.IO;
using UnityEngine;
using static CustomExperienceModeManager;

namespace ModSettings.Examples {
#if DEBUG // Change build profile to Debug to enable or Release to disable this example
	internal class DynamicSettings : ModSettingsBase {
		private const int NUM_FIBONACCI = 20;

		[HideFromModSettings]
		public KeyCode keyCode = KeyCode.Y;

		[Name("Yes / No choice")]
		[Description("public bool <field name> = <default value>")]
		public bool boolVal = true;

		[Name("Choice from string array")]
		[Description("Requires a Choice attribute and an integer, boolean, or enum type. Default value = index in choice array")]
		[Choice("Utopia", "Something in between", "Dystopian Nightmare")]
		public int stringChoice = 1;

		[Name("Choice with a standard enum")]
		[Description("The default value (no Choice attribute) for a CustomExperienceModeManager enum uses the default, localized names")]
		public CustomTunableTimeOfDay timeOfDayChoice = CustomTunableTimeOfDay.Midnight;

		[Name("Slider for a float, no steps")]
		[Description("Also works with double or decimal, but they're limited to float range and accuracy")]
		[Slider(-10f, 10f)]
		public float steplessSliderValue = 2.5f;

		[Name("Slider for a float with steps")]
		[Slider(-1f, 1f, 5)] // -1, -0.5, 0, 0.5, 1
		public float steppedSliderValue = -0.5f;

		[Name("Slider for an int")]
		[Description("Also works with byte, sbyte, short, ushort, uint, and long")]
		[Slider(-5, 5)] // -5, -4, ..., 4, 5
		public int intSliderValue = 1;

		[Name("Slider for a float with no slider attribute")]
		public float defaultFloatSlider = 0f;

		[Name("Slider for an int with no slider attribute")]
		public int defaultIntSlider = 0;

		[HideFromModSettings]
		public string textBox1 = "Text Box 1";

		[Name("Text Box 2")]
		[TextBox]
		public string textBox2 = "Text Box 2";

		internal override void MakeGUIContents(GUIBuilder guiBuilder) {
			guiBuilder.AddHeader("Information");
			guiBuilder.AddEmptySetting("ModSettings", BuildInfo.Version);
			guiBuilder.AddEmptySetting("Game Version", GameManager.GetVersionString());
			guiBuilder.AddEmptySetting("Mods Folder", Path.Combine(MelonLoader.MelonUtils.GameDirectory, "Mods"));

			guiBuilder.AddHeader("Settings");
			guiBuilder.AddKeySetting(this, GetFieldForName("keyCode"), "Custom Key Binding", "Click to set the keybinding");

			guiBuilder.AddChoiceSetting(this, GetFieldForName("boolVal"));
			guiBuilder.AddChoiceSetting(this, GetFieldForName("stringChoice"));
			guiBuilder.AddChoiceSetting(this, GetFieldForName("timeOfDayChoice"));

			guiBuilder.AddSliderSetting(this, GetFieldForName("intSliderValue")); //the order can be rearranged
			guiBuilder.AddSliderSetting(this, GetFieldForName("steplessSliderValue"));
			guiBuilder.AddSliderSetting(this, GetFieldForName("steppedSliderValue"));
			guiBuilder.AddSliderSetting(this, GetFieldForName("defaultFloatSlider"));
			guiBuilder.AddSliderSetting(this, GetFieldForName("defaultIntSlider"));

			guiBuilder.AddTextBoxSetting(this, GetFieldForName("textBox1"), "Text Box 1");
			guiBuilder.AddTextBoxSetting(this, GetFieldForName("textBox2"));

			guiBuilder.AddHeader("Fibonacci");
			guiBuilder.AddEmptySetting("0", "0");
			guiBuilder.AddEmptySetting("1", "1");
			int a = 0;
			int b = 1;
			int c;
			for (int i = 2; i <= NUM_FIBONACCI; i++) {
				c = a + b;
				guiBuilder.AddEmptySetting(i.ToString(), c.ToString());
				a = b;
				b = c;
			}
		}
	}

	internal static class DynamicExample {

		internal static readonly DynamicSettings settings = new DynamicSettings();

		public static void OnLoad() {
			settings.AddToModSettings("Dynamic Settings Example");
		}
	}
#endif
}
