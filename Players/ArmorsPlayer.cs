using CosmivengeonMod.Buffs.Harmful;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players{
	public class ArmorsPlayer : ModPlayer{
		public bool setBonus_Rockserpent;
		public bool setBonus_Crystalice;

		public override void ResetEffects(){
			setBonus_Rockserpent = false;
			setBonus_Crystalice = false;
		}

		public override void UpdateDead(){
			setBonus_Rockserpent = false;
			setBonus_Crystalice = false;
		}

		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
			=> OnHitNPCWithAnything(target);

		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
			=> OnHitNPCWithAnything(target);

		private void OnHitNPCWithAnything(NPC target){
			if(setBonus_Rockserpent && Main.rand.NextFloat() < 0.02f)
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 2 * 60);
			if(setBonus_Crystalice)
				target.AddBuff(BuffID.Frostburn, 6 * 60);
		}
	}
}
