using CosmivengeonMod.Abilities;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace CosmivengeonMod.UI {
	public class StaminaBackground : UIElement {
		private StaminaBar bar;
		private StaminaBolt bolt;

		public StaminaBackground() {
			Width.Set(160f, 0f);
			Height.Set(50f, 0f);
		}

		public override void OnInitialize() {
			bolt = new StaminaBolt();
			bolt.Left.Set(18, 0f);
			bolt.Top.Set(14, 0f);

			Append(bolt);

			bar = new StaminaBar();
			bar.Left.Set(58, 0f);
			bar.Top.Set(14, 0f);

			Append(bar);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			StaminaPlayer modPlayer = Main.LocalPlayer.GetModPlayer<StaminaPlayer>();
			Stamina stamina = modPlayer.stamina;

			/*
			Texture2D texture = IsBar ? Stamina.GetBarTexture() : Stamina.GetBackTexture();
			Vector2 drawPos = IsBar ? Stamina.BarDrawPos : Stamina.BackDrawPos;
			Rectangle? sourceRect = IsBar ? (Rectangle?)stamina.GetBarRect() : null;

			Rectangle rect = new Rectangle((int)drawPos.X, (int)drawPos.Y, texture.Width, texture.Height);
			if (IsBar) {
				rect.Width -= 4;
				rect.Height -= 4;

				rect.Width = (int)(rect.Width * (float)stamina.Value / stamina.MaxValue);
				sourceRect = new Rectangle(sourceRect.Value.X, sourceRect.Value.Y, rect.Width, rect.Height);
			}

			spriteBatch.Draw(texture, rect, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0);

			if (!IsBar) {
				Texture2D energy = stamina.GetIconTexture();

				float scale = 1f;
				if (stamina.UnderThreshold && !stamina.Exhaustion)
					scale = 1f + 0.75f * (1f - (float)stamina.Value / stamina.MaxValue / Stamina.FlashThreshold);

				spriteBatch.Draw(energy, drawPos + new Vector2(18, 14) + energy.Size() / 2f, null, Color.White, 0f, energy.Size() / 2f, scale, SpriteEffects.None, 0);

				if (stamina.BumpTimer > 0) {
					stamina.BumpTimer--;

					spriteBatch.Draw(energy, drawPos + new Vector2(18, 14) + energy.Size() / 2f, null, Color.White * 0.65f, 0f, energy.Size() / 2f, 1f + 0.75f * (float)Math.Sin((1f - stamina.BumpTimer / 20f) * MathHelper.Pi), SpriteEffects.None, 0);
				}
			}
			*/

			var asset = ModContent.Request<Texture2D>("CosmivengeonMod/Abilities/BarFrame", AssetRequestMode.ImmediateLoad);

			spriteBatch.Draw(asset.Value, GetDimensions().Position(), null, Color.White);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (IsMouseHovering)
				Main.instance.MouseText(Main.LocalPlayer.GetModPlayer<StaminaPlayer>().stamina.GetHoverText());
		}
	}
}
