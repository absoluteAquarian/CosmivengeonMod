using CosmivengeonMod.Projectiles.Weapons.Frostbite;
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

		public override bool CanUseItem(Player player) {
			player.FindSentryRestingSpot(Item.shoot, out int worldX, out int worldY, out _);
			worldX >>= 4;
			worldY >>= 4;
			worldY--;

			return !WorldGen.SolidTile(worldX, worldY);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			//If any of this weapon's sentries exist, kill them
			for (int i = 0; i < 1000; i++) {
				if (Main.projectile[i]?.active == true && Main.projectile[i].ModProjectile is IceScepterWall)
					Main.projectile[i].Kill();
			}

			return true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Close mimic of the DD2 sentry placement logic
			player.FindSentryRestingSpot(type, out int worldX, out int worldY, out _);
			float pushYUp = ContentSamples.ProjectilesByType[Item.shoot].height / 2f;

			position = new Vector2(worldX, worldY - pushYUp);
			velocity = Vector2.Zero;
		}
	}
}
