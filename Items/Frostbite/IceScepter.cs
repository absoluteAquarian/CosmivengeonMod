using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class IceScepter : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Ice Lord's Scepter");
			Tooltip.SetDefault("Summons the elemental frost lizard's snow wall at the cursor" +
				"\nThe wall lasts for 15 seconds");
		}

		public override void SetDefaults(){
			item.width = 44;
			item.height = 44;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 35;
			item.useAnimation = 35;
			item.summon = true;
			item.damage = 25;
			item.knockBack = 4f;
			item.mana = 25;
			item.shoot = ModContent.ProjectileType<IceScepterWall>();
			item.noMelee = true;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 4, copper: 80);
			item.UseSound = SoundID.Item30;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			//Find the closest solid tile above or below the cursor and spawn the sentry there
			int tileX = (int)Main.MouseWorld.X >> 4;
			int tileY = (int)Main.MouseWorld.Y >> 4;

			for(int i = 0; i < 50; i++){
				if(tileY + i <= Main.maxTilesY && CosmivengeonUtils.TileIsSolidOrPlatform(tileX, tileY + i)){
					tileY += i;
					break;
				}else if(tileY - i > 0 && CosmivengeonUtils.TileIsSolidOrPlatform(tileX, tileY - i)){
					tileY -= i;
					break;
				}
			}

			position = new Point(tileX, tileY).ToWorldCoordinates(8, 0) - new Vector2(0, 116 / 2f) * IceScepterWall.Scale;

			//If any of this weapon's sentries exist, kill them
			for(int i = 0; i < 1000; i++){
				if(Main.projectile[i]?.active == true && Main.projectile[i].modProjectile is IceScepterWall)
					Main.projectile[i].active = false;
			}

			return true;
		}
	}
}
