using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;
using Il2Cpp;
using UnityEngine;

namespace ModSettings {
	internal class DescriptionHolder : MonoBehaviour {

		static DescriptionHolder() {
			ClassInjector.RegisterTypeInIl2Cpp<DescriptionHolder>();
		}
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
