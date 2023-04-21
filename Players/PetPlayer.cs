using Terraria.ModLoader;

namespace CosmivengeonMod.Players {
	public class PetPlayer : ModPlayer {
		public bool cloudPet;

		public override void ResetEffects() {
			cloudPet = false;
		}

		public override void UpdateDead() {
			cloudPet = false;
		}
	}
}
