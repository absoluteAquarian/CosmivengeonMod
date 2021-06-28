using CosmivengeonMod.Abilities;
using CosmivengeonMod.API.Commands;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools{
	public class StaminaMirror : HidableTooltip{
		public override string ItemName => "Energy Inspector";

		public override string FlavourText => GetFlavorText(Main.LocalPlayer);

		public override void SetDefaults(){
			item.width = 24;
			item.height = 24;
			item.value = Item.sellPrice(silver: 2, copper: 35);
			item.rare = ItemRarityID.Blue;
		}

		private string GetFlavorText(Player player){
			if(player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server)
				return "";

			//Get a copy of this player's Stamina and use it
			StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
			BossLogPlayer logPlayer = player.GetModPlayer<BossLogPlayer>();
			Stamina stamina = new Stamina(player);
			stamina.Clone(staminaPlayer.stamina);
			stamina.Reset();
			stamina.ApplyEffects();

			string bosses = "";
			foreach(var sbd in logPlayer.BossesKilled){
				if(sbd.mod == "Terraria")
					bosses += $"\"{ClearBossKilledCommand.GetBossNameFromDictionary(int.Parse(sbd.key))}\", ";
				else{
					var otherMod = ModLoader.GetMod(sbd.mod);
					//Don't display the name if the mod it's from isn't loaded
					if(otherMod == null)
						continue;

					int id = otherMod.NPCType(sbd.key);
					bosses += $"\"{ClearBossKilledCommand.GetBossNameFromDictionary(id)}\", ";
				}
			}

			if(bosses != ""){
				bosses = bosses.Remove(bosses.Length - 2, 2);

				StringBuilder sb = new StringBuilder(bosses.Length);

				var words = bosses.Split(',');
				int counter = 0;
				for(int i = 0; i < words.Length; i++){
					string word = words[i].Trim();

					sb.Append(word);

					counter++;
					if(counter < 4)
						sb.Append(", ");
					else{
						sb.Append(",\n  ");
						counter = 0;
					}
				}
			}else
				bosses = null;

			return
				$"\nMaximum:  {GetUnitsDiffString((int)(Stamina.DefaultMaxValue * 10000), stamina.MaxValue)}" +
				$"\nIncrease/Decrease Rates:" +
				$"\n  Active Increase: {GetRatesString(stamina.IncreaseRate, Stamina.DefaultIncreaseRate)}" +
				$"\n  Exhuasted Increase: {GetRatesString(stamina.ExhaustionIncreaseRate, Stamina.DefaultExhaustionIncreaseRate)}" +
				$"\n  Active Decrease: {GetRatesString(-stamina.DecreaseRate, -Stamina.DefaultDecreaseRate)}" +
				$"\nModifiers:" +
				$"\n  Active - Attack Speed: {GetPercentDiffString(Stamina.DefaultAttackSpeedBuff, stamina.AttackSpeedBuffMultiplier)}" +
				$"\n  Active - Move Acceleration: {GetPercentDiffString(Stamina.DefaultMoveSpeedBuff, stamina.MoveSpeedBuffMultiplier)}" +
				$"\n  Active - Max Move Speed: {GetPercentDiffString(Stamina.DefaultMaxMoveSpeedBuff, stamina.MaxMoveSpeedBuffMultiplier)}" +
				$"\n  Exhausted - Attack Speed: {GetPercentDiffString(Stamina.DefaultAttackSpeedDebuff, stamina.AttackSpeedDebuffMultiplier)}" +
				$"\n  Exhausted - Move Acceleration: {GetPercentDiffString(Stamina.DefaultMoveSpeedDebuff, stamina.MoveSpeedDebuffMultiplier)}" +
				$"\n  Exhausted - Max Move Speed: {GetPercentDiffString(Stamina.DefaultMaxMoveSpeedDebuff, stamina.MaxMoveSpeedDebuffMultiplier)}" +
				$"\nDefeated Bosses:" +
				$"\n  {bosses ?? "none"}";
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
