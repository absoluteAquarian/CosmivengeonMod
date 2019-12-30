using CosmivengeonMod.Items.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Armor{
	[AutoloadEquip(EquipType.Head)]
	public class RockserpentHelmet : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockserpent Helm");
			Tooltip.SetDefault("Damage increased by 5%" +
				"\n+2 summon slots");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 16;
			item.rare = ItemRarityID.Orange;
			item.defense = 5;
			item.value = Item.sellPrice(gold: 1, silver: 35);
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
			=> head.type == ModContent.ItemType<RockserpentHelmet>()
				&& body.type == ModContent.ItemType<RockserpentChestplate>()
				&& legs.type == ModContent.ItemType<RockserpentLeggings>();

		public override void UpdateArmorSet(Player player){
			player.setBonus = "All attacks inflict poison\nAll weapons have a 10% chance to fire a Draek energy blast upon usage";
			player.GetModPlayer<CosmivengeonPlayer>().setBonus_Rockserpent = true;
		}

		public override void UpdateEquip(Player player){
			player.maxMinions += 2;
			player.allDamage += 0.05f;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 10);
			recipe.AddRecipeGroup("Cosmivengeon: Evil Drops", 3);
			recipe.AddRecipeGroup("IronBar", 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
