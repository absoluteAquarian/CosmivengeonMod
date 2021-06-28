using CosmivengeonMod.Buffs.Mechanics;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players{
	public class AccessoriesPlayer : ModPlayer{
		public ExtraJump oronitusJump;

		public bool snowCoat;
		public bool frostHorn;
		public bool brokenFrostHorn;
		public bool blizzardEye;
		public bool activeBlizzardEye;

		public override void ResetEffects(){
			snowCoat = false;
			frostHorn = false;
			brokenFrostHorn = false;
			blizzardEye = false;
			activeBlizzardEye = false;

			oronitusJump.abilityActive = false;
		}

		public override void UpdateDead(){
			oronitusJump.abilityActive = false;
			brokenFrostHorn = false;
		}

		public override void PostUpdateEquips(){
			if(!blizzardEye){
				for(int i = 0; i < Main.maxProjectiles; i++){
					Projectile projectile = Main.projectile[i];

					if(projectile.active && projectile.modProjectile is EyeOfTheBlizzardCrystal && projectile.owner == player.whoAmI)
						projectile.active = false;
				}
			}
		}

		public override void UpdateLifeRegen(){
			//No healing if the cooldown is active
			if(blizzardEye && !player.HasBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>())){
				if(Main.hardMode)
					player.lifeRegen += (activeBlizzardEye ? 8 : 4) * 2;
				else
					player.lifeRegen += (activeBlizzardEye ? 4 : 2) * 2;
			}
		}

		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
			=> OnHitNPCWithAnything(target);

		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
			=> OnHitNPCWithAnything(target);

		private void OnHitNPCWithAnything(NPC target){
			if(snowCoat && Main.rand.NextFloat() < 0.075f)
				target.AddBuff(BuffID.Frostburn, 5 * 60);
		}

		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit){
			if(frostHorn && !brokenFrostHorn && damage >= player.statLifeMax2 * 0.25f){
				player.AddBuff(ModContent.BuffType<FrostHornBroken>(), 10 * 60);

				Main.PlaySound(SoundID.Item27, player.Top);
				for(int i = 0; i < 60; i++){
					Dust dust = Dust.NewDustDirect(player.Top - new Vector2(8, 8), 16, 16, 74, Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
					Dust dust2 = Dust.NewDustDirect(player.Top - new Vector2(8, 8), 16, 16, 107, Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
					dust.noGravity = true;
					dust2.noGravity = true;
				}
			}
		}

		public override float UseTimeMultiplier(Item item) => activeBlizzardEye ? 1.05f : 1f;
	}
}
