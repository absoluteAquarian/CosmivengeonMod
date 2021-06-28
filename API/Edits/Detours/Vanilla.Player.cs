using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Terraria;

namespace CosmivengeonMod.API.Edits.Detours{
	public static partial class Vanilla{
		public static void Player_DoubleJumpVisuals(On.Terraria.Player.orig_DoubleJumpVisuals orig, Player self){
			orig(self);

			AccessoriesPlayer mp = self.GetModPlayer<AccessoriesPlayer>();
			if(mp.oronitusJump.jumpEffectActive && mp.oronitusJump.abilityActive && !mp.oronitusJump.jumpAgain && ((self.gravDir == 1f && self.velocity.Y < 0f) || (self.gravDir == -1f && self.velocity.Y > 0f))){
				if(self.height >= 32){
					for(int i = 0; i < 7; i++){
						Dust dust = Dust.NewDustDirect(self.position + new Vector2(0, 16), self.width, self.height - 16, 61, Scale: 2);
						dust.fadeIn = 1f;
						dust.noGravity = Main.rand.NextFloat() <= 0.6667f;
					}
				}
			}
		}
	}
}
