using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Buffs.Stamina{
	/// <summary>
	/// The custom buff thing for Cosmivengeon.
	/// </summary>
	public class Stamina{
		public static Color EnergizedColor => new Color(0x4c, 0xff, 0x00);
		public Player Parent{ get; private set; }

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
		public float IncreaseRate{ get; private set; }
		/// <summary>
		/// How quickly the stamina bar fills when the player is or was in the Exhausted state.
		/// Defaults to 1/8th the maximum Value per second (1250 units)
		/// </summary>
		public float ExhaustionIncreaseRate{ get; private set; }
		/// <summary>
		/// How quickly the stamina bar depletes when the player is in the Active state.
		/// Defaults to 1/5th the maximum Value per second (2000 units)
		/// </summary>
		public float DecreaseRate{ get; private set; }

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

		private static float[] DefaultMults => CosmivengeonUtils.CreateArray(1f, 10);
		private static float[] DefaultAdds => CosmivengeonUtils.CreateArray(0f, 4);

		private const int RegenDelay = 45;
		private int RegenDelayTimer;
		private bool StartRegen;

		private int FlashTimer;
		private const float FlashThreshold = 0.2f;
		private const int FlashCycle = 20;
		private int FlashDelay{
			get{
				float curRatio = value / FlashThreshold;
				curRatio = curRatio * curRatio * curRatio;
				curRatio.Clamp(0.3333f, 0.6667f);
				return (int)(FlashCycle * curRatio);
			}
		}

		public Texture2D BackGreen{ get; private set; }
		public Texture2D BackYellow{ get; private set; }
		public Texture2D BackRed{ get; private set; }
		public Texture2D BackGray{ get; private set; }
		public Texture2D BarGreen{ get; private set; }
		public Texture2D BarYellow{ get; private set; }
		public Texture2D BarRed{ get; private set; }
		public Texture2D BarGray{ get; private set; }

		public static Vector2 BackDrawPos => new Vector2(Main.screenWidth - 650, 30);
		public static Vector2 BarDrawPos => BackDrawPos + new Vector2(58, 14);

		public readonly bool Empty = false;

		public Stamina(Player player = null){
			if(player is null){
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
			StartRegen = false;
			IncreaseRate = DefaultIncreaseRate;
			ExhaustionIncreaseRate = DefaultExhaustionIncreaseRate;
			DecreaseRate = DefaultDecreaseRate;

			BackGreen = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBack_Green");
			BackYellow = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBack_Yellow");
			BackRed = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBack_Red");
			BackGray = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBack_Gray");
			BarGreen = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBar_Green");
			BarYellow = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBar_Yellow");
			BarRed = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBar_Red");
			BarGray = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/UIBar_Gray");
		}

		public void Clone(Stamina other){
			if(Empty)
				return;

			value = other.value;
			Active = other.Active;
			oldActive = other.oldActive;
			Exhaustion = other.Exhaustion;
			ExhaustionEffectsWornOff = other.ExhaustionEffectsWornOff;
			maxValue = other.maxValue;
			NoDecay = other.NoDecay;
			StartRegen = other.StartRegen;
			RegenDelayTimer = other.RegenDelayTimer;
			Parent = other.Parent;
			Multipliers = other.Multipliers;
			Adders = other.Adders;
		}

		public void SendData(ModPacket packet){
			if(Empty)
				return;

			BitsByte flag = new BitsByte(Active, oldActive, Exhaustion, ExhaustionEffectsWornOff, NoDecay, StartRegen);

			packet.Write(value);
			packet.Write(maxValue);
			packet.Write(RegenDelayTimer);
			packet.Write(flag);
			packet.Write((byte)Parent.whoAmI);
		}

		/// <summary>
		/// Assumes that this is being called within a syncing hook.
		/// </summary>
		public void ReceiveData(BinaryReader reader){
			value = reader.ReadSingle();
			maxValue = reader.ReadSingle();
			RegenDelayTimer = reader.ReadInt32();

			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref Active, ref oldActive, ref Exhaustion, ref ExhaustionEffectsWornOff, ref NoDecay, ref StartRegen);

			Parent = Main.player[reader.ReadByte()];
		}

		public TagCompound GetTagCompound() => new TagCompound(){
			["value"] = value,
			["maxValue"] = maxValue,
			["multipliers"] = Multipliers.ToList(),
			["adders"] = Adders.ToList()
		};

		public void ParseCompound(TagCompound tag){
			value = tag.GetFloat("value");
			maxValue = tag.GetFloat("maxValue");
			//These lists in the tag can be 'null' if this is a new character or one that doesn't have any Stamina data yet
			Multipliers = tag.GetList<float>("multipliers")?.ToArray() ?? DefaultMults;
			Adders = tag.GetList<float>("adders")?.ToArray() ?? DefaultAdds;
		}

		public void Update(){
			if(Empty)
				return;

			if(!CosmivengeonWorld.desoMode){
				if(value < 0.175f * maxValue)
					value = 0.175f * maxValue;
				return;
			}

			ApplyEffects();

			if(NoDecay){
				value = maxValue;
				Exhaustion = false;
				ExhaustionEffectsWornOff = true;
				StartRegen = false;
				goto End;
			}

			//If enabled and it's just been depleted, activate Exhaustion
			if(!Exhaustion && Active && value - DecreaseRate < 0f){
				Active = false;
				Exhaustion = true;
				value = 0f;
			}

			if(oldActive != Active) {
				RegenDelayTimer = RegenDelay;
				StartRegen = false;
			}

			//Cap value at maxValue and deactivate Exhaustion if active
			if((!Active || Exhaustion) && value + GetIncreaseRate() > maxValue){
				Exhaustion = false;
				StartRegen = false;
				ExhaustionEffectsWornOff = false;
				value = maxValue;
			}

			//Check which decrease to use, increase otherwise
			if(Active && !Exhaustion)
				value -= DecreaseRate;
			else if(!Active && StartRegen)
				value += GetIncreaseRate();

			if(!StartRegen && !Active && value < maxValue)
				RegenDelayTimer--;
			if(RegenDelayTimer == 0 && !StartRegen)
				StartRegen = true;
			FlashTimer++;

			if(ApplyExhaustionDebuffs && value > 0.175f * maxValue)
				ExhaustionEffectsWornOff = true;

End:
			oldActive = Active;

			//Apply a visual buff
			if(Active)
				Parent.AddBuff(ModContent.BuffType<EnergizedBuff>(), 2);
			else if(ApplyExhaustionDebuffs)
				Parent.AddBuff(ModContent.BuffType<ExhaustedDebuff>(), 2);

			if(value > maxValue)
				value = maxValue;
		}

		public Texture2D GetBackTexture(){
			if(Empty)
				return Main.magicPixel;

			if(Exhaustion)
				return BackGray;
			else if(value > FlashThreshold)
				return BackGreen;
			else if(FlashTimer % FlashDelay < FlashDelay / 2f)
				return BackYellow;
			return BackRed;
		}

		public Texture2D GetBarTexture(){
			if(Empty)
				return Main.magicPixel;

			if(Exhaustion)
				return BarGray;
			else if(value > FlashThreshold)
				return BarGreen;
			else if(FlashTimer % FlashDelay < FlashDelay / 2f)
				return BarYellow;
			return BarRed;
		}

		public Rectangle GetBarRect() => Empty ? Rectangle.Empty : new Rectangle(0, 0, (int)(GetBarTexture().Width * (value / maxValue)), GetBarTexture().Height);

		public string GetHoverText() => Empty ? "0/0" : $"{Value:D5}/{MaxValue}";

		public float UseTimeMultiplier()
			=> CosmivengeonWorld.desoMode && !Empty ? (ApplyExhaustionDebuffs ? AttackSpeedDebuffMultiplier * DefaultAttackSpeedDebuff : (Active ? AttackSpeedBuffMultiplier * DefaultAttackSpeedBuff : 1f)) : 1f;

		/// <summary>
		/// Called in ModPlayer.PostUpdateRunSpeeds()
		/// </summary>
		public void RunSpeedChange(){
			if(!CosmivengeonWorld.desoMode || Empty)
				return;

			Parent.moveSpeed *= ApplyExhaustionDebuffs ? MoveSpeedDebuffMultiplier * DefaultMoveSpeedDebuff : (Active ? MoveSpeedBuffMultiplier * DefaultMoveSpeedBuff : 1f);

			float runSpeedMult = ApplyExhaustionDebuffs ? MaxMoveSpeedDebuffMultiplier * DefaultMaxMoveSpeedDebuff : (Active ? MaxMoveSpeedBuffMultiplier * DefaultMaxMoveSpeedBuff : 1f);
			Parent.maxRunSpeed *= runSpeedMult;
			Parent.accRunSpeed *= runSpeedMult;
		}

		/// <summary>
		/// Called in ModPlayer.PreUpdate()
		/// </summary>
		public void FallSpeedDebuff(){
			if(!CosmivengeonWorld.desoMode || Empty)
				return;

			Parent.gravity *= ApplyExhaustionDebuffs ? 1.3f : 1f;
			Parent.maxFallSpeed *= ApplyExhaustionDebuffs ? 1.15f : 1f;
		}

		/// <summary>
		/// Resets this Stamina's rates to default.  Should only be called in a ResetEffects() hook.
		/// </summary>
		public void Reset(){
			if(Empty)
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
		public void ResetValue(){
			if(Empty)
				return;

			if(Parent.dead)
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
		public void AddEffects(float incAdd = 0f, float exIncAdd = 0f, float decAdd = 0f, float incMult = 0f, float exIncMult = 0f, float decMult = 0f, int maxAdd = 0, float maxMult = 0f){
			if(Empty)
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

		public void ApplyEffects(){
			if(Empty)
				return;

			//Apply the boss buffs, if any should be applied
			CosmivengeonPlayer mp = Parent.GetModPlayer<CosmivengeonPlayer>();
			foreach(var thing in StaminaBuffsGlobalNPC.BuffActions)
				if(mp.BossesKilled.Contains(thing.Key))
					thing.Value(this);

			IncreaseRate = IncreaseRate * Multipliers[0] + Adders[0];
			ExhaustionIncreaseRate = ExhaustionIncreaseRate * Multipliers[1] + Adders[1];
			DecreaseRate = DecreaseRate * Multipliers[2] + Adders[2];
			maxValue = maxValue * Multipliers[3] + Adders[3];
		}

		public float GetIncreaseRate() => Empty ? 0f : (Exhaustion ? ExhaustionIncreaseRate : IncreaseRate);
	}
}
