using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Pets{
	public class FrostCloudPet : ModProjectile{
		//Copied from ExampleMod
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Baby Blizzard Cloud");
			Main.projFrames[projectile.type] = 4;
			Main.projPet[projectile.type] = true;
		}

		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.ZephyrFish);
			projectile.width = 46;
			projectile.height = 32;
			aiType = ProjectileID.ZephyrFish;
		}

		public override bool PreAI(){
			//Zephyr Fish sets this to true, so we need to override that
			// and make it false instead (we don't want the Zephyr Fish
			// to appear)
			Main.player[projectile.owner].zephyrfish = false;
			return true;
		}

		public override void AI(){
			Player playerOwner = Main.player[projectile.owner];
			CosmivengeonPlayer modPlayer = playerOwner.GetModPlayer<CosmivengeonPlayer>();

			if(playerOwner.dead)
				modPlayer.cloudPet = false;
			if(modPlayer.cloudPet)
				projectile.timeLeft = 2;
		}
	}
}
