using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;

namespace CosmivengeonMod.Items.Draek{
	public class TerraBolt : ModItem{
		public override string Texture => "CosmivengeonMod/Items/Draek/Terra_Bolt";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Terra Bolt");
			Tooltip.SetDefault("[c/6a00aa:Desolator]" +
				"\nLaunches a devastating barrage of earthen stones. Rocks will curse" +
				"\nenemies with Primordial Wrath, lowering their defense and damaging" +
				"\nthem over time." +
				"\nRight click to shoot a shotgun burst of rocks." +
				"\nOne of the legendary 10 Desolator weapons, an ancient stone capable" +
				"\nof ripping apart the planet itself if used in the right hands.  How" +
				"\nDraek managed to stumble across something this powerful is unknown," +
				"\nbut if it were to ever be combined with the others of its class..." +
				"\nDesolation Mode Item");
		}

		private const int PRIMARY_DAMAGE = 7;
		private const int SECONDARY_DAMAGE = 2;
		private const int PRIMARY_USETIME = 9;
		private const int SECONDARY_USETIME = 46;

		public override void SetDefaults(){
			item.damage = PRIMARY_DAMAGE;
			item.width = 42;
			item.height = 24;
			item.useTime = PRIMARY_USETIME;
			item.useAnimation = PRIMARY_USETIME;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 2f;
			item.value = Item.sellPrice(0, 5, 0, 0);
			item.autoReuse = true;
			item.rare = 3;
			item.UseSound = SoundID.Item11;
			item.shoot = 10;
			item.shootSpeed = 30f;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player){
			if(player.altFunctionUse == 2){		//Right click
				item.damage = SECONDARY_DAMAGE;
				item.useTime = SECONDARY_USETIME;
				item.useAnimation = SECONDARY_USETIME;
				item.shootSpeed = 30f;
				item.UseSound = SoundID.Item38;
			}else{
				item.damage = PRIMARY_DAMAGE;
				item.useTime = PRIMARY_USETIME;
				item.useAnimation = PRIMARY_USETIME;
				item.shootSpeed = 30f;
				item.UseSound = SoundID.Item11;
			}
			return true;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			type = ModContent.ProjectileType<TerraBoltProjectile>();
			if(player.altFunctionUse == 2){		//Right click
				//Spawn 8 projectiles in a "cone"
				Vector2 speed = new Vector2(speedX, speedY);
				float speedRotation = speed.ToRotation();
				int numProjectiles = 8;

				for(int i = 0; i < numProjectiles; i++){
					float newRotation = speedRotation + MathHelper.ToRadians(Main.rand.NextFloat(-5, 5));
					float speedOffset = speed.Length() + Main.rand.NextFloat(-7, 7);
					Vector2 randomSpeed = newRotation.ToRotationVector2() * speedOffset;

					Projectile.NewProjectile(position,
						randomSpeed,
						type,
						damage,
						knockBack,
						item.owner
					);
				}

				return false;	//Prevent the normal projectile from being fired
			}
			return true;
		}
	}
}