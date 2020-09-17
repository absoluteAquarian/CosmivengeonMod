using CosmivengeonMod.Commands;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StaminaAbility = CosmivengeonMod.Buffs.Stamina.Stamina;

namespace CosmivengeonMod.Items.Stamina{
	public class StaminaMirror : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Energy Inspector");
			Tooltip.SetDefault("Prints the statistics of your Stamina ability to the chat.");
		}

		public override void SetDefaults(){
			item.width = 24;
			item.height = 24;
			item.useStyle = ItemUseStyleID.HoldingUp;
			item.useTime = item.useAnimation = 20;
			item.value = Item.sellPrice(silver: 2, copper: 35);
			item.useTurn = false;
			item.autoReuse = false;
			item.rare = ItemRarityID.Blue;
		}

		public override bool UseItem(Player player){
			if(player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server)
				return false;

			//Get a copy of this player's Stamina and use it
			CosmivengeonPlayer mp = player.GetModPlayer<CosmivengeonPlayer>();
			StaminaAbility stamina = new StaminaAbility(player);
			stamina.Clone(mp.stamina);
			stamina.Reset();
			stamina.ApplyEffects();

			string bosses = "";
			foreach(int id in mp.BossesKilled){
				bosses += $"\"{ClearBossKilledCommand.GetBossNameFromDictionary(ClearBossKilledCommand.ConvertIDToTypeInDictionary(id))}\", ";
			}
			bosses = bosses.Remove(bosses.Length - 2, 2);

			string[] lines = ($"Stamina Stats for \"{player.name}\":" +
				$"\nMaximum:  {GetUnitsDiffString((int)(StaminaAbility.DefaultMaxValue * 10000), stamina.MaxValue)}" +
				$"\nIncrease/Decrease Rates:" +
				$"\n  Active Increase: {GetRatesString(stamina.IncreaseRate, StaminaAbility.DefaultIncreaseRate)}" +
				$"\n  Exhuasted Increase: {GetRatesString(stamina.ExhaustionIncreaseRate, StaminaAbility.DefaultExhaustionIncreaseRate)}" +
				$"\n  Active Decrease: {GetRatesString(-stamina.DecreaseRate, -StaminaAbility.DefaultDecreaseRate)}" +
				$"\nModifiers:" +
				$"\n  Active - Attack Speed: {GetPercentDiffString(StaminaAbility.DefaultAttackSpeedBuff, stamina.AttackSpeedBuffMultiplier)}" +
				$"\n  Active - Move Acceleration: {GetPercentDiffString(StaminaAbility.DefaultMoveSpeedBuff, stamina.MoveSpeedBuffMultiplier)}" +
				$"\n  Active - Max Move Speed: {GetPercentDiffString(StaminaAbility.DefaultMaxMoveSpeedBuff, stamina.MaxMoveSpeedBuffMultiplier)}" +
				$"\n  Exhausted - Attack Speed: {GetPercentDiffString(StaminaAbility.DefaultAttackSpeedDebuff, stamina.AttackSpeedDebuffMultiplier)}" +
				$"\n  Exhausted - Move Acceleration: {GetPercentDiffString(StaminaAbility.DefaultMoveSpeedDebuff, stamina.MoveSpeedDebuffMultiplier)}" +
				$"\n  Exhausted - Max Move Speed: {GetPercentDiffString(StaminaAbility.DefaultMaxMoveSpeedDebuff, stamina.MaxMoveSpeedDebuffMultiplier)}" +
				$"\nDefeated Bosses: {bosses}"
			).Split('\n');

			foreach(string line in lines)
				Main.NewText(line, Color.Gray);

			return true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("Wood", 5);
			recipe.AddRecipeGroup("IronBar", 8);
			recipe.AddIngredient(ItemID.FallenStar, 2);
			recipe.AddIngredient(ItemID.Diamond, 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		private string GetUnitsDiffString(int original, int current){
			int diff = current - original;
			string sign = GetSign(diff);
			return $"{current} units ({sign}{diff} units)";
		}

		private string GetPercentDiffString(float defaultRate, float multiplier){
			float current = defaultRate * multiplier;
			int cur = (int)((current - 1) * 100);
			int diff = (int)((current - defaultRate) * 100);
			string signCur = GetSign(cur);
			string signDiff = GetSign(diff);
			return $"{signCur}{cur}% ({signDiff}{diff}%)";
		}

		private string GetRatesString(float current, float defaultRate){
			current *= 60 * 10000;
			defaultRate *= 60 * 10000;
			int diff = (int)(current - defaultRate);
			string signCur = GetSign(current);
			string signDiff = GetSign(diff);
			return $"{signCur}{current} units/s ({signDiff}{diff} units/s)";
		}

		private string GetSign(float value)
			=> value > 0 ? "+" : (value < 0 ? "" : "+/-");
	}
}
