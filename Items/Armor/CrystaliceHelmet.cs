using CosmivengeonMod.Items.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Armor{
	[AutoloadEquip(EquipType.Head)]
	public class CrystaliceHelmet : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Hood");
			Tooltip.SetDefault("Damage increased by 10%" +
				"\n+1 summon slot");
		}

		public override void SetDefaults(){
			item.width = 24;
			item.height = 26;
			item.defense = 2;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 8, copper: 40);
		}

		public override void UpdateEquip(Player player){
			player.allDamage += 0.1f;
			player.maxMinions++;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
			=> body.type == ModContent.ItemType<CrystaliceBreastplate>() && legs.type == ModContent.ItemType<CrystaliceLeggings>();

		public override void UpdateArmorSet(Player player){
			player.setBonus = "All attacks inflict [c/6fa8dc:Frostburn]";
			player.GetModPlayer<CosmivengeonPlayer>().setBonus_Crystalice = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}