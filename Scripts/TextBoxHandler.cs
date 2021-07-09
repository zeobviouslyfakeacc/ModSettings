using System;

namespace ModSettings.Scripts {
	internal class TextBoxHandler : UnityEngine.MonoBehaviour {
		static TextBoxHandler() => UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<TextBoxHandler>();
		public TextBoxHandler(System.IntPtr intPtr) : base(intPtr) { }

		public Action onDeselect;
	}
}
