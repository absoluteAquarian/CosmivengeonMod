using CosmivengeonMod.DamageClasses.Desolate;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek {
	public class TerraBolt : DesolatorItem {
		public override string ItemName => "Terra Bolt";

		public override string FlavourText =>
			"Launches a devastating barrage of earthen stones. Rocks will curse" +
			"\nenemies with Primordial Wrath, lowering their defense and damaging" +
			"\nthem over time." +
			"\nRight click to shoot a shotgun burst of rocks." +
			"\nOne of the legendary 10 Desolator weapons, an ancient stone capable" +
			"\nof ripping apart the planet itself if used in the right hands.  How" +
			"\nDraek managed to stumble across something this powerful is unknown," +
			"\nbut if it were to ever be combined with the others of its class...";

		public override void SafeSetDefaults() {
			Item.damage = 18;
			Item.width = 42;
			Item.height = 24;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = 6;  //Custom use style
			Item.channel = true;
			Item.mana = 3;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<TerraBoltCharge>();
			Item.shootSpeed = 5f;

			//charge sound: Item15
			//shoot sound: Item84
		}

		public override bool UseItemFrame(Player player) {
			//Only one Charge projectile should be out at once
			//Therefore, we can just check for the first active one and use it
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.owner == player.whoAmI && proj.ModProjectile is TerraBoltCharge charge) {
					player.bodyFrame.Y = charge.BodyIndex() * player.bodyFrame.Height;
					return true;
				}
			}

			return false;
		}

		public override bool HoldItemFrame(Player player)
			=> UseItemFrame(player);

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			speedX = 0;
			speedY = 0;
			return true;
		}

		public override bool SafeCanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;
	}
}
