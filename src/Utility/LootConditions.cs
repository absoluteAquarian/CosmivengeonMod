using CosmivengeonMod.Systems;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace CosmivengeonMod.Utility {
	public static class LootConditions {
		public class Lambda : IItemDropRuleCondition {
			public readonly Func<bool> action;
			public readonly string description;
			private readonly bool isDescriptionLangKey;

			private Lambda(Func<bool> action, string description, bool isDescriptionLangKey) {
				this.action = action;
				this.description = description;
				this.isDescriptionLangKey = isDescriptionLangKey;
			}

			public static Lambda FromLiteral(Func<bool> action, string text) => new(action, text, false);

			public static Lambda FromKey(Func<bool> action, string key) => new(action, key, true);

			public bool CanDrop(DropAttemptInfo info) => action();

			public bool CanShowItemDropInUI() => true;

			public string GetConditionDescription() => isDescriptionLangKey ? Language.GetTextValue(description) : description;
		}

		public class DesolationMode : IItemDropRuleCondition {
			public bool CanDrop(DropAttemptInfo info) => WorldEvents.desoMode;

			public bool CanShowItemDropInUI() => true;

			public string GetConditionDescription() => Language.GetTextValue("Mods.CosmivengeonMod.LootText.Desolation");
		}
	}
}
