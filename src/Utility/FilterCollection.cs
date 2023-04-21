using Terraria.Graphics.Effects;

namespace CosmivengeonMod.Utility {
	public static class FilterCollection {
		public static readonly string Name_Screen_EoC = "EoC_ScreenDarken";
		public static Filter Screen_EoC {
			get => Filters.Scene[Name_Screen_EoC];
			set => Filters.Scene[Name_Screen_EoC] = value;
		}
	}
}
