using System.Reflection;

namespace ModSettings.Examples {
#if DEBUG // Change build profile to Debug to enable or Release to disable this example

	/*
	 * Example for how the onChange method and the field visibility methods work.
	 * Adds a slider that lets you change how many settings from BasicExample are visible.
	 */
	internal class VisibilityExampleSettings : ModSettingsBase {
		[Section("Visibility Example")]
		[Name("Number of visible custom settings")]
		[Description("... in the \"Custom GUI Examples\" section below")]
		[Slider(0, 9)]
		public int visibleSettingsCount = 9;

		/*
		 * This method is called whenever a field in this object is changed.
		 * The method is not marked abstract, so you don't have to override it if you don't need it.
		 */
		protected override void OnChange(FieldInfo field, object oldValue, object newValue) {
			// We already know there's just one field, but let's be pretend we don't
			if (field.Name == nameof(visibleSettingsCount)) {
				int visible = (int)newValue;
				VisibilityExample.SetFieldsVisible(visible);
			}
		}

		/*
		 * This method is called every time this particular settings page is opened.
		 * The method is not marked abstract, so you don't have to override it if you don't need it.
		 */
		protected override void OnSelect() {
			VisibilityExample.SetFieldsVisible(visibleSettingsCount); //Set the correct number of settings visible
			base.OnSelect(); //Let the base refresh the GUI
		}
	}

	internal static class VisibilityExample {

		private static readonly VisibilityExampleSettings onChangeExample = new VisibilityExampleSettings();
		private static readonly ExampleSettings basicExampleSettings = new ExampleSettings();
		private static readonly PaddingSettings padding = new PaddingSettings();

		public static void OnLoad() {
			// Let's have these in the main menu only
			onChangeExample.AddToModSettings("Custom Settings Example", MenuType.MainMenuOnly);
			basicExampleSettings.AddToModSettings("Custom Settings Example", MenuType.MainMenuOnly);

			// Just to show off the scroll bars in the mod settings panel
			padding.AddToModSettings("Custom Settings Example", MenuType.MainMenuOnly);
		}

		internal static void SetFieldsVisible(int visibleFields) {
			FieldInfo[] fields = basicExampleSettings.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

			for (int i = 0; i < fields.Length; ++i) {
				bool shouldBeVisible = i < visibleFields;
				basicExampleSettings.SetFieldVisible(fields[i], shouldBeVisible);
			}
		}
	}

	// Just to show off the scroll bars in the mod settings panel. Seriously, ignore this
	internal class PaddingSettings : ModSettingsBase {
		[Section("Padding")]

		[Name("P")]
		public bool p = true;
		[Name("A")]
		public bool a = true;
		[Name("D")]
		public bool d0 = true;
		[Name("D")]
		public bool d1 = true;
		[Name("I")]
		public bool i = true;
		[Name("N")]
		public bool n = true;
		[Name("G")]
		public bool g = true;
	}
#endif
}
