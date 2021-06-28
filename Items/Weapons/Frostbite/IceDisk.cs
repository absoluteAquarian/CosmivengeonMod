using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite{
	public class IceDisk : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Abominable's Animosity");
			Tooltip.SetDefault("Launches a frozen disk that homes in on" +
				"\nthe cursor's position.  Once it reaches" +
				"\nthe cursor, it shoots out several frozen" +
				"\nshards then splits into two smaller disks.");
		}

		public override void SetDefaults(){
			item.width = 42;
			item.height = 38;
			item.thrown = true;
			item.damage = 13;
			item.knockBack = 5.1f;
			item.rare = ItemRarityID.Blue;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 20;
			item.useAnimation = 20;
			item.noUseGraphic = true;
			item.autoReuse = true;
			item.noMelee = true;
			item.UseSound = SoundID.Item1;
			item.value = Item.sellPrice(silver: 1, copper: 50);
			item.shootSpeed = IceDiskProjectile.MaxVelocity;
			item.shoot = ModContent.ProjectileType<IceDiskProjectile>();
			item.channel = true;
		}

		public override bool CanUseItem(Player player){
			int ai1Count = 0;
			for(int i = 0; i < Main.maxProjectiles; i++){
				Projectile projectile = Main.projectile[i];

				if(!projectile.active || !(projectile.modProjectile is IceDiskProjectile))
					continue;

				if(projectile.ai[0] == 0)  //Existing disk is following the mouse.  Don't spawn another one
					return false;

				if(projectile.ai[0] == 1){  //Existing disc is spinning in place, spawning shards.  Only let up to 3 of those exist at once
					ai1Count++;

					if(ai1Count >= 3)
						return false;
				}
			}

			return true;
		}
	}
}
