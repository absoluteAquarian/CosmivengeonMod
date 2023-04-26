using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace CosmivengeonMod.UI {
	public class StaminaUI : UIState {
		public StaminaBackground staminaBackground;

		public override void OnInitialize() {
			staminaBackground = new StaminaBackground();
			staminaBackground.Left.Set(-650, 1f);
			staminaBackground.Top.Set(30, 0f);
			Append(staminaBackground);
		}
	}
}
