﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class IceforgedRelic : HidableTooltip{
		public override string ItemName => "Iceforged Relic";

		public override string FlavourText => "Unusual living conditions have always lead to strange adaptations." +
				"\nHundreds of years ago, Frostbite's distant ancestors lived in an" +
				"\nordinary Forest.  But one day, during the legendary battle between the" +
				"\nDraconian Gods and Cosmodod, a freezing self-aura spell casted by" +
				"\nCosmodod backfired, becoming a frozen bomb.  This bomb plummeted into" +
				"\nthat Forest, irreversibly transforming it into a frozen wasteland," +
				"\nkilling most of its inhabitants." +
				"\nThe ones that didn't perish adapted to their new frigid environment" +
				"\nand became a new species.  Frostibite appears to be the latest member" +
				"\nof this bizarre ecological miracle.";

		public override void SetDefaults(){
			item.maxStack = 1;
			item.rare = ItemRarityID.Blue;
			item.consumable = false;
		}

		public override bool CanUseItem(Player player) => false;
	}
}
