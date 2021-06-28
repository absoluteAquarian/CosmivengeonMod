using CosmivengeonMod.Abilities;

namespace CosmivengeonMod.UI{
	public class StaminaBarUI : StaminaBackUI{
		public StaminaBarUI(){
			Left.Set(Stamina.BarDrawPos.X, 0f);
			Top.Set(Stamina.BarDrawPos.Y, 0f);
			Width.Set(88f, 0f);
			Height.Set(22f, 0f);
			IsBar = true;
		}
	}
}
