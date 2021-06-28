using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Armor.Rockserpent{
	[AutoloadEquip(EquipType.Head)]
	public class RockserpentHelmet : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockserpent Helmet");
			Tooltip.SetDefault("Damage increased by 5%" +
				"\n+2 minion slots");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 16;
			item.rare = ItemRarityID.Orange;
			item.defense = 5;
			item.value = Item.sellPrice(gold: 1, silver: 35);
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
			=> body.type == ModContent.ItemType<RockserpentChestplate>() && legs.type == ModContent.ItemType<RockserpentLeggings>();

		public override void UpdateArmorSet(Player player){
			player.setBonus = "Weapons have a chance to inflict Primordial Wrath";
			player.GetModPlayer<ArmorsPlayer>().setBonus_Rockserpent = true;
		}

		public override void UpdateEquip(Player player){
			player.maxMinions += 2;
			player.allDamage += 0.05f;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 10);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.EvilDrops, 3);
			recipe.AddRecipeGroup("IronBar", 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
