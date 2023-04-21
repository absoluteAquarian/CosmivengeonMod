using CosmivengeonMod.DataStructures;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.Items.Lore {
	public class StoneTablet : HidableTooltip {
		public override string ItemName => "Stone Tablet";

		public override string FlavourText => "Long ago, the ancient earth elemental Oronitus ruled over these lands." +
				"\nOne day, his legacy collapsed, and all his relics were cast into obscurity," +
				"\nburied beneath thousands of years of rock and legend.  Many years later," +
				"\nan innocent serpent stumbled across Oronitus's most prized possession," +
				"\nhis sacred jewel, and its mystical powers transformed the snake into the" +
				"\nterrifying guardian of the forest, Draek, capable of manipulating" +
				"\nthe terrain around him into forming arms, servants, and even granting the ability" +
				"\nto hover above the ground.  The earthen worm held his position fiercely, striking" +
				"\ndown any challengers with other relics recovered shortly after, until now, where" +
				"\nit appears that he has finally met his match...";

		public override void SetDefaults() {
			Item.maxStack = 1;
			Item.rare = ItemRarityID.Green;
			Item.consumable = false;
		}

		public override bool CanUseItem(Player player) {
			return false;
		}
	}
}
