using CosmivengeonMod.Buffs.Mechanics;
using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using CosmivengeonMod.API;

namespace CosmivengeonMod.Abilities {
	/// <summary>
	/// The custom buff thing for Cosmivengeon.
	/// </summary>
	public class Stamina {
		public Color EnergizedColor {
			get {
				if (value > maxValue * FlashThreshold)
					return new Color(0x4c, 0xff, 0x00);  // Green
				else if (value > maxValue * FlashThreshold / 2)
					return new Color(0xe0, 0xc9, 0x00);  // Yellow

				return new Color(0xea, 0x23, 0x23);  // Red
			}
		}

		public const float ValueScalar = 10000f;

		public bool NoDecay;

		private float maxValue;
		public int MaxValue => (int)(maxValue * ValueScalar);
		private float value;
		public int Value => (int)(value * ValueScalar);
		public bool Active;
		private bool oldActive;
		public bool Exhaustion;
		public bool ApplyExhaustionDebuffs => Exhaustion && !exhaustionEffectsWornOff;
		private bool exhaustionEffectsWornOff;

		/// <summary>
		/// How quickly the stamina bar fills when the player is in the Idle state.
		/// Defaults to 1/4th the maximum Value per second (2500 units)
		/// </summary>
		public float RestorationRate { get; private set; }
		/// <summary>
		/// How quickly the stamina bar fills when the player is or was in the Exhausted state.
		/// Defaults to 1/8th the maximum Value per second (1250 units)
		/// </summary>
		public float ExhaustedRestorationRate { get; private set; }
		/// <summary>
		/// How quickly the stamina bar depletes when the player is in the Active state.
		/// Defaults to 1/5th the maximum Value per second (2000 units)
		/// </summary>
		public float ConsumptionRate { get; private set; }

		public const float DefaultMaxQuantity = 1f;
		public const float DefaultRestorationRate = DefaultMaxQuantity * 0.25f / 60f;
		public const float DefaultExhaustedRestorationRate = DefaultMaxQuantity * 0.125f / 60f;
		public const float DefaultConsumptionRate = DefaultMaxQuantity * 0.2f / 60f;
		public const float DefaultAttackSpeed = 1.35f;
		public const float DefaultExhaustedAttackSpeed = 2f / 3f;
		public const float DefaultMaxRunSpeed = 1.2f;
		public const float DefaultExhaustedMaxRunSpeed = 0.73f;
		public const float DefaultRunAcceleration = 4f / 3f;
		public const float DefaultExhaustedRunAcceleration = 0.84f;

		public StaminaStatModifier stats = StaminaStatModifier.Default;

		private const int RegenDelay = 45;
		private int regenDelayTimer;
		private bool startRegen;

		private int flashTimer;
		public const float FlashThreshold = 0.2f;
		public const int FlashCycle = 20;
		private int FlashDelay {
			get {
				float curRatio = value / FlashThreshold;
				curRatio = curRatio * curRatio * curRatio;
				curRatio.Clamp(0.3333f, 0.6667f);
				return (int)(FlashCycle * curRatio);
			}
		}

		public bool UnderThreshold => value < FlashThreshold;

		public float AttackPunishment { get; private set; }

		public bool Recharging => startRegen;

		public int BumpTimer = 20;

		private readonly List<(uint, bool)> queuedRestorations;

		public Stamina() {
			NoDecay = false;

			value = maxValue = DefaultMaxQuantity;
			Active = false;
			oldActive = false;
			Exhaustion = false;
			exhaustionEffectsWornOff = false;
			regenDelayTimer = RegenDelay;
			startRegen = false;
			RestorationRate = DefaultRestorationRate;
			ExhaustedRestorationRate = DefaultExhaustedRestorationRate;
			ConsumptionRate = DefaultConsumptionRate;

			stats = StaminaStatModifier.Default;

			queuedRestorations = new List<(uint, bool)>();
		}

