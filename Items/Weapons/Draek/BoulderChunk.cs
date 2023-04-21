using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek{
	public class BoulderChunk : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockserpent Boulder");
			Tooltip.SetDefault("Hurls a crystal-infused boulder in the direction of" +
				"\nthe cursor.  Unable to be thrown far, but man, do they" +
				"\npack a real punch!");
		}

		public override void SetDefaults(){
			Item.shootSpeed = BoulderChunkProjectile.MAX_VELOCITY;
			Item.damage = 58;
			Item.knockBack = 7.9f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.width = 40;
			Item.height = 40;
			Item.maxStack = 999;
			Item.rare = ItemRarityID.Green;

			Item.consumable = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.DamageType = DamageClass.Throwing;

			Item.UseSound = SoundID.Item1;
			Item.value = Item.sellPrice(0, 0, 2, 75);

			Item.shoot = ModContent.ProjectileType<BoulderChunkProjectile>();
		}

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe(50);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
