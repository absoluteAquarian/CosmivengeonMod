using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Utility.Extensions{
	public static partial class Extensions{
		public static SoundEffectInstance PlayCustomSound(this Mod mod, Vector2 position, string file)
			=> Main.PlaySound(SoundLoader.customSoundType, (int)position.X, (int)position.Y, mod.GetSoundSlot(SoundType.Custom, $"Sounds/Custom/{file}"));
	}
}
