using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace CosmivengeonMod.UI{
	public class StaminaUI : UIState{
		public StaminaBackUI staminaBackUI;
		public StaminaBarUI staminaBarUI;
		public static bool Visible;

		public override void OnInitialize(){
			staminaBackUI = new StaminaBackUI();
			staminaBarUI = new StaminaBarUI();

			Append(staminaBackUI);
			Append(staminaBarUI);
		}

		private const int ExhaustionAnimation1 = 15;
		private const int ExhaustionAnimation2 = ExhaustionAnimation1 + 30;
		private const int ExhaustionAnimation3 = ExhaustionAnimation2 + 15;
		private const int ExhaustionAnimation4 = ExhaustionAnimation3 + 30;
		private const int ExhaustionAnimation5 = ExhaustionAnimation4 + 15;
		private const int ExhaustionAnimation6 = ExhaustionAnimation5 + 30;
		private const int ExhaustionAnimation7 = ExhaustionAnimation6 + 15;
		private const int ExhaustionAnimation8 = ExhaustionAnimation7 + 20;
		private const int ExhaustionAnimation9 = ExhaustionAnimation8 + 20;
		private const int ExhaustionAnimation10 = ExhaustionAnimation9 + 20;
		private const int ExhaustionAnimation11 = ExhaustionAnimation10 + 20;

		protected override void DrawSelf(SpriteBatch spriteBatch){
			Player player = Main.LocalPlayer;
			StaminaPlayer modPlayer = player.GetModPlayer<StaminaPlayer>();
			Vector2 positionOffset = new Vector2(-10, -180);
			float scale = 2f;
			Texture2D animationTexture = ModContent.GetTexture("CosmivengeonMod/Abilities/ExhaustionAnimation");
			Vector2 animationCenter = animationTexture.Frame(1, 10).Size() / 2f;

			if(modPlayer.stamina.Exhaustion){
				Vector2 shakeOffset = Vector2.Zero;
				int frame = 0;
				float colorMult = 1f;
				
				if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation1)
					frame = 0;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation2){
					modPlayer.shakeTimer++;
					frame = 1;

					if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation2 - 10)
						shakeOffset = new Vector2(5f * MiscUtils.fSin(modPlayer.shakeTimer / 5f * MathHelper.TwoPi), 0f);
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation3){
					modPlayer.shakeTimer = 0;
					frame = 1;
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation4){
					modPlayer.shakeTimer++;
					frame = 2;

					if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation4 - 10)
						shakeOffset = new Vector2(5f * MiscUtils.fSin(modPlayer.shakeTimer / 5f * MathHelper.TwoPi), 0f);
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation5){
					modPlayer.shakeTimer = 0;
					frame = 2;
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation6){
					modPlayer.shakeTimer++;
					frame = 3;

					if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation6 - 10)
						shakeOffset = new Vector2(5f * MiscUtils.fSin(modPlayer.shakeTimer / 5f * MathHelper.TwoPi), 0f);
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation7)
					frame = 3;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation8)
					frame = 4;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation9)
					frame = 5;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation10)
					frame = 6;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation11)
					frame = 7;
				else
					colorMult = 0f;

				spriteBatch.Draw(animationTexture,
					modPlayer.Player.position + positionOffset - Main.screenPosition + shakeOffset + animationCenter,
					animationTexture.Frame(1, 10, 0, frame),
					Color.White * colorMult,
					0f,
					animationCenter,
					scale,
					SpriteEffects.None,
					0);

				modPlayer.exhaustionAnimationTimer++;
			}else{
				modPlayer.exhaustionAnimationTimer = 0;

				if(modPlayer.stamina.UnderThreshold){
					//Player is about to run out of stamina
					int frame = 8 + modPlayer.stamina.GetFlashType();

					spriteBatch.Draw(animationTexture,
						modPlayer.Player.position + positionOffset - Main.screenPosition + animationCenter,
						animationTexture.Frame(1, 10, 0, frame),
						Color.White,
						0f,
						animationCenter,
						scale,
						SpriteEffects.None,
						0);
				}
			}
		}
	}
}
