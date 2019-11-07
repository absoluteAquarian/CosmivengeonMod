using Microsoft.Xna.Framework.Graphics;
using Terraria;
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
	}
}