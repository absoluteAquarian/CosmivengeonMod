using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using SerousCommonLib.API;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits {
	internal class JewelDoubleJumpVisualsDetour : Edit {
		public override void LoadEdits() {
			// DoubleJumpVisuals is small enough that I can just detour it and add the custom jump's effects
			On.Terraria.Player.DoubleJumpVisuals += Player_DoubleJumpVisuals;
		}

		public override void UnloadEdits() {
			On.Terraria.Player.DoubleJumpVisuals -= Player_DoubleJumpVisuals;
		}

		public static void Player_DoubleJumpVisuals(On.Terraria.Player.orig_DoubleJumpVisuals orig, Player self) {
			orig(self);

			AccessoriesPlayer mp = self.GetModPlayer<AccessoriesPlayer>();
			if (mp.oronitusJump.jumpEffectActive && mp.oronitusJump.abilityActive && !mp.oronitusJump.jumpAgain && ((self.gravDir == 1f && self.velocity.Y < 0f) || (self.gravDir == -1f && self.velocity.Y > 0f))) {
				if (self.height >= 32) {
					for (int i = 0; i < 7; i++) {
						Dust dust = Dust.NewDustDirect(self.position + new Vector2(0, 16), self.width, self.height - 16, DustID.GreenTorch, Scale: 2);
						dust.fadeIn = 1f;
						dust.noGravity = Main.rand.NextFloat() <= 0.6667f;
					}
				}
			}
		}
	}
}
