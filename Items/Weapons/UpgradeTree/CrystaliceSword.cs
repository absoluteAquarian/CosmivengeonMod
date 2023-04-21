using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.UpgradeTree {
	public class CrystaliceSword : ModItem {
		public override bool OnlyShootOnSwing/* tModPorter Note: Removed. If you returned true, set Item.useTime to a multiple of Item.useAnimation */ => true;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Sword");
			Tooltip.SetDefault("Has a chance to inflict [c/00dddd:Frostburn].");
		}

		public override void SetDefaults() {
			Item.damage = 15;
			Item.DamageType = DamageClass.Melee/* tModPorter Suggestion: Consider MeleeNoSpeed for no attack speed scaling */;
			Item.useTurn = false;
			Item.width = 50;
			Item.height = 50;
			Item.useTime = 40;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 4.3f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			if (Main.rand.NextFloat() < 0.5f)
				target.AddBuff(BuffID.Frostburn, 6 * 60);
		}
	}
}
