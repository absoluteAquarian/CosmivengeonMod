using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite {
	public class IceDisk : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Abominable's Animosity");
			Tooltip.SetDefault("Launches a frozen disk that homes in on" +
				"\nthe cursor's position.  Once it reaches" +
				"\nthe cursor, it shoots out several frozen" +
				"\nshards then splits into two smaller disks.");
		}

		public override void SetDefaults() {
			Item.width = 42;
			Item.height = 38;
			Item.DamageType = DamageClass.Throwing;
			Item.damage = 13;
			Item.knockBack = 5.1f;
			Item.rare = ItemRarityID.Blue;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item1;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.shootSpeed = IceDiskProjectile.MaxVelocity;
			Item.shoot = ModContent.ProjectileType<IceDiskProjectile>();
			Item.channel = true;
		}

		public override bool CanUseItem(Player player) {
			int ai1Count = 0;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile projectile = Main.projectile[i];

				if (!projectile.active || projectile.ModProjectile is not IceDiskProjectile)
					continue;

				if (projectile.ai[0] == 0)  //Existing disk is following the mouse.  Don't spawn another one
					return false;

				if (projectile.ai[0] == 1) {  //Existing disc is spinning in place, spawning shards.  Only let up to 3 of those exist at once
					ai1Count++;

					if (ai1Count >= 3)
						return false;
				}
			}

			return true;
		}
	}
}
