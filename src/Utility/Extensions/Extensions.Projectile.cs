using Terraria;

namespace CosmivengeonMod.Utility {
	public static partial class Extensions {
		public static void TryDecrementAlpha(this Projectile proj, int amount) {
			if (proj.alpha > 0)
				proj.alpha -= amount;
			if (proj.alpha < 0)
				proj.alpha = 0;
		}
	}
}
