using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class EyeOfTheBlizzard_Cooldown : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Eye of the Blizzard: Ability Cooldown");
			Description.SetDefault("The crystal is recharging");
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<CosmivengeonPlayer>().equipped_EyeOfTheBlizzard = true;
			if(Main.projectile.Count(p => p?.active == true && p.modProjectile is EyeOfTheBlizzardCrystal && p.owner == player.whoAmI) < 1)
				Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<EyeOfTheBlizzardCrystal>(), EyeOfTheBlizzardCrystal.Damage, EyeOfTheBlizzardCrystal.Knockback, player.whoAmI);
		}
	}
}
