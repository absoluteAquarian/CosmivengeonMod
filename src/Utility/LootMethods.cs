using CosmivengeonMod.Utility.LootRules;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace CosmivengeonMod.Utility {
	public static class LootMethods {
		public static LeadingConditionRule AddInstancedDrop(this ILoot loot, Func<bool> condition, int itemID, string desc, bool literal = true) {
			LeadingConditionRule rule = new LeadingConditionRule(literal ? LootConditions.Lambda.FromLiteral(condition, desc) : LootConditions.Lambda.FromKey(condition, desc));
			rule.OnSuccess(new InstancedPlayerLootRule(itemID, 1));
			loot.Add(rule);
			return rule;
		}
	}
}
