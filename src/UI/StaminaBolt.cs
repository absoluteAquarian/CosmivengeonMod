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
	internal class StaminaBolt : UIElement {
		public StaminaBolt() {
			Width.Set(22, 0f);
			Height.Set(28, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Stamina stamina = Main.LocalPlayer.GetModPlayer<StaminaPlayer>().stamina;

			var asset = ModContent.Request<Texture2D>($"CosmivengeonMod/Abilities/BarFrameEnergyIcon", AssetRequestMode.ImmediateLoad);
			var position = GetDimensions().Center();
			var frame = asset.Value.Frame(1, 4, 0, stamina.GetIconFrame(), 0, -2);
			var center = asset.Value.Size() / 2f;

			float scale = 1f;
			if (stamina.UnderThreshold && !stamina.Exhaustion)
				scale = 1f + 0.75f * (1f - (float)stamina.Value / stamina.MaxValue / Stamina.FlashThreshold);

			spriteBatch.Draw(asset.Value, position, frame, Color.White, 0f, center, scale, SpriteEffects.None, 0);

			if (stamina.BumpTimer > 0) {
				stamina.BumpTimer--;

				float sin = (float)Math.Sin((1f - stamina.BumpTimer / 20f) * MathHelper.Pi);

				spriteBatch.Draw(asset.Value, position, frame, Color.White * 0.65f, 0f, center, 1f + 0.75f * sin, SpriteEffects.None, 0);
			}
		}
	}
}
