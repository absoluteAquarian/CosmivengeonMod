using Terraria.ModLoader;

namespace CosmivengeonMod.DataStructures {
	public class ModReference {
		private Mod instance;
		public Mod Instance => GetMod();
		private readonly string modName;
		private bool loadCheck;

		public bool Active { get; private set; }

		public ModReference(string name) {
			modName = name;
		}

		public static implicit operator Mod(ModReference reference) => reference.GetMod();

		private Mod GetMod() {
			if (!loadCheck) {
				loadCheck = true;
				Active = ModLoader.TryGetMod(modName, out instance);
			}

			return instance;
		}

		public void Unload() {
			loadCheck = false;
			instance = null;
		}
	}
}
