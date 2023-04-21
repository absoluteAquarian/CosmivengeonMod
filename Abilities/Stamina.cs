using CosmivengeonMod.Buffs.Mechanics;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Utility.Extensions;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Abilities {
	/// <summary>
	/// The custom buff thing for Cosmivengeon.
	/// </summary>
	public class Stamina {
		public static Color EnergizedColor => new Color(0x4c, 0xff, 0x00);
		public Player Parent { get; private set; }

		public bool NoDecay;

		private float maxValue;
		public int MaxValue => (int)(maxValue * 10000f);
		private float value;
		public int Value => (int)(value * 10000f);
		public bool Active;
		private bool oldActive;
		public bool Exhaustion;
		public bool ApplyExhaustionDebuffs => Exhaustion && !ExhaustionEffectsWornOff;
		private bool ExhaustionEffectsWornOff;

		/// <summary>
		/// How quickly the stamina bar fills when the player is in the Idle state.
		/// Defaults to 1/4th the maximum Value per second (2500 units)
		/// </summary>
		public float IncreaseRate { get; private set; }
		/// <summary>
		/// How quickly the stamina bar fills when the player is or was in the Exhausted state.
		/// Defaults to 1/8th the maximum Value per second (1250 units)
		/// </summary>
		public float ExhaustionIncreaseRate { get; private set; }
		/// <summary>
		/// How quickly the stamina bar depletes when the player is in the Active state.
		/// Defaults to 1/5th the maximum Value per second (2000 units)
		/// </summary>
		public float DecreaseRate { get; private set; }

		public static float DefaultIncreaseRate => DefaultMaxValue * 0.25f / 60f;
		public static float DefaultExhaustionIncreaseRate => DefaultMaxValue * 0.125f / 60f;
		public static float DefaultDecreaseRate => DefaultMaxValue * 0.2f / 60f;
		public static float DefaultMaxValue => 1f;
		public static float DefaultAttackSpeedBuff => 1.35f;
		public static float DefaultAttackSpeedDebuff => 0.6667f;
		public static float DefaultMaxMoveSpeedBuff => 1.2f;
		public static float DefaultMaxMoveSpeedDebuff => 0.73f;
		public static float DefaultMoveSpeedBuff => 1.33f;
		public static float DefaultMoveSpeedDebuff => 0.84f;

		public ref float AttackSpeedBuffMultiplier => ref Multipliers[4];
		public ref float AttackSpeedDebuffMultiplier => ref Multipliers[7];
		public ref float MaxMoveSpeedBuffMultiplier => ref Multipliers[5];
		public ref float MaxMoveSpeedDebuffMultiplier => ref Multipliers[9];
		public ref float MoveSpeedBuffMultiplier => ref Multipliers[7];
		public ref float MoveSpeedDebuffMultiplier => ref Multipliers[8];

		public float[] Multipliers = DefaultMults;
		public float[] Adders = DefaultAdds;

		private static float[] DefaultMults => MiscUtils.CreateArray(1f, 10);
		private static float[] DefaultAdds => MiscUtils.CreateArray(0f, 4);

		private const int RegenDelay = 45;
		private int RegenDelayTimer;
		private bool startRegen;

		private int FlashTimer;
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

		public static Vector2 BackDrawPos => new Vector2(Main.screenWidth - 650, 30);
		public static Vector2 BarDrawPos => BackDrawPos + new Vector2(58, 14);

		public readonly bool Empty = false;

		public float AttackPunishment { get; private set; }

		public bool Recharging => startRegen;

		public int BumpTimer = 20;

		private readonly List<(uint, bool)> queuedAdds;

		public Stamina(Player player = null) {
			if (player is null) {
				Empty = true;
				return;
			}

			Parent = player;
			NoDecay = false;

			value = DefaultMaxValue;
			maxValue = DefaultMaxValue;
			Active = false;
			oldActive = false;
			Exhaustion = false;
			ExhaustionEffectsWornOff = false;
			RegenDelayTimer = RegenDelay;
			startRegen = false;
			IncreaseRate = DefaultIncreaseRate;
			ExhaustionIncreaseRate = DefaultExhaustionIncreaseRate;
			DecreaseRate = DefaultDecreaseRate;

			queuedAdds = new List<(uint, bool)>();
		}

		public void Clone(Stamina other) {
			if (Empty)
				return;

			value = other.value;
			Active = other.Active;
			oldActive = other.oldActive;
			Exhaustion = other.Exhaustion;
			ExhaustionEffectsWornOff = other.ExhaustionEffectsWornOff;
			maxValue = other.maxValue;
			NoDecay = other.NoDecay;
			startRegen = other.startRegen;
			RegenDelayTimer = other.RegenDelayTimer;
			Parent = other.Parent;
			Multipliers = other.Multipliers;
			Adders = other.Adders;
		}

		public void SendData(ModPacket packet) {
			if (Empty)
				return;

			BitsByte flag = new BitsByte(Active, oldActive, Exhaustion, ExhaustionEffectsWornOff, NoDecay, startRegen);

			packet.Write(value);
			packet.Write(maxValue);
			packet.Write(RegenDelayTimer);
			packet.Write(flag);
			packet.Write((byte)Parent.whoAmI);
		}

		/// <summary>
		/// Assumes that this is being called within a syncing hook.
		/// </summary>
		public void ReceiveData(BinaryReader reader) {
			value = reader.ReadSingle();
			maxValue = reader.ReadSingle();
			RegenDelayTimer = reader.ReadInt32();

			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref Active, ref oldActive, ref Exhaustion, ref ExhaustionEffectsWornOff, ref NoDecay, ref startRegen);

			Parent = Main.player[reader.ReadByte()];
		}

		public TagCompound GetTagCompound() => new TagCompound() {
			["value"] = value,
			["maxValue"] = maxValue,
			["multipliers"] = Multipliers.ToList(),
			["adders"] = Adders.ToList()
		};

		public void ParseCompound(TagCompound tag) {
			value = tag.GetFloat("value");
			maxValue = tag.GetFloat("maxValue");
			//These lists in the tag can be 'null' if this is a new character or one that doesn't have any Stamina data yet
			Multipliers = tag.GetList<float>("multipliers")?.ToArray() ?? DefaultMults;
			Adders = tag.GetList<float>("adders")?.ToArray() ?? DefaultAdds;
		}

		public void Update() {
			if (Empty)
				return;

			if (!WorldEvents.desoMode) {
				if (value < 0.175f * maxValue)
					value = 0.175f * maxValue;
				return;
			}

			ApplyEffects();

			CapPunishment();

			HandleAdds();

			if (NoDecay || Parent.GetModPlayer<DicePlayer>().noStaminaDecay) {
				value = maxValue;
				Exhaustion = false;
				ExhaustionEffectsWornOff = true;
				startRegen = false;
				goto End;
			}

			//If enabled and it's just been depleted, activate Exhaustion
			if (!Exhaustion && Active && value - DecreaseRate < 0f) {
				Active = false;
				Exhaustion = true;
				value = 0f;
			}

			if (oldActive != Active) {
				RegenDelayTimer = RegenDelay;
				startRegen = false;
			}

			//Cap value at maxValue and deactivate Exhaustion if active
			float oldValue = value;

			if ((!Active || Exhaustion) && value + GetIncreaseRate() > maxValue) {
				Exhaustion = false;
				startRegen = false;
				ExhaustionEffectsWornOff = false;
				value = maxValue;

				if (oldValue < maxValue)
					BumpTimer = 20;
			}

			//Check which decrease to use, increase otherwise
			if (Active && !Exhaustion)
				value -= DecreaseRate;
			else if (!Active && startRegen)
				value += GetIncreaseRate();

			if (!startRegen && !Active && value < maxValue)
				RegenDelayTimer--;
			if (RegenDelayTimer == 0 && !startRegen)
				startRegen = true;
			FlashTimer++;

			if (ApplyExhaustionDebuffs && value > 0.175f * maxValue)
				ExhaustionEffectsWornOff = true;

End:
			oldActive = Active;

			//Apply a visual buff
			if (Active)
				Parent.AddBuff(ModContent.BuffType<EnergizedBuff>(), 2);
			else if (ApplyExhaustionDebuffs)
				Parent.AddBuff(ModContent.BuffType<ExhaustedDebuff>(), 2);

			if (value > maxValue)
				value = maxValue;

			//-0.035 per second while not using a weapon
			bool notUsingItem = Parent.itemAnimation == 0 && Parent.reuseDelay == 0;
			bool itemIsWeapon = !Parent.HeldItem.IsAir && Parent.HeldItem.damage > 0 && Parent.HeldItem.useStyle != 0;
			if (AttackPunishment > 0 && (notUsingItem || !itemIsWeapon)) {
				AttackPunishment -= (Active ? 0.01f : 0.035f) / 60f;

				if (AttackPunishment < 0)
					AttackPunishment = 0;
			}
		}

		public Texture2D GetBackTexture() {
			if (Empty)
				return TextureAssets.MagicPixel.Value;

			return ModContent.GetTexture("CosmivengeonMod/Abilities/BarFrame");
		}

		public Texture2D GetBarTexture() {
			if (Empty)
				return TextureAssets.MagicPixel.Value;

			return ModContent.GetTexture("CosmivengeonMod/Abilities/Bar");
		}

		public Texture2D GetIconTexture() {
			if (Empty)
				return TextureAssets.MagicPixel.Value;

			string name;
			if (Exhaustion)
				name = "_Gray";
			else if (value > FlashThreshold)
				name = "";
			else {
				if (FlashTimer % FlashDelay > FlashDelay / 2)
					name = "_Yellow";
				else
					name = "_Red";
			}

			return ModContent.GetTexture($"Cosmivengeon/Abilities/BarFrameEnergyIcon{name}");
		}

		/// <summary>
		/// Returns a 0 for Red and a 1 for Yellow
		/// </summary>
		public int GetFlashType() => FlashTimer % FlashDelay > FlashDelay / 2 ? 1 : 0;

		public Rectangle GetBarRect() {
			Texture2D bar = GetBarTexture();
			return Empty ? Rectangle.Empty : new Rectangle(2, 2, bar.Width - 4, bar.Height - 4);
		}

		public string GetHoverText() => Empty ? "0/0 (0%)" : $"{Value:D5}/{MaxValue} ({(1f - AttackPunishment) * 100:N1}%)";

		public float UseTimeMultiplier()
			=> WorldEvents.desoMode && !Empty
				? (ApplyExhaustionDebuffs
					? AttackSpeedDebuffMultiplier * DefaultAttackSpeedDebuff
					: (Active
						? AttackSpeedBuffMultiplier * DefaultAttackSpeedBuff
						: 1f))
				: 1f;

		/// <summary>
		/// Called in ModPlayer.PostUpdateRunSpeeds()
		/// </summary>
		public void RunSpeedChange() {
			if (!WorldEvents.desoMode || Empty)
				return;

			Parent.runAcceleration *= ApplyExhaustionDebuffs ? MoveSpeedDebuffMultiplier * DefaultMoveSpeedDebuff : (Active ? MoveSpeedBuffMultiplier * DefaultMoveSpeedBuff : 1f);

			float runSpeedMult = ApplyExhaustionDebuffs ? MaxMoveSpeedDebuffMultiplier * DefaultMaxMoveSpeedDebuff : (Active ? MaxMoveSpeedBuffMultiplier * DefaultMaxMoveSpeedBuff : 1f);
			Parent.maxRunSpeed *= runSpeedMult;
			Parent.accRunSpeed *= runSpeedMult;

			//If the player is exhausted, cap the horizontal speed
			if (ApplyExhaustionDebuffs)
				Parent.velocity.X.Clamp(-9f, 9f);
		}

		/// <summary>
		/// Called in ModPlayer.PreUpdate()
		/// </summary>
		public void FallSpeedDebuff() {
			if (!WorldEvents.desoMode || Empty)
				return;

			Parent.gravity *= ApplyExhaustionDebuffs ? 1.3f : 1f;
			Parent.maxFallSpeed *= ApplyExhaustionDebuffs ? 1.15f : 1f;
		}

		/// <summary>
		/// Resets this Stamina's rates to default.  Should only be called in a ResetEffects() hook.
		/// </summary>
		public void Reset() {
			if (Empty)
				return;

			IncreaseRate = DefaultIncreaseRate;
			ExhaustionIncreaseRate = DefaultExhaustionIncreaseRate;
			DecreaseRate = DefaultDecreaseRate;
			maxValue = DefaultMaxValue;

			Multipliers = DefaultMults;
			Adders = DefaultAdds;
		}

		/// <summary>
		/// Resets this Stamina's "value" to "maxValue" ONLY if its Parent is dead.  Use in ModPlayer.UpdateDead()
		/// </summary>
		public void ResetValue() {
			if (Empty)
				return;

			if (Parent.dead)
				value = maxValue;
		}

		/// <summary>
		/// Call this method to set changes to this Stamina's rates.
		/// </summary>
		/// <param name="incAdd">The direct increase to IncreaseRate.</param>
		/// <param name="exIncAdd">The direct increase to ExhaustionIncreaseRate.</param>
		/// <param name="decAdd">The direct increase to DecreaseRate.</param>
		/// <param name="incMult">A scalar change to IncreaseRate.  Applied before <paramref name="incAdd"/>.</param>
		/// <param name="exIncMult">A scalar change to ExhaustionIncreaseRate.  Applied before <paramref name="exIncAdd"/>.</param>
		/// <param name="decMult">A scalar change to DecreaseRate.  Applied before <paramref name="decAdd"/>.</param>
		/// <param name="maxAdd">The direct increase to MaxValue.</param>
		/// <param name="maxMult">A scalar change to MaxValue.  Applied before <paramref name="maxAdd"/>.</param>
		public void AddEffects(float incAdd = 0f, float exIncAdd = 0f, float decAdd = 0f, float incMult = 0f, float exIncMult = 0f, float decMult = 0f, int maxAdd = 0, float maxMult = 0f) {
			if (Empty)
				return;

			Multipliers[0] += incMult;
			Adders[0] += incAdd;
			Multipliers[1] += exIncMult;
			Adders[1] += exIncAdd;
			Multipliers[2] += decMult;
			Adders[2] += decAdd;
			Multipliers[3] += maxMult;
			Adders[3] += maxAdd / 10000f;
		}

		public void ApplyEffects() {
			if (Empty)
				return;

			//Apply the boss buffs, if any should be applied
			BossLogPlayer mp = Parent.GetModPlayer<BossLogPlayer>();
			foreach (var thing in StaminaBuffsTrackingNPC.BuffActions) {
				int id = thing.Key;
				if (id < NPCID.Count && mp.BossesKilled.Any(sbd => int.TryParse(sbd.key, out int i) && i == id))
					thing.Value(this);
				else if (id >= NPCID.Count) {
					var mn = ModContent.GetModNPC(id);
					if (mn != null && mp.BossesKilled.Any(sbd => sbd.mod == mn.Mod.Name && sbd.key == mn.Name))
						thing.Value(this);
				}
			}

			IncreaseRate = IncreaseRate * Multipliers[0] + Adders[0];
			ExhaustionIncreaseRate = ExhaustionIncreaseRate * Multipliers[1] + Adders[1];
			DecreaseRate = DecreaseRate * Multipliers[2] + Adders[2];
			maxValue = maxValue * Multipliers[3] + Adders[3];

			//Apply the punishment
			IncreaseRate *= 1f - AttackPunishment;
			ExhaustionIncreaseRate *= 1f - AttackPunishment;
		}

		public float GetIncreaseRate() => Empty ? 0f : (Exhaustion ? ExhaustionIncreaseRate : IncreaseRate);

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

		public void Add(uint amount, bool doScaleWithMax = false) {
			queuedAdds.Add((amount, doScaleWithMax));
		}

		private void HandleAdds() {
			foreach (var tuple in queuedAdds) {
				float calculated = tuple.Item1 / (DefaultMaxValue * 10000);
				//Scale it with the max
				if (tuple.Item2)
					calculated *= maxValue;

				value += calculated;

				if (value > maxValue)
					value = maxValue;
			}

			queuedAdds.Clear();
		}
	}
}
