using CosmivengeonMod.Desolator.Prefixes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CosmivengeonMod.Desolator{
	public abstract class DesolatorItem : HidableTooltip{
		public override bool CloneNewInstances => true;

		public override string FlavourText => CosmivengeonMod.PlaceHolderDescription;

		public virtual void SafeSetDefaults(){ }

		public sealed override void SetDefaults(){
			SafeSetDefaults();

			item.melee = false;
			item.ranged = false;
			item.magic = false;
			item.thrown = false;
			item.summon = false;
		}

		public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat){
			add += player.Desolate().damageAdd;
			mult += player.Desolate().damageMult;
		}

		public override void GetWeaponKnockback(Player player, ref float knockback){
			knockback += player.Desolate().knockback;
		}

		public override void GetWeaponCrit(Player player, ref int crit){
			crit += player.Desolate().crit;
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips){
			TooltipLine damageLine = tooltips.FirstOrDefault(x => x.Name == "Damage" && x.mod == "Terraria");
			if(damageLine != null){
				string[] splitText = damageLine.text.Split(' ');
				string damageValue = splitText.First();
				string damageWord = splitText.Last();
				
				damageLine.text = damageValue + " desolate " + damageWord;
			}

			int customIndex = FindCustomTooltipIndex(tooltips);
			if(customIndex > 0){
				tooltips.Insert(customIndex++, new TooltipLine(mod, "Desolator", "[c/6a00aa:Desolator]"));

				while(customIndex < tooltips.Count && tooltips[customIndex].Name.StartsWith("CustomTooltip"))
					customIndex++;

				TooltipLine line = new TooltipLine(mod, "DesolationModeItem", "[c/adadad:Desolation Mode Item]");
				tooltips.Insert(customIndex, line);
			}
		}

		public virtual bool SafeCanUseItem(Player player) => true;

		public sealed override bool CanUseItem(Player player)
			=> CosmivengeonWorld.desoMode && SafeCanUseItem(player);

		public override int ChoosePrefix(UnifiedRandom rand){
			WeightedRandom<int> wRand = new WeightedRandom<int>(rand);

			//Can't use normal means because source code go brrrrrrr
			//If an item fails the melee, ranged and magic weapon checks, then it won't be reforgeable if it ain't an accessory
			foreach(ModPrefix prefix in CosmivengeonMod.desolatorPrefixes.Values)
				wRand.Add(prefix.Type, (prefix as DesolatorPrefix).Chance);

			return wRand.Get();
		}
	}
}
