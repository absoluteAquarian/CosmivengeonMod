using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class EyeOfTheBlizzardBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Eye of the Blizzard");
			Description.SetDefault("The ice crystal will support you");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<CosmivengeonPlayer>().equipped_EyeOfTheBlizzard = true;
			if(Main.projectile.Count(p => p?.active == true && p.modProjectile is EyeOfTheBlizzardCrystal && p.owner == player.whoAmI) < 1)
				Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<EyeOfTheBlizzardCrystal>(), EyeOfTheBlizzardCrystal.Damage, EyeOfTheBlizzardCrystal.Knockback, player.whoAmI);
		}
	}
}