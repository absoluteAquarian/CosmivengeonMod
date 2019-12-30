using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.SwordUpgrade{
	public class CrystaliceSword : ModItem{
		public static float SlowChance => 0.1428f;

		public override bool OnlyShootOnSwing => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Sword");
			Tooltip.SetDefault("Occasionally fires a slow-moving ice bolt with high damage." +
				"\nHas a chance to inflict frostburn.");
		}

		public override void SetDefaults(){
			item.damage = 21;
			item.melee = true;
			item.useTurn = false;
			item.width = 50;
			item.height = 50;
			item.useTime = 40;
			item.useAnimation = 28;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 5f;
			item.value = Item.sellPrice(silver: 50);
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.autoReuse = false;
			item.shoot = 10;
			item.shootSpeed = 10f;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Frostbite.FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit){
			if(Main.rand.NextFloat() < SlowChance)
				target.AddBuff(BuffID.Frostburn, 6 * 60);
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			type = ModContent.ProjectileType<Projectiles.SwordUpgrade.CrystaliceSwordProjectile>();
			damage = (int)(item.damage * 1.5f);
			return true;
		}
	}
}