using Terraria.ModLoader;

namespace CosmivengeonMod.Players{
	public class MinionPlayer : ModPlayer{
		public bool babySnek;
		public bool babyProwler;

		public override void ResetEffects(){
			babySnek = false;
			babyProwler = false;
		}

		public override void UpdateDead(){
			babySnek = false;
			babyProwler = false;
		}
	}
}
