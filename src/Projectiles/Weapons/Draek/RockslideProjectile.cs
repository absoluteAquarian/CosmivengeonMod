﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek {
	//Some code copied from ExampleYoyoProjectile
	public class RockslideProjectile : ModProjectile {
		public override void SetStaticDefaults() {
			//The following sets are only applicable to yoyo that use aiStyle 99.
			//YoyosLifeTimeMultiplier is how long in seconds the yoyo will stay out before automatically returning to the player. 
			//Vanilla values range from 3f(Wood) to 16f(Chik), and defaults to -1f. Leaving as -1 will make the time infinite.
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 6f;

			//YoyosMaximumRange is the maximum distance the yoyo sleep away from the player. 
			//Vanilla values range from 130f(Wood) to 400f(Terrarian), and defaults to 200f
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 250f;

			//YoyosTopSpeed is top speed of the yoyo projectile. 
			//Vanilla values range from 9f(Wood) to 17.5f(Terrarian), and defaults to 10f
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 12f;
		}

		public override void SetDefaults() {
			Projectile.extraUpdates = 0;
			Projectile.width = 24;
			Projectile.height = 24;
			//aiStyle 99 is used for all yoyos, and is Extremely suggested, as yoyo are extremely difficult without them
			Projectile.aiStyle = 99;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.scale = 1f;
		}
		//notes for aiStyle 99: 
		//localAI[0] is used for timing up to YoyosLifeTimeMultiplier
		//localAI[1] can be used freely by specific types
		//ai[0] and ai[1] usually point towards the x and y world coordinate hover point
		//ai[0] is -1f once YoyosLifeTimeMultiplier is reached, when the player is stoned/frozen, when the yoyo is too far away, or the player is no longer clicking the shoot button.
		//ai[0] being negative makes the yoyo move back towards the player
		//Any AI method can be used for dust, spawning projectiles, etc specific to your yoyo.

		public override void PostAI() {
			//Add a green light from the projectile
			Lighting.AddLight(Projectile.Center, 0f, 1f, 0f);

			if (Main.rand.NextBool(8))
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenFairy);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (Main.rand.NextFloat() <= 0.125)
				target.AddBuff(BuffID.Poisoned, 10 * 60);
		}
	}
}
