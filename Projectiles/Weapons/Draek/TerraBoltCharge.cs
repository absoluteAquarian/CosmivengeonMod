using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek{
	public class TerraBoltCharge : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Weapons/Draek/TerraBoltProjectile";

		public ref float Charge => ref projectile.ai[0];

		public ref float State => ref projectile.ai[1];

		public float MaxCharge => 60 / PlayerHooks.UseTimeMultiplier(Main.player[projectile.owner], Main.player[projectile.owner].HeldItem);

		private int wait;

		public override void SetDefaults(){
			projectile.width = 10;
			projectile.height = 10;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public const float SpawnDistance = 8;

		private int animTimer;

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			//Only draw the things during the charge animation
			if(State > 0)
				return false;

			Player owner = Main.player[projectile.owner];

			//Draw a wobbling circle that grows bigger as each stage is reached
			float factor = GetChargeFactor();
			float amplitude = 0.5f * factor + 0.2f;
			//3.5f "rotations" per second
			float sin = Math.Abs((float)Math.Sin(MathHelper.ToRadians(6f * 3.5f * animTimer))) * amplitude - 0.3f;

			Vector2 spawn = owner.gravDir > 0
				? owner.Top - new Vector2(0, 20)
				: owner.Bottom + new Vector2(0, 20);

			//Draw a filled circle with the edges fading to transparency
			PrimitiveDrawing.DrawFilledCircle(spawn, sin * 12, Color.Lime, fadeEdge: true);

			return false;
		}

		private int forceDir;

		public override void AI(){
			Player owner = Main.player[projectile.owner];

			animTimer++;

			float targetTime = 15 / PlayerHooks.UseTimeMultiplier(owner, owner.HeldItem);

			if(State < 2 && wait < targetTime - 1)
				owner.ChangeDir(forceDir = owner.Center.X < Main.MouseWorld.X ? 1 : -1);
			else
				owner.ChangeDir(forceDir);

			if(State == 0)
				projectile.Center = owner.Center;
			else if(State == 1){
				float curRot = owner.DirectionTo(projectile.Center).ToRotation();
				float targetRot = owner.DirectionTo(Main.MouseWorld).ToRotation();

				if(Math.Abs(curRot - targetRot) > MathHelper.ToRadians(15))
					projectile.Center = MathHelper.Lerp(curRot, targetRot, 0.25f).ToRotationVector2() * SpawnDistance + owner.Center;
				else if(wait >= targetTime - 1 && wait < targetTime)
					projectile.Center = targetRot.ToRotationVector2() * SpawnDistance + owner.Center;
			}

			if(State < 2)
				projectile.timeLeft = 60;

			if(State == 1)
				wait++;

			//Spawn light where the thing is
			if(State == 0){
				Vector2 loc = owner.gravDir > 0 ? owner.Top - new Vector2(0, 20) : owner.Bottom + new Vector2(0, 20);
				Lighting.AddLight(loc, Color.Green.ToVector3() * 1.3f);
			}else if(State == 1)
				Lighting.AddLight(projectile.Center, Color.Green.ToVector3() * 1.3f);

			if(State == 0 && projectile.soundDelay == 0){
				projectile.soundDelay = 18;

				Main.PlaySound(SoundID.Item15, owner.Center);
			}

			//Only drain mana when the charge stage increases
			if(State == 0 && owner.channel && ((int)Charge % (MaxCharge / 4) != 0 || owner.CheckMana(owner.HeldItem.mana, pay: true)))
				Charge++;
			else if(!owner.channel && State == 0){
				State = 1;
				wait = 0;
				return;
			}

			if(Charge >= MaxCharge && State == 0){
				State = 1;
				wait = 0;
			}else if(State == 1){
				if(wait >= targetTime)
					SpawnProjectile();
			}
		}

		public int BodyIndex(){
			Player owner = Main.player[projectile.owner];

			if(State == 0)
				return 5;
			else if(wait < 15 / PlayerHooks.UseTimeMultiplier(owner, owner.HeldItem) - 1)
				return 6;
			else{
				float rotation = owner.DirectionTo(projectile.Center).ToRotation();

				if(Math.Abs(rotation) < MathHelper.ToRadians(45) || Math.Abs(MathHelper.Pi - Math.Abs(rotation)) < MathHelper.ToRadians(45))
					return 3;
				else if(rotation > 0)
					return 4;
				return 2;
			}
		}

		private float GetChargeFactor(){
			float factor;
			float max = MaxCharge;

			if(Charge < max / 4f)
				factor = 1f;
			else if(Charge >= max / 4f && Charge < max / 2f)
				factor = 2f;
			else if(Charge >= max / 2f && Charge < max * 3f / 4f)
				factor = 3f;
			else if(Charge >= max * 3f / 4f && Charge < max)
				factor = 4f;
			else
				factor = 5f;

			return factor;
		}

		public override bool ShouldUpdatePosition() => false;

		private void SpawnProjectile(){
			projectile.timeLeft = 12;
			State = 2;

			Player owner = Main.player[projectile.owner];

			Main.PlaySound(SoundID.Item84, owner.Center);

			//Scale damage, knockback, speed and size based on how charged the projectile is
			int damage = owner.HeldItem.damage;
			float knockback = owner.HeldItem.knockBack;
			float speed = owner.HeldItem.shootSpeed;
			float scale = 0.4f;

			float factor = GetChargeFactor();

			//Make the projectile appear away from the player, if possible
			Vector2 spawnCenter = owner.Center;
			Vector2 direction = owner.DirectionTo(Main.MouseWorld);
			Vector2 offset = direction * SpawnDistance;
			if(Collision.CanHit(spawnCenter, 0, 0, spawnCenter + offset, 0, 0))
				spawnCenter += offset;

			Projectile spawn = Projectile.NewProjectileDirect(spawnCenter,
				direction * speed * factor,
				ModContent.ProjectileType<TerraBoltProjectile>(),
				(int)(damage * factor),
				knockback * factor,
				owner.whoAmI);
			spawn.scale = scale * factor;
			spawn.width = (int)(spawn.width * spawn.scale);
			spawn.height = (int)(spawn.height * spawn.scale);
		}
	}
}
