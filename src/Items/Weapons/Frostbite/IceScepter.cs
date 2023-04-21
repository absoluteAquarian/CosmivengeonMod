using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite {
	public class IceScepter : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ice Lord's Scepter");
			Tooltip.SetDefault("Summons the elemental frost lizard's snow wall at the cursor" +
				"\nThe wall lasts for 15 seconds");
		}

		public override void SetDefaults() {
			Item.width = 44;
			Item.height = 44;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.DamageType = DamageClass.Summon;
			Item.damage = 25;
			Item.knockBack = 4f;
			Item.mana = 25;
			Item.shoot = ModContent.ProjectileType<IceScepterWall>();
			Item.noMelee = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 4, copper: 80);
			Item.UseSound = SoundID.Item30;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			//Find the closest solid tile above or below the cursor and spawn the sentry there
			int tileX = (int)Main.MouseWorld.X >> 4;
			int tileY = (int)Main.MouseWorld.Y >> 4;

			for (int i = 0; i < 50; i++) {
				if (tileY + i <= Main.maxTilesY && MiscUtils.TileIsSolidOrPlatform(tileX, tileY + i)) {
					tileY += i;
					break;
				} else if (tileY - i > 0 && MiscUtils.TileIsSolidOrPlatform(tileX, tileY - i)) {
					tileY -= i;
					break;
				}
			}

			position = new Point(tileX, tileY).ToWorldCoordinates(8, 0) - new Vector2(0, 116 / 2f) * IceScepterWall.Scale;

			//If any of this weapon's sentries exist, kill them
			for (int i = 0; i < 1000; i++) {
				if (Main.projectile[i]?.active == true && Main.projectile[i].ModProjectile is IceScepterWall)
					Main.projectile[i].active = false;
			}

			return true;
		}
	}
}
