using ModSettings;
using UnityEngine;
using static CustomExperienceModeManager;

namespace ModSettingsExample {
#if DEBUG // Change build profile to Debug to enable or Release to disable this example

	internal class ExampleSettings : ModSettingsBase {
		[Section("Custom GUI Examples")]

		[Name("Yes / No choice")]
		[Description("public bool <field name> = <default value>")]
		public bool boolVal = true;

		[Name("Choice from string array")]
		[Description("Requires a Choice attribute and an integer, boolean, or enum type. Default value = index in choice array")]
		[Choice("Utopia", "Something in between", "Dystopian Nightmare")]
		public int stringChoice = 1;

		[Name("Choice from an enum with default names")]
		[Description("Will produce the same choice names as above. Only works when enum values start with 0 and are consecutive")]
		public ExampleEnum enumChoice = ExampleEnum.DystopianNightmare;

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

		/*
		 * This method is called whenever the user presses the confirm button in the
		 * custom experience mode panel or in the mod settings panel.
		 * You also don't need to override this method if you don't need it.
		 */
		protected override void OnConfirm() {
			Debug.Log("Settings applied!");
			Debug.Log("boolVal = " + boolVal);
			Debug.Log("stringChoice = " + stringChoice);
			Debug.Log("enumChoice = " + enumChoice);
			Debug.Log("timeOfDayChoice = " + timeOfDayChoice);
			Debug.Log("steplessSliderValue = " + steplessSliderValue);
			Debug.Log("steppedSliderValue = " + steppedSliderValue);
			Debug.Log("intSliderValue = " + intSliderValue);
		}
	}

	internal enum ExampleEnum {
		Utopia, Something_in_between, DystopianNightmare
	}

	internal static class BasicExample {

		internal static readonly ExampleSettings settings = new ExampleSettings();

		public static void OnLoad() {
			// We can add this to the custom mode menu, somewhere above, below, or between Hinterland's settings
			settings.AddToCustomModeMenu(Position.AboveAll);

			// And we can add them to the new mod settings menu
			settings.AddToModSettings("Custom Settings Example", MenuType.InGameOnly); // or MenuType.MainMenuOnly, or MenuType.Both
		}
	}
#endif
}
