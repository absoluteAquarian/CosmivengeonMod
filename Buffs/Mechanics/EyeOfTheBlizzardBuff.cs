using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Mechanics{
	public class EyeOfTheBlizzardBuff : ModBuff{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Eye of the Blizzard");
			Description.SetDefault("The ice crystal will support you");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<AccessoriesPlayer>().blizzardEye = true;

			if(player.ownedProjectileCounts[ModContent.ProjectileType<EyeOfTheBlizzardCrystal>()] < 1)
				Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<EyeOfTheBlizzardCrystal>(), EyeOfTheBlizzardCrystal.Damage, EyeOfTheBlizzardCrystal.Knockback, player.whoAmI);
		}
	}
}
