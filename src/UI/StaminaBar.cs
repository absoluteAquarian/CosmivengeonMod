using CosmivengeonMod.Abilities;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace CosmivengeonMod.UI {
	public class StaminaBar : UIElement {
		public StaminaBar() {
			Width.Set(90f, 0f);
			Height.Set(26f, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Stamina stamina = Main.LocalPlayer.GetModPlayer<StaminaPlayer>().stamina;

			var asset = ModContent.Request<Texture2D>("CosmivengeonMod/Abilities/Bar", AssetRequestMode.ImmediateLoad);

			Rectangle rect = GetDimensions().ToRectangle();
			Rectangle src = asset.Value.Bounds;

			rect.Width = src.Width = (int)(src.Width * (float)stamina.Value / stamina.MaxValue);

			spriteBatch.Draw(asset.Value, rect, src, Color.White);
		}
	}
}
