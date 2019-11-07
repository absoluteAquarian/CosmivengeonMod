using CosmivengeonMod.Buffs.Stamina;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
			CosmivengeonPlayer modPlayer = Main.LocalPlayer.GetModPlayer<CosmivengeonPlayer>();
			Texture2D texture = IsBar ? modPlayer.stamina.GetBarTexture() : modPlayer.stamina.GetBackTexture();
			Vector2 drawPos = IsBar ? Stamina.BarDrawPos : Stamina.BackDrawPos;
			Rectangle? drawRect = IsBar ? (Rectangle?)modPlayer.stamina.GetBarRect() : null;

			spriteBatch.Draw(texture, drawPos, drawRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);

			if(ContainsPoint(Main.MouseScreen))
				Main.hoverItemName = Main.LocalPlayer.GetModPlayer<CosmivengeonPlayer>().stamina.GetHoverText();
		}
	}
}