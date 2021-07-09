using System;

namespace ModSettings.Examples {
#if DEBUG // Change build profile to Debug to enable or Release to disable this example
	internal class CustomChoiceSettings : ModSettingsBase {
		[Name("Total")]
		[Description("The sum of the two numbers")]
		[Display]
		public int total = 0;

		[Name("Sign")]
		[Description("Positive, Negative, or Zero")]
		[Display]
		public string sign = "Dont Know";

		[HideFromModSettings]
		public int num1 = 0;

		[HideFromModSettings]
		public int num2 = 0;

		internal override void MakeGUIContents(GUIBuilder guiBuilder) {
			guiBuilder.AddPaddingHeader();

			guiBuilder.AddCustomChoiceSetting(this,
				GetFieldForName("num1"),
				"Number 1",
				"Left is down. Right is up",
				new Func<int>(DecreaseNum1),
				new Func<int>(IncreaseNum1),
				new Action(UpdateDisplay));
			guiBuilder.AddCustomChoiceSetting(this,
				GetFieldForName("num2"),
				"Number 2",
				"Left is down. Right is up",
				new Func<int>(DecreaseNum2),
				new Func<int>(IncreaseNum2),
				new Action(UpdateDisplay));

			guiBuilder.AddPaddingHeader();

			base.MakeGUIContents(guiBuilder);
		}

		protected override void OnSelect() => UpdateDisplay();
		private int DecreaseNum1() => num1 - 1;
		private int IncreaseNum1() => num1 + 1;
		private int DecreaseNum2() => num2 - 1;
		private int IncreaseNum2() => num2 + 1;
		internal void UpdateDisplay() {
			total = num1 + num2;
			if (total < 0) sign = "Negative";
			else if (total > 0) sign = "Positive";
			else sign = "Zero";
			RefreshGUI();
		}
	}

	internal static class CustomChoiceBoxExample {

		internal static readonly CustomChoiceSettings settings = new CustomChoiceSettings();

		public static void OnLoad() {
			settings.AddToModSettings("Custom Choice Boxes Example");
		}
	}
#endif
}
