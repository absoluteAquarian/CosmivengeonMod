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
			item.width = 26;
			item.height = 30;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 6;
			item.useAnimation = 6;
			item.useTurn = true;
			item.noMelee = true;
			item.maxStack = 1;
			item.rare = ItemRarityID.Blue;
			item.consumable = false;
			item.autoReuse = true;
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
				item.useTime = 6;
				item.useAnimation = 6;
				item.useStyle = ItemUseStyleID.HoldingOut;
				closestNPCtoMouse = Main.npc.Where(n => n.active).OrderBy(n => Vector2.Distance(Main.MouseWorld, n.Center)).First();
				canHealNPC = closestNPCtoMouse.MouseWithinRange(2 * 16);
			}else{
				item.useStyle = ItemUseStyleID.SwingThrow;
				item.useTime = 6;
				item.useAnimation = 6;
			}
			return true;
		}

		public override bool UseItem(Player player){
			int regen;
			Rectangle location;
			if(player.altFunctionUse == 2 && closestNPCtoMouse != null && canHealNPC){
				NPC actualNPC = closestNPCtoMouse.realLife >= 0 ? Main.npc[closestNPCtoMouse.realLife] : closestNPCtoMouse;
				regen = (int)Math.Max(Math.Ceiling(closestNPCtoMouse.lifeMax * 0.2f / 60f * item.useTime), 4);
				location = new Rectangle((int)closestNPCtoMouse.TopLeft.X - 5, (int)closestNPCtoMouse.Top.Y - 20, closestNPCtoMouse.width + 5, 20);
				actualNPC.life += regen;
				actualNPC.life.Clamp(0, actualNPC.lifeMax);
			}else if(player.altFunctionUse != 2){
				regen = (int)Math.Ceiling(player.statLifeMax2 * 0.25f / 60f * item.useTime);
				location = new Rectangle((int)player.TopLeft.X - 5, (int)player.Top.Y - 20, player.width + 5, 20);
				player.statLife += regen;
			}else
				return false;

			CombatText.NewText(location, CombatText.HealLife, regen, dot: true);

			return true;
		}
	}
}