		public void Clone(Stamina other) {
			value = other.value;
			Active = other.Active;
			oldActive = other.oldActive;
			Exhaustion = other.Exhaustion;
			exhaustionEffectsWornOff = other.exhaustionEffectsWornOff;
			maxValue = other.maxValue;
			NoDecay = other.NoDecay;
			startRegen = other.startRegen;
			regenDelayTimer = other.regenDelayTimer;
			stats = other.stats;
		}

		public void SendData(ModPacket packet) {
			BitsByte flag = new BitsByte(Active, oldActive, Exhaustion, exhaustionEffectsWornOff, NoDecay, startRegen);

			packet.Write(value);
			packet.Write(maxValue);
			packet.Write(regenDelayTimer);
			packet.Write(flag);
		}

		/// <summary>
		/// Assumes that this is being called within a syncing hook.
		/// </summary>
		public void ReceiveData(BinaryReader reader) {
			value = reader.ReadSingle();
			maxValue = reader.ReadSingle();
			regenDelayTimer = reader.ReadInt32();

			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref Active, ref oldActive, ref Exhaustion, ref exhaustionEffectsWornOff, ref NoDecay, ref startRegen);
		}

		public TagCompound GetTagCompound() => new TagCompound() {
			["value"] = value,
			["maxValue"] = maxValue
		};

		public void ParseCompound(TagCompound tag) {
			value = tag.GetFloat("value");
			maxValue = tag.GetFloat("maxValue");
		}

		public void Update(Player player) {
			if (!WorldEvents.desoMode) {
				if (value < 0.175f * maxValue)
					value = 0.175f * maxValue;

				return;
			}

			ProcessEffects(player);

			CapPunishment();

			HandleRestorations();

			if (NoDecay || player.GetModPlayer<DicePlayer>().noStaminaDecay) {
				value = maxValue;
				Exhaustion = false;
				exhaustionEffectsWornOff = true;
				startRegen = false;
				goto End;
			}

			//If enabled and it's just been depleted, activate Exhaustion
			if (!Exhaustion && Active && value - ConsumptionRate < 0f) {
				Active = false;
				Exhaustion = true;
				value = 0f;
			}

			if (oldActive != Active) {
				regenDelayTimer = RegenDelay;
				startRegen = false;
			}

			//Cap value at maxValue and deactivate Exhaustion if active
			float oldValue = value;

			if ((!Active || Exhaustion) && value + GetIncreaseRate() > maxValue) {
				Exhaustion = false;
				startRegen = false;
				exhaustionEffectsWornOff = false;
				value = maxValue;

				if (oldValue < maxValue)
					BumpTimer = 20;
			}

			//Check which decrease to use, increase otherwise
			if (Active && !Exhaustion)
				value -= ConsumptionRate;
			else if (!Active && startRegen)
				value += GetIncreaseRate();

			if (!startRegen && !Active && value < maxValue)
				regenDelayTimer--;
			if (regenDelayTimer == 0 && !startRegen)
				startRegen = true;
			flashTimer++;

			if (ApplyExhaustionDebuffs && value > 0.175f * maxValue)
				exhaustionEffectsWornOff = true;

End:
			oldActive = Active;

			//Apply a visual buff
			if (Active)
				player.AddBuff(ModContent.BuffType<EnergizedBuff>(), 2);
			else if (ApplyExhaustionDebuffs)
				player.AddBuff(ModContent.BuffType<ExhaustedDebuff>(), 2);

			if (value > maxValue)
				value = maxValue;

			//-0.035 per second while not using a weapon
			bool notUsingItem = player.itemAnimation == 0 && player.reuseDelay == 0;
			bool itemIsWeapon = !player.HeldItem.IsAir && player.HeldItem.damage > 0 && player.HeldItem.useStyle != ItemUseStyleID.None;
			if (AttackPunishment > 0 && (notUsingItem || !itemIsWeapon)) {
				AttackPunishment -= (Active ? 0.01f : 0.035f) / 60f;

				if (AttackPunishment < 0)
					AttackPunishment = 0;
			}
		}

