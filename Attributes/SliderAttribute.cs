using System;
using System.Reflection;
using static ModSettings.AttributeFieldTypes;

namespace ModSettings {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class SliderAttribute : Attribute {

		internal const string DefaultFloatFormat = "{0:F1}";
		internal const string DefaultIntFormat = "{0:D}";
		internal static readonly SliderAttribute DefaultFloatRange = new SliderAttribute(0, 1) { numberFormat = "{0:F2}" };
		internal static readonly SliderAttribute DefaultIntRange = new SliderAttribute(0, 100, 101) { numberFormat = "{0,3:D}%" };

		private readonly float from;
		private readonly float to;
		private readonly int numberOfSteps;
		private string numberFormat = null;

		public SliderAttribute(float from, float to) : this(from, to, -1) { }

		public SliderAttribute(float from, float to, int numberOfSteps) {
			this.from = from;
			this.to = to;
			this.numberOfSteps = numberOfSteps;
		}

		public float From {
			get => from;
		}

		public float To {
			get => to;
		}

		public int NumberOfSteps {
			get => numberOfSteps;
		}

		public string NumberFormat {
			get => numberFormat;
			set => numberFormat = value;
		}

		internal void ValidateFor(ModSettingsBase modSettings, FieldInfo field) {
			Type fieldType = field.FieldType;
			if (!IsSliderType(fieldType))
				throw new ArgumentException("[ModSettings] 'Slider' attribute doesn't support fields of type " + fieldType.Name, field.Name);

			float max = Math.Max(from, to);
			float min = Math.Min(from, to);
			float defaultValue = Convert.ToSingle(field.GetValue(modSettings));

			if (max == min) {
				throw new ArgumentException("[ModSettings] 'Slider' must have different 'From' and 'To' values", field.Name);
			} else if (defaultValue < min || defaultValue > max) {
				throw new ArgumentException("[ModSettings] 'Slider' default value must be between 'From' and 'To'", field.Name);
			}

			if (IsIntegerType(fieldType)) {
				long minAsLong = (long) Math.Round(min);
				long maxAsLong = (long) Math.Round(max);

				if (minAsLong < MinValue(fieldType))
					throw new ArgumentException("[ModSettings] 'Slider' minimum value smaller than minimum value of " + fieldType.Name, field.Name);
				if (maxAsLong > MaxValue(fieldType))
					throw new ArgumentException("[ModSettings] 'Slider' maximum value larger than maximum value of " + fieldType.Name, field.Name);

				if (numberOfSteps > (maxAsLong - minAsLong + 1L))
					throw new ArgumentException("[ModSettings] 'Slider' has too many steps to be able to support integer values", field.Name);
			}

			if (!string.IsNullOrEmpty(numberFormat)) {
				try {
					if (IsFloatType(fieldType)) {
						string.Format(numberFormat, 0f);
					} else {
						string.Format(numberFormat, 0);
					}
				} catch (FormatException fe) {
					throw new ArgumentException("[ModSettings] Invalid 'Slider' number format", field.Name, fe);
				}
			}
		}
	}
}
