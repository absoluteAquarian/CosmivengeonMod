using CosmivengeonMod.DamageClasses.Desolate;
using Terraria;

namespace CosmivengeonMod.Utility.Extensions {
	public static partial class Extensions {
		public static DesolatorDamageClass Desolate(this Player player) => player.GetModPlayer<DesolatorDamageClass>();
	}
}
