using CosmivengeonMod.Buffs.Minions;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek {
	public class BasiliskStaff : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Basilisk Staff");
			Tooltip.SetDefault("Summons a baby basilisk to fight for you.");
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.shoot = ModContent.ProjectileType<BabySnek>();
			Item.scale = 0.6667f;
			Item.width = 80;
			Item.height = 80;
			Item.UseSound = SoundID.Item44;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.rare = ItemRarityID.Green;
			Item.noMelee = true;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.buffType = ModContent.BuffType<BabyBasiliskBuff>();
			Item.mana = 12;
			Item.damage = 23;
			Item.knockBack = 2f;
			Item.DamageType = DamageClass.Summon;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(Item.buffType, 3600);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position = Main.MouseWorld;  //Make the summon spawn at the cursor
		}
	}
}
