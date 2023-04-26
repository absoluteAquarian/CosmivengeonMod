using CosmivengeonMod.Abilities;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.UI {
	public class StaminaBar : StaminaBackground {
		public StaminaBar() {
			Width.Set(88f, 0f);
			Height.Set(22f, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Stamina stamina = Main.LocalPlayer.GetModPlayer<StaminaPlayer>().stamina;

			Rectangle rect = GetDimensions().ToRectangle();

			rect.Width = (int)(rect.Width * (float)stamina.Value / stamina.MaxValue);

			var asset = ModContent.Request<Texture2D>("CosmivengeonMod/Abilities/Bar", AssetRequestMode.ImmediateLoad);

			spriteBatch.Draw(asset.Value, rect, Color.White);
		}
	}
}
