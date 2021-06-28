using CosmivengeonMod.DamageClasses.Desolate;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Managers{
	public static class PrefixManager{
		public static Dictionary<string, ModPrefix> desolatorPrefixes;

		public static void Load(){
			LoadDesolatePrefixes();
		}

		private static void LoadDesolatePrefixes(){
			/*  Desolator weapons have custom prefixes applied to them. The attributes of each prefix is listed below:
			 *  
			 *      Name     |       Rarity       | Category  |   Value multiplier   | Stat changes
			 *  --------------------------------------------------------------------------------------------------
			 *   "Pleasant"  |       Common       | AnyWeapon | Somewhat Undesirable | -5% damage, -8% knockback, -6% size
			 *   "Chaotic"   |       Common       | AnyWeapon |   Somewhat Valuable  | +3% damage, +1% knockback, +3% size
			 *    "Unholy"   |      Uncommon      | AnyWeapon |       Valuable       | +6% damage, -6% use speed, +6% crit
			 *   "Prideful"  |    Very Uncommon   | AnyWeapon |     Very Valuable    | +9% damage, +10% shoot speed, -12% size, -4% crit
			 *   "Indolent"  |        Rare        | AnyWeapon |     Very Valuable    | +8% knockback, +12% use speed, +18% size, -23% shoot speed
			 *  "Chaelosmic" | Exceptionally Rare | AnyWeapon |       Priceless      | +12% damage, +9% knockback, +14% size, -15% use speed, +34% shoot speed, +15% crit
			 *  
			 */
			desolatorPrefixes = new Dictionary<string, ModPrefix>();

			AddDesolatePrefixType(DesolatorPrefixName.Pleasant, DesolatorPrefixChance.Common, DesolatorPrefixValue.SomewhatUndesireable,
				PrefixCategory.AnyWeapon,
				damageMult: 0.95f, knockbackMult: 0.92f, scaleMult: 0.94f);
			AddDesolatePrefixType(DesolatorPrefixName.Chaotic, DesolatorPrefixChance.Common, DesolatorPrefixValue.SomewhatValuable,
				PrefixCategory.AnyWeapon,
				damageMult: 1.03f, knockbackMult: 1.01f, scaleMult: 1.03f);
			AddDesolatePrefixType(DesolatorPrefixName.Unholy, DesolatorPrefixChance.Uncommon, DesolatorPrefixValue.Valuable,
				PrefixCategory.AnyWeapon,
				damageMult: 1.06f, useTimeMult: 0.94f, critBonus: 6);
			AddDesolatePrefixType(DesolatorPrefixName.Prideful, DesolatorPrefixChance.VeryUncommon, DesolatorPrefixValue.VeryValuable,
				PrefixCategory.AnyWeapon,
				damageMult: 1.09f, scaleMult: 0.88f, shootSpeedMult: 1.1f, critBonus: -4);
			AddDesolatePrefixType(DesolatorPrefixName.Slothful, DesolatorPrefixChance.Rare, DesolatorPrefixValue.VeryValuable,
				PrefixCategory.AnyWeapon,
				knockbackMult: 1.08f, useTimeMult: 1.12f, scaleMult: 1.18f, shootSpeedMult: 0.77f);
			AddDesolatePrefixType(DesolatorPrefixName.Chaelosmic, DesolatorPrefixChance.ExceptionallyRare, DesolatorPrefixValue.Priceless,
				PrefixCategory.AnyWeapon,
				damageMult: 1.12f, knockbackMult: 1.09f, useTimeMult: 0.85f, scaleMult: 1.14f, shootSpeedMult: 1.34f, critBonus: 15);
		}

		public static void Unload(){
			desolatorPrefixes = null;
		}

		private static void AddDesolatePrefixType(string name, float chance, float valueMult, PrefixCategory category, float damageMult = 1f, float knockbackMult = 1f, float useTimeMult = 1f, float scaleMult = 1f, float shootSpeedMult = 1f, float manaMult = 1f, int critBonus = 0){
			CoreMod.Instance.AddPrefix(name, new DesolatorPrefix(chance, valueMult, category, damageMult, knockbackMult, useTimeMult, scaleMult, shootSpeedMult, manaMult, critBonus));

			desolatorPrefixes.Add(name, CoreMod.Instance.GetPrefix(name));
		}

		public static DesolatorPrefix GetDesolatorPrefix(byte type){
			foreach(ModPrefix prefix in desolatorPrefixes.Values)
				if(prefix.Type == type)
					return prefix as DesolatorPrefix;

			return null;
		}

		public static void ApplyDesolatorPrefix(Item item, DesolatorPrefix prefix){
			//I'm having to do this manually because the prefix code is fucking awful and I hate it
			item.damage = (int)Math.Round(item.damage * prefix.DamageMultiplier);
			item.knockBack *= prefix.KnockbackMultiplier;
			item.useTime = item.useAnimation = (int)Math.Round(item.useAnimation * prefix.UseTimeMultiplier);
			item.scale *= prefix.ScaleMultiplier;
			item.shootSpeed *= prefix.ShootSpeedMultiplier;
			item.mana = (int)Math.Round(item.mana * prefix.ManaMultiplier);
			item.crit += prefix.CritBonus;

			float valueMult = prefix.ValueMultiplier;
			prefix.PostApply(item, valueMult);

			if(item.rare > ItemRarityID.Expert + 1){
				if(item.rare < ItemRarityID.Gray)
					item.rare = ItemRarityID.Gray;

				if(item.rare > ItemRarityID.Purple)
					item.rare = ItemRarityID.Purple;
			}

			valueMult *= valueMult;
			item.value = (int)(item.value * valueMult);
			item.prefix = prefix.Type;
		}
	}
}
