using CosmivengeonMod.DamageClasses.Desolate;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Managers {
	public static class PrefixManager {
		private class Loadable : ILoadable {
			public void Load(Mod mod) {
				PrefixManager.Load();
			}

			public void Unload() {
				PrefixManager.Unload();
			}
		}

		public static Dictionary<string, ModPrefix> desolatorPrefixes;

		private static void Load() {
			LoadDesolatePrefixes();
		}

		private static void LoadDesolatePrefixes() {
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

		private static void Unload() {
			desolatorPrefixes = null;
		}

		private static void AddDesolatePrefixType(string name, float chance, float valueMult, PrefixCategory category, float damageMult = 1f, float knockbackMult = 1f, float useTimeMult = 1f, float scaleMult = 1f, float shootSpeedMult = 1f, float manaMult = 1f, int critBonus = 0) {
			var prefix = new DesolatorPrefix(name, chance, valueMult, category, damageMult, knockbackMult, useTimeMult, scaleMult, shootSpeedMult, manaMult, critBonus);
			CoreMod.Instance.AddContent(prefix);

			desolatorPrefixes.Add(name, prefix);
		}

		public static DesolatorPrefix GetDesolatorPrefix(int type) {
			foreach (ModPrefix prefix in desolatorPrefixes.Values)
				if (prefix.Type == type)
					return prefix as DesolatorPrefix;

			return null;
		}
	}
}
