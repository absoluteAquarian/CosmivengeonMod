using CosmivengeonMod.Utility;
using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Debugging{
	public class CrazyDiamondHand : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crazy Diamond's Hand");
			Tooltip.SetDefault(CoreMod.Descriptions.DebugItem +
				"\nWOAH IS THAT A JOJO'S REFERENCE??!?" +
				"\nJokes aside, this weapon will increase the user's" +
				"\nhealth by 25% of max HP per second." +
				"\nPress right click while hovering over an NPC to" +
				"\nheal them at 20% max HP per second instead!");
		}

		public override void SetDefaults(){
			Item.width = 26;
			Item.height = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.useTurn = true;
			Item.noMelee = true;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.Blue;
			Item.consumable = false;
			Item.autoReuse = true;
		}

		public override bool AltFunctionUse(Player player) => true;

		NPC closestNPCtoMouse = null;
		bool canHealNPC = false;

		public override bool CanUseItem(Player player){
			if(!Debug.debug_canUseCrazyHand)
				return false;

			closestNPCtoMouse = null;
			canHealNPC = false;
			if(player.altFunctionUse == 2){
				Item.useTime = 6;
				Item.useAnimation = 6;
				Item.useStyle = ItemUseStyleID.Shoot;
				closestNPCtoMouse = Main.npc.Where(n => n.active).OrderBy(n => Vector2.Distance(Main.MouseWorld, n.Center)).First();
				canHealNPC = closestNPCtoMouse.MouseWithinRange(2 * 16);
			}else{
				Item.useStyle = ItemUseStyleID.Swing;
				Item.useTime = 6;
				Item.useAnimation = 6;
			}
			return true;
		}

		public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */{
			int regen;
			Rectangle location;
			if(player.altFunctionUse == 2 && closestNPCtoMouse != null && canHealNPC){
				NPC actualNPC = closestNPCtoMouse.realLife >= 0 ? Main.npc[closestNPCtoMouse.realLife] : closestNPCtoMouse;
				regen = (int)Math.Max(Math.Ceiling(closestNPCtoMouse.lifeMax * 0.2f / 60f * Item.useTime), 4);
				location = new Rectangle((int)closestNPCtoMouse.TopLeft.X - 5, (int)closestNPCtoMouse.Top.Y - 20, closestNPCtoMouse.width + 5, 20);
				actualNPC.life += regen;
				actualNPC.life.Clamp(0, actualNPC.lifeMax);
			}else if(player.altFunctionUse != 2){
				regen = (int)Math.Ceiling(player.statLifeMax2 * 0.25f / 60f * Item.useTime);
				location = new Rectangle((int)player.TopLeft.X - 5, (int)player.Top.Y - 20, player.width + 5, 20);
				player.statLife += regen;
			}else
				return false;

			CombatText.NewText(location, CombatText.HealLife, regen, dot: true);

			return true;
		}
	}
}
