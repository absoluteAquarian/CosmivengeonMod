using System;
using Terraria.ModLoader;

namespace CosmivengeonMod.DataStructures{
	public class ModReference{
		private Mod instance;
		public Mod Instance => GetMod();
		private readonly string modName;
		private bool loadCheck;

		public bool Active => Instance != null;

		public ModReference(string name){
			modName = name;
		}

		public static implicit operator Mod(ModReference reference) => reference.GetMod();

		private Mod GetMod(){
			if(!loadCheck){
				loadCheck = true;
				instance = ModLoader.GetMod(modName);
			}
			return instance;
		}

		public void Unload(){
			loadCheck = false;
			instance = null;
		}

		/// <summary>
		/// Used for weak inter-mod communication. This allows you to interact with other mods without having to reference their types or namespaces, provided that they have implemented this method.
		/// </summary>
		public object Call(params object[] args) => Instance?.Call(args);

		/// <summary>
		/// Gets the type of this reference's <seealso cref="Instance"/> or <c>null</c> if the reference's mod isn't loaded.
		/// </summary>
		public new Type GetType() => Instance?.GetType();

		public static bool operator ==(Mod mod, ModReference reference) => mod == reference.GetMod();

		public static bool operator !=(Mod mod, ModReference reference) => mod != reference.GetMod();

		public override bool Equals(object obj)
			=> obj is ModReference reference && instance == reference.instance;

		public override int GetHashCode() => base.GetHashCode();
	}
}
