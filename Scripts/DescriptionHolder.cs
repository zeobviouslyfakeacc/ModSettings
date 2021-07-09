using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace ModSettings.Scripts {
	internal class DescriptionHolder : MonoBehaviour {

		static DescriptionHolder() => UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<DescriptionHolder>();
		public DescriptionHolder(System.IntPtr ptr) : base(ptr) { }

		[HideFromIl2Cpp]
		internal void SetDescription(string description, bool localize) {
			if (localize) {
				Text = description;
			} else {
				Text = Localization.Get(description);
			}
		}

		internal string Text {
			[HideFromIl2Cpp]
			get;

			[HideFromIl2Cpp]
			private set;
		}
	}
}