		public int GetIconFrame() {
			if (Exhaustion)
				return 1;  // Gray
			else if (value > FlashThreshold)
				return 0;  // Blue
			else {
				var delay = FlashDelay;

				if (flashTimer % delay > delay / 2)
					return 3;  // Yellow

				return 2;  // Red
			}
		}

		public string GetHoverText() => $"{Value:D5}/{MaxValue} ({(1f - AttackPunishment) * 100:N1}%)";

		/// <summary>
		/// Called in ModPlayer.PostUpdateMiscEffects()
		/// </summary>
		public void ApplyAttackSpeed(Player player) {
			if (!WorldEvents.desoMode || (!Active && !ApplyExhaustionDebuffs))
				return;

			float apply = stats.attackSpeed.ApplyTo(DefaultAttackSpeed, DefaultExhaustedAttackSpeed, ApplyExhaustionDebuffs);

			player.GetAttackSpeed(DamageClass.Generic) += apply - 1f;
		}

		/// <summary>
		/// Called in ModPlayer.PostUpdateRunSpeeds()
		/// </summary>
		public void ApplyRunSpeedChanges(Player player) {
			if (!WorldEvents.desoMode || (!Active && !ApplyExhaustionDebuffs))
				return;

			stats.ApplyTo(ref player.maxRunSpeed, ref player.accRunSpeed, ref player.runAcceleration, ApplyExhaustionDebuffs);

			//If the player is exhausted, cap the horizontal speed
			if (ApplyExhaustionDebuffs)
				player.velocity.X.Clamp(-9f, 9f);
		}

		/// <summary>
		/// Called in ModPlayer.PreUpdate()
		/// </summary>
		public void ApplyFallSpeedDebuff(Player player) {
			if (!WorldEvents.desoMode || !ApplyExhaustionDebuffs)
				return;

			player.gravity *= 1.3f;
			player.maxFallSpeed *= 1.15f;
		}

		/// <summary>
		/// Resets this Stamina's rates to default.  Should only be called in a ResetEffects() hook.
		/// </summary>
		public void Reset() {
			stats = StaminaStatModifier.Default;
		}

		public void ApplyStats() {
			maxValue = stats.maxQuantity.ApplyTo(DefaultMaxQuantity);
			RestorationRate = stats.restorationRate.ApplyTo(DefaultRestorationRate, false);
			ExhaustedRestorationRate = stats.restorationRate.ApplyTo(DefaultExhaustedRestorationRate, true);
			ConsumptionRate = stats.consumptionRate.ApplyTo(DefaultConsumptionRate);
		}

		public void ProcessEffects(Player player) {
			//Apply the boss buffs, if any should be applied
			BossLogPlayer mp = player.GetModPlayer<BossLogPlayer>();

			foreach (var buff in StaminaBossKillBuffLoader.Filter(mp.BossesKilled)) {
				buff.ModifyStamina(out var npcStats);
				stats = stats.CombineWith(npcStats);
			}

			ApplyStats();

			//Apply the punishment
			RestorationRate *= 1f - AttackPunishment;
			ExhaustedRestorationRate *= 1f - AttackPunishment;
		}

		public float GetIncreaseRate() => Exhaustion ? ExhaustedRestorationRate : RestorationRate;

		public void ForceExhaustion() {
			value = 0;
			Exhaustion = true;
			Active = false;
		}

		public void AddAttackPunishment(float amount) {
			AttackPunishment += amount * 0.05f;

			CapPunishment();
		}

		private void CapPunishment() {
			if (AttackPunishment > 0.75f)
				AttackPunishment = 0.75f;
			if (AttackPunishment < 0)
				AttackPunishment = 0;
		}

		public void Restore(uint amount, bool scalesWithMax = false) {
			queuedRestorations.Add((amount, scalesWithMax));
		}

		private void HandleRestorations() {
			foreach (var (amount, scaled) in queuedRestorations) {
				float calculated = amount / (DefaultMaxQuantity * 10000);

				//Scale it with the max
				if (scaled)
					calculated *= maxValue;

				value += calculated;

				if (value > maxValue)
					value = maxValue;
			}

			queuedRestorations.Clear();
		}
	}
}
