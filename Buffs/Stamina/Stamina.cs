using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Stamina{
	/// <summary>
	/// The custom buff thing for Cosmivengeon.
	/// </summary>
	public class Stamina {
		public Player Parent{ get; private set; }

		private float value;
		public int Value => (int)(value * 10000f);
		public bool Active;
		private bool oldActive;
		public bool Exhaustion{ get; private set; }
		public bool ApplyExhaustionDebuffs => Exhaustion && ExhaustionTimer > 0 && !ExhaustionEffectsWornOff;
		private bool ExhaustionEffectsWornOff;
		public float IncreaseRate{ get; private set; }
		public float ExhaustionIncreaseRate{ get; private set; }
		public float DecreaseRate{ get; private set; }

		private const float DefaultIncreaseRate = 1f / (4f * 60f);
		private const float DefaultExhaustionIncreaseRate = 1f / (8f * 60f);
		private const float DefaultDecreaseRate = 1f / (5f * 60f);

		private const int ExhaustionTimerMax = 60;
		private int ExhaustionTimer;
		private const int RegenDelay = 45;
		private int RegenDelayTimer;
		private bool StartRegen;

		private int FlashTimer;
		private const float FlashThreshold = 0.2f;
		private const int FlashCycle = 20;
		private int FlashDelay{
			get{
				float curRatio = value / FlashThreshold;
				curRatio = Utils.Clamp(curRatio * curRatio * curRatio, 0.3333f, 0.6667f);
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

		public Stamina(Player player){
			Parent = player;

			value = 1f;
			Active = false;
			oldActive = false;
			Exhaustion = false;
			ExhaustionTimer = 0;
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

		public void Update(){
			if(!CosmivengeonWorld.desoMode)
				return;

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

			//Cap value at 1f and deactivate Exhaustion if active
			if((!Active || Exhaustion) && value + GetIncreaseRate() > 1f){
				Exhaustion = false;
				StartRegen = false;
				ExhaustionEffectsWornOff = false;
				value = 1f;
			}

			//Check which decrease to use, increase otherwise
			if(Active && !Exhaustion)
				value -= DecreaseRate;
			else if(!Active && StartRegen)
				value += GetIncreaseRate();

			if(!StartRegen && !Active && value < 1f)
				RegenDelayTimer--;
			if(RegenDelayTimer == 0 && !StartRegen)
				StartRegen = true;
			FlashTimer++;

			if(!ExhaustionEffectsWornOff && Exhaustion && ExhaustionTimer == 0){
				ExhaustionTimer = ExhaustionTimerMax;
				if(value > 0.1f)
					ExhaustionEffectsWornOff = true;
			}

			if(ExhaustionTimer > 0 && Exhaustion)
				ExhaustionTimer--;

			oldActive = Active;

			//Apply a visual buff
			if(Active)
				Parent.AddBuff(ModContent.BuffType<EnergizedBuff>(), 2);
			else if(ApplyExhaustionDebuffs)
				Parent.AddBuff(ModContent.BuffType<ExhaustedDebuff>(), 2);
		}

		public Texture2D GetBackTexture(){
			if(Exhaustion)
				return BackGray;
			else if(value > FlashThreshold)
				return BackGreen;
			else if(FlashTimer % FlashDelay < FlashDelay / 2f)
				return BackYellow;
			return BackRed;
		}

		public Texture2D GetBarTexture(){
			if(Exhaustion)
				return BarGray;
			else if(value > FlashThreshold)
				return BarGreen;
			else if(FlashTimer % FlashDelay < FlashDelay / 2f)
				return BarYellow;
			return BarRed;
		}

		public Rectangle GetBarRect() => new Rectangle(0, 0, (int)(GetBarTexture().Width * value), GetBarTexture().Height);

		public string GetHoverText() => $"{Value:D5}/10000";

		public float UseTimeMultiplier() => CosmivengeonWorld.desoMode ? (ApplyExhaustionDebuffs ? 0.6667f : (Active ? 1.35f : 1f)) : 1f;

		/// <summary>
		/// Called in ModPlayer.PostUpdateRunSpeeds()
		/// </summary>
		public void RunSpeedChange(){
			if(!CosmivengeonWorld.desoMode)
				return;

			Parent.moveSpeed *= ApplyExhaustionDebuffs ? 0.84f : (Active ? 1.33f : 1f);

			float runSpeedMult = ApplyExhaustionDebuffs ? 0.73f : (Active ? 1.2f : 1f);
			Parent.maxRunSpeed *= runSpeedMult;
			Parent.accRunSpeed *= runSpeedMult;
		}

		/// <summary>
		/// Called in ModPlayer.PreUpdate()
		/// </summary>
		public void FallSpeedDebuff(){
			if(!CosmivengeonWorld.desoMode)
				return;

			Parent.gravity *= ApplyExhaustionDebuffs ? 1.3f : 1f;
			Parent.maxFallSpeed *= 1.15f;
		}

		/// <summary>
		/// Resets this Stamina's rates to default.  Should only be called in a ResetEffects() hook.
		/// </summary>
		public void Reset(){
			IncreaseRate = DefaultIncreaseRate;
			ExhaustionIncreaseRate = DefaultExhaustionIncreaseRate;
			DecreaseRate = DefaultDecreaseRate;
		}

		/// <summary>
		/// Resets this Stamina's "value" to 1 ONLY if its Parent is dead.  Use in ModPlayer.UpdateDead()
		/// </summary>
		public void ResetValue(){
			if(Parent.dead)
				value = 1f;
		}

		/// <summary>
		/// Call this method to set changes to this Stamina's rates.
		/// </summary>
		/// <param name="incAdd">The direct increase to IncreaseRate.</param>
		/// <param name="exIncAdd">The direct increase to ExhaustionIncreaseRate.</param>
		/// <param name="decAdd">The direct increase to DecreaseRate</param>
		/// <param name="incMult">A scalar change to IncreaseRate.  Applied before <paramref name="incAdd"/>.</param>
		/// <param name="exIncMult">A scalar change to ExhaustionIncreaseRate.  Applied before <paramref name="exIncAdd"/>.</param>
		/// <param name="decMult">A scalar change to DecreaseRate.  Applied before <paramref name="decAdd"/>.</param>
		public void ApplyEffects(float incAdd = 0f, float exIncAdd = 0f, float decAdd = 0f, float incMult = 1f, float exIncMult = 1f, float decMult = 1f){
			IncreaseRate *= incMult;
			IncreaseRate += incAdd;
			ExhaustionIncreaseRate *= exIncMult;
			ExhaustionIncreaseRate += exIncAdd;
			DecreaseRate *= decMult;
			DecreaseRate += decAdd;
		}

		public float GetIncreaseRate() => Exhaustion ? ExhaustionIncreaseRate : IncreaseRate;
	}
}
