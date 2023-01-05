namespace ModSettings {
	internal abstract class Group {

		private int children;
		private int visibleChildren;

		protected Group() {
			children = 0;
			visibleChildren = 0;
		}

		protected abstract void SetVisible(bool visible);

		internal void NotifyChildAdded(bool visible) {
			++children;

			if (visible) {
				++visibleChildren;
				if (children > 1 && visibleChildren == 1) {
					SetVisible(true);
				}
			} else if (children == 1) {
				SetVisible(false);
			}
		}

		internal void NotifyChildVisible(bool visible) {
			if (visible) {
				if (visibleChildren == 0) {
					SetVisible(true);
				} else if (visibleChildren == children) {
					throw new InvalidOperationException("[ModSettings] All children of group already visible");
				}

				++visibleChildren;
			} else {
				if (visibleChildren == 1) {
					SetVisible(false);
				} else if (visibleChildren == 0) {
					throw new InvalidOperationException("[ModSettings] All children of group already invisible");
				}

				--visibleChildren;
			}
		}
	}
}
