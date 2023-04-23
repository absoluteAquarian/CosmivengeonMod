using CosmivengeonMod.DamageClasses.Desolate;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek {
	public class TerraBolt : DesolatorItem {
		public override string ItemName => "Terra Bolt";

		public override string FlavourText =>
			"Hold left click to charge a cursed rock that inflicts Primordial Wrath" +
			"\nwhen striking a target." +
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
			Item.useStyle = ItemUseStyleID.DrinkLong;  //Custom use style
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

		public override void UseItemFrame(Player player) {
			// Only one Charge projectile should be out at once
			// Therefore, we can just check for the first active one and use it
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.owner == player.whoAmI && proj.ModProjectile is TerraBoltCharge charge) {
					// TODO: composite arm movement?

					player.bodyFrame.Y = charge.BodyIndex() * player.bodyFrame.Height;
					break;
				}
			}
		}

		public override void HoldItemFrame(Player player)
			=> UseItemFrame(player);

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = Vector2.Zero;
		}

		public override bool SafeCanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;
	}
}
