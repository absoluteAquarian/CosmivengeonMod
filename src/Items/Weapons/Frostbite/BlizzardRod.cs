using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite {
	public class BlizzardRod : ModItem {
		private static int LastCloudSpawnedIndex = -1;
		public static int[] SpawnedClouds = new int[2] { -1, -1 };

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blizzard Rod");
			Tooltip.SetDefault("Spawns a blizzard cloud at the cursor." +
				"\nUp to 2 clouds can exist at once.");
		}

		public override void SetDefaults() {
			Item.DamageType = DamageClass.Magic;
			Item.mana = 21;
			Item.width = 40;
			Item.height = 44;
			Item.noMelee = true;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 3, copper: 25);
			Item.UseSound = SoundID.Item30;
			Item.damage = 18;
			Item.knockBack = 4.15f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shoot = ModContent.ProjectileType<BlizzardRodProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			position = Main.MouseWorld;

			//Generate a new projectile
			int projWhoAmI = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

			//Go to the next index
			LastCloudSpawnedIndex = ++LastCloudSpawnedIndex % SpawnedClouds.Length;

			//If this index has a cloud already, kill the cloud that's there if it's still active
			if (SpawnedClouds[LastCloudSpawnedIndex] > 0) {
				var existing = Main.projectile[SpawnedClouds[LastCloudSpawnedIndex]];

				if (existing.active && existing.ModProjectile is BlizzardRodProjectile)
					existing.Kill();
			}

			//Replace the whoAmI at this index
			SpawnedClouds[LastCloudSpawnedIndex] = projWhoAmI;

			return false;
		}
	}
}
