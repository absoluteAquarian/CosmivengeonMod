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
