using ModSettings;
using System.Reflection;

namespace ModSettingsExample {
#if DEBUG // Change build profile to Debug to enable or Release to disable this example

	/*
	 * Example for how the onChange method and the RefreshGUI methods work.
	 * Adds 3 sliders whose values always add up to 1, with a preset choice.
	 */
	internal class OnChangeExampleSettings : ModSettingsBase {
		[Name("Preset")]
		[Description("Select from these presets or create your own")]
		[Choice("Custom", "Even", "First 2", "Only 1st", "Only 2nd", "Only 3rd")]
		public int preset = 1; // Default to "Even"

		[Name("Slider 1")]
		public float slider1 = 1f / 3f;
		[Name("Slider 2")]
		public float slider2 = 1f / 3f;
		[Name("Slider 3")]
		public float slider3 = 1f / 3f;

		/*
		 * This method is called whenever a field in this object is changed.
		 * The method is not marked abstract, so you don't have to override it if you don't need it.
		 */
		protected override void OnChange(FieldInfo field, object oldValue, object newValue) {
			if (field.Name == nameof(preset)) {
				UsePreset((int) newValue);
			} else {
				FixSliders(field.Name);
				preset = 0; // Custom
			}

			// Call this method to make the newly set field values show up in the GUI!
			RefreshGUI();
		}

		private void FixSliders(string sliderName) {
			// Update the other 2 sliders, but not our own
			if (sliderName == nameof(slider1)) {
				BalanceSliders(ref slider2, ref slider3);
			} else if (sliderName == nameof(slider2)) {
				BalanceSliders(ref slider1, ref slider3);
			} else if (sliderName == nameof(slider3)) {
				BalanceSliders(ref slider1, ref slider2);
			}
		}

		private void BalanceSliders(ref float upper, ref float lower) {
			float total = slider1 + slider2 + slider3;
			float diff = 1f - total;

			float upperRatio = (upper + lower < 0.01f) ? 0.5f : upper / (upper + lower);

			upper += upperRatio * diff;
			lower += (1 - upperRatio) * diff;
		}

		private void UsePreset(int preset) {
			// Ugly code ahead!
			switch (preset) {
				case 1:
					slider1 = slider2 = slider3 = (1f / 3f);
					break;
				case 2:
					slider1 = slider2 = 0.5f;
					slider3 = 0f;
					break;
				case 3:
					slider1 = 1f;
					slider2 = slider3 = 0f;
					break;
				case 4:
					slider2 = 1f;
					slider1 = slider3 = 0f;
					break;
				case 5:
					slider3 = 1f;
					slider1 = slider2 = 0f;
					break;
			}
		}
	}

	internal static class OnChangeExample {

		private static readonly OnChangeExampleSettings onChangeExample = new OnChangeExampleSettings();

		public static void OnLoad() {
			// No 2nd argument -> visible in main menu and in-game, same as MenuType.Both
			onChangeExample.AddToModSettings("OnChange Example");
		}
	}
#endif
}
