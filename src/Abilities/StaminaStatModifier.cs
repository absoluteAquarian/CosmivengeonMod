using Terraria.ModLoader;

namespace CosmivengeonMod.Abilities {
	public struct StaminaStatModifier {
		public StaminaStatModifierPair restorationRate;
		public StatModifier consumptionRate;
		public StatModifier maxQuantity;
		public StaminaStatModifierPair attackSpeed;
		public StaminaStatModifierPair maxRunSpeed;
		public StaminaStatModifierPair runAcceleration;

		public static readonly StaminaStatModifier Default = new StaminaStatModifier();

		public StaminaStatModifier() {
			restorationRate = StaminaStatModifierPair.Default;
			consumptionRate = StatModifier.Default;
			maxQuantity = StatModifier.Default;
			attackSpeed = StaminaStatModifierPair.Default;
			maxRunSpeed = StaminaStatModifierPair.Default;
			runAcceleration = StaminaStatModifierPair.Default;
		}

		public StaminaStatModifier(StaminaStatModifierPair restorationRate, StatModifier consumptionRate, StatModifier maxQuantity, StaminaStatModifierPair attackSpeed, StaminaStatModifierPair maxRunSpeed, StaminaStatModifierPair runAcceleration) {
			this.restorationRate = restorationRate;
			this.consumptionRate = consumptionRate;
			this.maxQuantity = maxQuantity;
			this.attackSpeed = attackSpeed;
			this.maxRunSpeed = maxRunSpeed;
			this.runAcceleration = runAcceleration;
		}

		public StaminaStatModifier CombineWith(StaminaStatModifier other)
			=> new StaminaStatModifier(restorationRate.CombineWith(other.restorationRate),
				consumptionRate.CombineWith(other.consumptionRate),
				maxQuantity.CombineWith(other.maxQuantity),
				attackSpeed.CombineWith(other.attackSpeed),
				maxRunSpeed.CombineWith(other.maxRunSpeed),
				runAcceleration.CombineWith(other.runAcceleration));

		public void ApplyTo(ref float maxRunSpeed, ref float accRunSpeed, ref float runAcceleration, bool isExhausted) {
			maxRunSpeed = this.maxRunSpeed.ApplyTo(maxRunSpeed, isExhausted);
			accRunSpeed = this.maxRunSpeed.ApplyTo(accRunSpeed, isExhausted);
			runAcceleration = this.runAcceleration.ApplyTo(runAcceleration, isExhausted);
		}
	}

	public struct StaminaStatModifierPair {
		public StatModifier active;
		public StatModifier exhausted;

		public static readonly StaminaStatModifierPair Default = new StaminaStatModifierPair();

		public StaminaStatModifierPair() {
			active = StatModifier.Default;
			exhausted = StatModifier.Default;
		}

		public StaminaStatModifierPair(StatModifier active, StatModifier exhausted) {
			this.active = active;
			this.exhausted = exhausted;
		}

		public StaminaStatModifierPair CombineWith(StaminaStatModifierPair other)
			=> new StaminaStatModifierPair(active.CombineWith(other.active), exhausted.CombineWith(other.exhausted));

		public float ApplyTo(float value, bool isExhausted)
			=> (isExhausted ? exhausted : active).ApplyTo(value);

		public float ApplyTo(float activeValue, float exhaustedValue, bool isExhausted)
			=> isExhausted
				? exhausted.ApplyTo(exhaustedValue)
				: active.ApplyTo(activeValue);
	}
}
