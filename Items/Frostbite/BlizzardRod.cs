using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class BlizzardRod : ModItem{
		private int LastCloudSpawnedIndex = -1;
		public int[] SpawnedClouds = new int[2]{ -1, -1 };

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Blizzard Rod");
			Tooltip.SetDefault("Spawns a blizzard cloud at the cursor." +
				"\nUp to 2 clouds can exist at once.");
		}

		public override void SetDefaults(){
			item.magic = true;
			item.mana = 21;
			item.width = 40;
			item.height = 44;
			item.noMelee = true;
			item.useTime = 24;
			item.useAnimation = 24;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 3, copper: 25);
			item.UseSound = SoundID.Item30;
			item.damage = 18;
			item.knockBack = 4.15f;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.shoot = ModContent.ProjectileType<Projectiles.Weapons.Frostbite.BlizzardRodProjectile>();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			position = Main.MouseWorld;

			//Generate a new projectile
			int projWhoAmI = Projectile.NewProjectile(position, Vector2.Zero, type, damage, knockBack);

			//Go to the next index
			LastCloudSpawnedIndex = ++LastCloudSpawnedIndex % SpawnedClouds.Length;

			//If this index has a cloud already, kill the cloud that's there if it's still active
			if(SpawnedClouds[LastCloudSpawnedIndex] > 0 && Main.projectile[SpawnedClouds[LastCloudSpawnedIndex]].active)
				Main.projectile[SpawnedClouds[LastCloudSpawnedIndex]].Kill();

			//Replace the whoAmI at this index
			SpawnedClouds[LastCloudSpawnedIndex] = projWhoAmI;

			return false;
		}
	}
}
