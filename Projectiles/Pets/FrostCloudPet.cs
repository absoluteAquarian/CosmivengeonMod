using CosmivengeonMod.Players;
using CosmivengeonMod.Utility.Extensions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Pets{
	public class FrostCloudPet : ModProjectile{
		//Copied from ExampleMod
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Baby Blizzard Cloud");
			Main.projFrames[Projectile.type] = 4;
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.ZephyrFish);
			Projectile.width = 46;
			Projectile.height = 32;
			AIType = ProjectileID.ZephyrFish;
		}

		public override bool PreAI(){
			//Zephyr Fish sets this to true, so we need to override that
			// and make it false instead (we don't want the Zephyr Fish
			// to appear)
			Main.player[Projectile.owner].zephyrfish = false;
			return true;
		}

		public override void AI(){
			Player playerOwner = Main.player[Projectile.owner];
			PetPlayer modPlayer = playerOwner.GetModPlayer<PetPlayer>();

			if(playerOwner.dead)
				modPlayer.cloudPet = false;
			if(modPlayer.cloudPet)
				Projectile.timeLeft = 2;

			if(!Projectile.WithinDistance(playerOwner.Center, 300 * 16))
				Projectile.Center = playerOwner.Center;
		}
	}
}
