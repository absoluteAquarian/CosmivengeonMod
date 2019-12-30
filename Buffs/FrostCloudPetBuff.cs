using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class FrostCloudPetBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Baby Blizzard Cloud");
			Description.SetDefault("A baby blizzard cloud follows you.");
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.buffTime[buffIndex] = 18000;
			player.GetModPlayer<CosmivengeonPlayer>().cloudPet = true;
			if(player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Pets.FrostCloudPet>()] <= 0 && player.whoAmI == Main.myPlayer)
				Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.Pets.FrostCloudPet>(), 0, 0, player.whoAmI);
		}
	}
}
