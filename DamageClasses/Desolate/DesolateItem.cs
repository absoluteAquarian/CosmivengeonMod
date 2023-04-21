using CosmivengeonMod.API.Managers;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Utility.Extensions;
using CosmivengeonMod.Worlds;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CosmivengeonMod.DamageClasses.Desolate {
	public abstract class DesolatorItem : HidableTooltip {
		protected override bool CloneNewInstances => true;

		public override string FlavourText => CoreMod.Descriptions.PlaceHolder;

		public virtual void SafeSetDefaults() { }

		public sealed override void SetDefaults() {
			SafeSetDefaults();

			Item.melee = false/* tModPorter Suggestion: Remove. See Item.DamageType */;
			Item.ranged = false/* tModPorter Suggestion: Remove. See Item.DamageType */;
			Item.magic = false/* tModPorter Suggestion: Remove. See Item.DamageType */;
			Item.thrown = false/* tModPorter Suggestion: Remove. See Item.DamageType */;
			Item.summon = false/* tModPorter Suggestion: Remove. See Item.DamageType */;
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			add += player.Desolate().damageAdd;
			mult += player.Desolate().damageMult;
		}

		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) {
			knockback += player.Desolate().knockback;
		}

		public override void ModifyWeaponCrit(Player player, ref float crit) {
			crit += player.Desolate().crit;
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips) {
			TooltipLine damageLine = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.Mod == "Terraria");
			if (damageLine != null) {
				string[] splitText = damageLine.Text.Split(' ');
				string damageValue = splitText.First();
				string damageWord = splitText.Last();

				damageLine.Text = damageValue + " desolate " + damageWord;
			}

			int customIndex = FindCustomTooltipIndex(tooltips);
			if (customIndex > 0) {
				tooltips.Insert(customIndex++, new TooltipLine(Mod, "Desolator", "[c/6a00aa:Desolator]"));

				while (customIndex < tooltips.Count && tooltips[customIndex].Name.StartsWith("CustomTooltip"))
					customIndex++;

				TooltipLine line = new TooltipLine(Mod, "DesolationModeItem", "[c/adadad:Desolation Mode Item]");
				tooltips.Insert(customIndex, line);
			}
		}

		public virtual bool SafeCanUseItem(Player player) => true;

		public sealed override bool CanUseItem(Player player)
			=> WorldEvents.desoMode && SafeCanUseItem(player);

		public override int ChoosePrefix(UnifiedRandom rand) {
			WeightedRandom<int> wRand = new WeightedRandom<int>(rand);

			//Can't use normal means because source code go brrrrrrr
			//If an item fails the melee, ranged and magic weapon checks, then it won't be reforgeable if it ain't an accessory
			foreach (ModPrefix prefix in PrefixManager.desolatorPrefixes.Values)
				wRand.Add(prefix.Type, (prefix as DesolatorPrefix).Chance);

			return wRand.Get();
		}
	}
}
