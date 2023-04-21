using System;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.DamageClasses.Desolate{
	public sealed class DesolatorPrefix : ModPrefix{
		private readonly PrefixCategory category;
		public override PrefixCategory Category => category;

		public sealed override bool CanRoll(Item item) => true;

		public readonly float Chance = DesolatorPrefixChance.Common;
		public readonly float ValueMultiplier = DesolatorPrefixValue.Normal;
		public readonly float DamageMultiplier = 1f;
		public readonly float KnockbackMultiplier = 1f;
		public readonly float UseTimeMultiplier = 1f;
		public readonly float ScaleMultiplier = 1f;
		public readonly float ShootSpeedMultiplier = 1f;
		public readonly float ManaMultiplier = 1f;
		public readonly int CritBonus = 0;

		//No RollChance because source code go brrrrrrrr

		public DesolatorPrefix(){ }

		/// <summary>
		/// Creates a new <seealso cref="DesolatorPrefix"/>, a custom prefix that can be applied to Desolator items.
		/// </summary>
		/// <param name="chance">The weighted chance to get this prefix.</param>
		/// <param name="valueMult">The multiplier applied to the value of the item.</param>
		/// <param name="category">The <seealso cref="PrefixCategory"/> this prefix is assigned to.</param>
		/// <param name="damageMult">The damage multiplier.</param>
		/// <param name="knockbackMult">The knockback multiplier.</param>
		/// <param name="useTimeMult">The use time multiplier.</param>
		/// <param name="scaleMult">The size multiplier.</param>
		/// <param name="shootSpeedMult">The shot projectile speed modifier.</param>
		/// <param name="manaMult">The mana usage modifier.</param>
		/// <param name="critBonus">The crit bonus.</param>
		public DesolatorPrefix(float chance, float valueMult, PrefixCategory category, float damageMult = 1f, float knockbackMult = 1f, float useTimeMult = 1f, float scaleMult = 1f, float shootSpeedMult = 1f, float manaMult = 1f, int critBonus = 0){
			Chance = chance;
			ValueMultiplier = valueMult;
			this.category = category;
			DamageMultiplier = damageMult;
			KnockbackMultiplier = knockbackMult;
			UseTimeMultiplier = useTimeMult;
			ScaleMultiplier = scaleMult;
			ShootSpeedMultiplier = shootSpeedMult;
			ManaMultiplier = manaMult;
			CritBonus = critBonus;
		}

		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus){
			damageMult += DamageMultiplier - 1;
			knockbackMult += KnockbackMultiplier - 1;
			useTimeMult += UseTimeMultiplier - 1;
			scaleMult += ScaleMultiplier - 1;
			shootSpeedMult += ShootSpeedMultiplier - 1;
			manaMult += ManaMultiplier - 1;
			critBonus += CritBonus;
		}

		public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ => false;

		public override void ModifyValue(ref float valueMult) => valueMult = ValueMultiplier;

		public override void Apply(Item item){
			//Desolator prefixes will affect rarity differently than vanilla...
			float valueMult = ValueMultiplier;

			//Do the inverse of what Item.Prefix(int) does
			if(valueMult >= 1.2f)
				item.rare -= 2;
			else if(valueMult >= 1.05f)
				item.rare--;
			else if(valueMult <= 0.8f)
				item.rare += 2;
			else if(valueMult <= 0.95f)
				item.rare++;

			//Then apply the custom rarity modification
			PostApply(item, valueMult);
		}

		public void PostApply(Item item, float valueMult){
			switch(valueMult){
				case DesolatorPrefixValue.Priceless:
					item.rare += 3;
					break;
				case DesolatorPrefixValue.ExceptionallyValuable:
				case DesolatorPrefixValue.VeryValuable:
					item.rare += 2;
					break;
				case DesolatorPrefixValue.Valuable:
				case DesolatorPrefixValue.SomewhatValuable:
					item.rare++;
					break;
				case DesolatorPrefixValue.Normal:
					break;
				case DesolatorPrefixValue.SomewhatUndesireable:
				case DesolatorPrefixValue.Undesireable:
					item.rare--;
					break;
				case DesolatorPrefixValue.VeryUndesireable:
				case DesolatorPrefixValue.ExceptionallyUndesireable:
					item.rare -= 2;
					break;
				case DesolatorPrefixValue.UtterTrash:
					item.rare -= 3;
					break;
				default:
					throw new InvalidOperationException("Desolator item used a value multiplier not in \"DesolatorPrefixValue\".");
			}
		}
	}
}
