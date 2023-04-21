using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Pets{
	public class FrostCloudPetBuff : ModBuff{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Baby Blizzard Cloud");
			Description.SetDefault("A baby blizzard cloud follows you.");
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.buffTime[buffIndex] = 18000;
			player.GetModPlayer<PetPlayer>().cloudPet = true;
			if(player.ownedProjectileCounts[ModContent.ProjectileType<FrostCloudPet>()] <= 0 && player.whoAmI == Main.myPlayer)
				Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<FrostCloudPet>(), 0, 0, player.whoAmI);
		}
	}
}
