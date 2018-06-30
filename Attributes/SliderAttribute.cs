using System;

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
	}
}
