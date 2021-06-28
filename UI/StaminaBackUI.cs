using CosmivengeonMod.Abilities;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.UI;

namespace CosmivengeonMod.UI{
	public class StaminaBackUI : UIElement{
		internal bool IsBar;
		public StaminaBackUI(){
			Left.Set(Stamina.BackDrawPos.X, 0f);
			Top.Set(Stamina.BackDrawPos.Y, 0f);
			Width.Set(160f, 0f);
			Height.Set(50f, 0f);
			IsBar = false;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch){
			StaminaPlayer modPlayer = Main.LocalPlayer.GetModPlayer<StaminaPlayer>();
			Stamina stamina = modPlayer.stamina;

			Texture2D texture = IsBar ? stamina.GetBarTexture() : stamina.GetBackTexture();
			Vector2 drawPos = IsBar ? Stamina.BarDrawPos : Stamina.BackDrawPos;
			Rectangle? sourceRect = IsBar ? (Rectangle?)stamina.GetBarRect() : null;

			Rectangle rect = new Rectangle((int)drawPos.X, (int)drawPos.Y, texture.Width, texture.Height);
			if(IsBar){
				rect.Width -= 4;
				rect.Height -= 4;

				rect.Width = (int)(rect.Width * (float)stamina.Value / stamina.MaxValue);
				sourceRect = new Rectangle(sourceRect.Value.X, sourceRect.Value.Y, rect.Width, rect.Height);
			}

			spriteBatch.Draw(texture, rect, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0);

			if(!IsBar){
				Texture2D energy = stamina.GetIconTexture();

				float scale = 1f;
				if(stamina.UnderThreshold && !stamina.Exhaustion)
					scale = 1f + 0.75f * (1f - (float)stamina.Value / stamina.MaxValue / Stamina.FlashThreshold);

				spriteBatch.Draw(energy, drawPos + new Vector2(18, 14) + energy.Size() / 2f, null, Color.White, 0f, energy.Size() / 2f, scale, SpriteEffects.None, 0);

				if(stamina.BumpTimer > 0){
					stamina.BumpTimer--;

					spriteBatch.Draw(energy, drawPos + new Vector2(18, 14) + energy.Size() / 2f, null, Color.White * 0.65f, 0f, energy.Size() / 2f, 1f + 0.75f * (float)Math.Sin((1f - stamina.BumpTimer / 20f) * MathHelper.Pi), SpriteEffects.None, 0);
				}
			}

			if(ContainsPoint(Main.MouseScreen))
				Main.hoverItemName = Main.LocalPlayer.GetModPlayer<StaminaPlayer>().stamina.GetHoverText();
		}
	}
}
