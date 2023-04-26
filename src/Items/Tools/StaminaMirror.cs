using CosmivengeonMod.Abilities;
using CosmivengeonMod.API.Commands;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools {
	public class StaminaMirror : HidableTooltip {
		public override string ItemName => "Energy Inspector";

		public override string FlavourText => GetFlavorText(Main.LocalPlayer);

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.value = Item.sellPrice(silver: 2, copper: 35);
			Item.rare = ItemRarityID.Blue;
		}

		private static string GetFlavorText(Player player) {
			if (player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server)
				return "";

			//Get a copy of this player's Stamina and use it
			StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
			BossLogPlayer logPlayer = player.GetModPlayer<BossLogPlayer>();
			Stamina stamina = new Stamina();
			stamina.Clone(staminaPlayer.stamina);
			stamina.Reset();
			stamina.ProcessEffects(player);

			string bosses = "";
			foreach (var sbd in logPlayer.BossesKilled) {
				if (sbd.mod == "Terraria")
					bosses += $"\"{ClearBossKilledCommand.GetBossNameFromDictionary(int.Parse(sbd.key))}\", ";
				else {
					var otherMod = ModLoader.GetMod(sbd.mod);
					//Don't display the name if the mod it's from isn't loaded
					if (otherMod == null)
						continue;

					int id = otherMod.Find<ModNPC>(sbd.key).Type;
					bosses += $"\"{ClearBossKilledCommand.GetBossNameFromDictionary(id)}\", ";
				}
			}

			if (bosses != "") {
				bosses = bosses.Remove(bosses.Length - 2, 2);

				StringBuilder sb = new StringBuilder(bosses.Length);

				var words = bosses.Split(',');
				int counter = 0;
				for (int i = 0; i < words.Length; i++) {
					string word = words[i].Trim();

					sb.Append(word);

					counter++;
					if (counter < 4)
						sb.Append(", ");
					else {
						sb.Append(",\n  ");
						counter = 0;
					}
				}
			} else
				bosses = null;

			var maxQuantity = stamina.stats.maxQuantity.ApplyTo(Stamina.DefaultMaxQuantity);
			var restorationRate = stamina.stats.restorationRate.active.ApplyTo(Stamina.DefaultRestorationRate);
			var restorationRateExhausted = stamina.stats.restorationRate.exhausted.ApplyTo(Stamina.DefaultExhaustedRestorationRate);
			var consumptionRate = stamina.stats.consumptionRate.ApplyTo(Stamina.DefaultConsumptionRate);
			var attackSpeed = stamina.stats.attackSpeed.active.ApplyTo(Stamina.DefaultAttackSpeed);
			var runAcceleration = stamina.stats.runAcceleration.active.ApplyTo(Stamina.DefaultRunAcceleration);
			var maxRunSpeed = stamina.stats.maxRunSpeed.active.ApplyTo(Stamina.DefaultMaxRunSpeed);
			var attackSpeedExhausted = stamina.stats.attackSpeed.exhausted.ApplyTo(Stamina.DefaultExhaustedAttackSpeed);
			var runAccelerationExhausted = stamina.stats.runAcceleration.exhausted.ApplyTo(Stamina.DefaultExhaustedRunAcceleration);
			var maxRunSpeedExhausted = stamina.stats.maxRunSpeed.exhausted.ApplyTo(Stamina.DefaultExhaustedMaxRunSpeed);

			return
				$"\nMaximum:  {GetUnitsDiffString((int)(Stamina.DefaultMaxQuantity * Stamina.ValueScalar), (int)(maxQuantity * Stamina.ValueScalar))}" +
				$"\nRestoration/Consumption Rates:" +
				$"\n  Active Restoration: {GetRatesString(restorationRate, Stamina.DefaultRestorationRate)}" +
				$"\n  Exhuasted Restoration: {GetRatesString(restorationRateExhausted, Stamina.DefaultExhaustedRestorationRate)}" +
				$"\n  Active Consumption: {GetRatesString(-consumptionRate, -Stamina.DefaultConsumptionRate)}" +
				$"\nModifiers:" +
				$"\n  Active - Attack Speed: {GetPercentDiffString(Stamina.DefaultAttackSpeed, attackSpeed)}" +
				$"\n  Active - Run Acceleration: {GetPercentDiffString(Stamina.DefaultRunAcceleration, runAcceleration)}" +
				$"\n  Active - Max Run Speed: {GetPercentDiffString(Stamina.DefaultMaxRunSpeed, maxRunSpeed)}" +
				$"\n  Exhausted - Attack Speed: {GetPercentDiffString(Stamina.DefaultExhaustedAttackSpeed, attackSpeedExhausted)}" +
				$"\n  Exhausted - Run Acceleration: {GetPercentDiffString(Stamina.DefaultExhaustedRunAcceleration, runAccelerationExhausted)}" +
				$"\n  Exhausted - Max Run Speed: {GetPercentDiffString(Stamina.DefaultExhaustedMaxRunSpeed, maxRunSpeedExhausted)}" +
				$"\nDefeated Bosses:" +
				$"\n  {bosses ?? "none"}";
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup("Wood", 5);
			recipe.AddRecipeGroup("IronBar", 8);
			recipe.AddIngredient(ItemID.FallenStar, 2);
			recipe.AddIngredient(ItemID.Diamond, 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}

		private static string GetUnitsDiffString(int original, int current) {
			int diff = current - original;
			string sign = GetSign(diff);
			return $"{current} units ({sign}{diff} units)";
		}

		private static string GetPercentDiffString(float defaultRate, float current) {
			int cur = (int)((current - 1) * 100);
			int diff = (int)((current - defaultRate) * 100);
			string signCur = GetSign(cur);
			string signDiff = GetSign(diff);
			return $"{signCur}{cur:0.###}% ({signDiff}{diff:0.###}%)";
		}

		private static string GetRatesString(float current, float defaultRate) {
			current *= 60 * 10000;
			defaultRate *= 60 * 10000;
			int diff = (int)(current - defaultRate);
			string signCur = GetSign(current);
			string signDiff = GetSign(diff);
			return $"{signCur}{current:0.###} units/s ({signDiff}{diff:0.###} units/s)";
		}

		private static string GetSign(float value)
			=> value > 0 ? "+" : (value < 0 ? "" : "+/-");
	}
}
