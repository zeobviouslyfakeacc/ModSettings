using UnityEngine;

namespace ModSettings {
	internal class DescriptionHolder : MonoBehaviour {

		private string descriptionText;

		internal void SetDescription(string description, bool localize) {
			if (localize) {
				descriptionText = description;
			} else {
				descriptionText = Localization.Get(description);
			}
		}

		internal string Text => descriptionText;
	}
}
