using Terraria.ModLoader;

namespace CosmivengeonMod.DamageClasses.Desolate{
	public class DesolatorDamageClass : ModPlayer{
		public float damageAdd;
		public float damageMult;
		public float knockback;
		public int crit;

		public override void ResetEffects(){
			Reset();
		}

		public override void UpdateDead(){
			Reset();
		}

		private void Reset(){
			damageAdd = 0;
			damageMult = 0;
			knockback = 0;
			crit = 0;
		}
	}
}
