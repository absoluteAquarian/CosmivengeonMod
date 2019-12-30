using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CosmivengeonMod{
	public class CosmivengeonGlobalItem : GlobalItem{
		public override void SetDefaults(Item item){
			//Allows the Frostfire's Breath to use the vanilla ice blocks as ammo
			//This might override the use of Ice Blocks by other mods!
			if(new int[]{ ItemID.IceBlock, ItemID.PinkIceBlock, ItemID.PurpleIceBlock, ItemID.RedIceBlock }.Contains(item.type))
				item.ammo = ItemID.IceBlock;
		}

		public override bool CanUseItem(Item item, Player player){
			//Check if this item is from Calamity
			if(CosmivengeonMod.CalamityActive && item.modItem?.mod == CosmivengeonMod.CalamityInstance){
				ModItem revengeanceItem = CosmivengeonMod.CalamityInstance.GetItem("Revenge");
				ModItem deathItem = CosmivengeonMod.CalamityInstance.GetItem("Death");

				//If the item is either of these types, disable Desolation mode if it is active
				//However, if the item is Death and revengeance isn't active, don't do anything
				if(item.type == deathItem.item.type && !(bool)CosmivengeonMod.CalamityInstance.Call("Difficulty", "Rev"))
					return false;
				if(CosmivengeonWorld.desoMode && item.type == revengeanceItem.item.type || item.type == deathItem.item.type)
					CosmivengeonWorld.desoMode = false;
			}

			return true;
		}
	}
}
