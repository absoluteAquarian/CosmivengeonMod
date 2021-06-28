using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss{
	public class DraekP1ExtraProjectile : ModProjectile{
		public override string Texture => "CosmivengeonMod/Assets/Empty";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
		}

		public override void SetDefaults(){
			projectile.width = 125;
			projectile.height = 23;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.npcProj = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.alpha = 255;
		}

		private bool spawned = false;
		private Draek Parent = null;

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => false;

		public override void AI(){
			if(projectile.width == 0 || projectile.height == 0)
				projectile.hostile = false;
			else
				projectile.hostile = true;

			if(!spawned){
				spawned = true;
				Parent = Main.npc[(int)projectile.ai[0]].modNPC as Draek;
			}

			//kill this projectile if the boss it's attached to has died or despawned
			if(Parent?.npc.active != true || Main.npc[Parent.npc.whoAmI].type != Parent.npc.type){
				projectile.active = false;
				return;
			}

			Vector4 packet = Parent.GetLastingProjectileHitbox();
			Vector2 parentVisualPosition = Parent.npc.Center - Main.npcTexture[Parent.npc.type].Frame(1, Main.npcFrameCount[Parent.npc.type], 0, 0).Size() / 2f;

			projectile.position = (parentVisualPosition + new Vector2(packet.X, packet.Y)) * Parent.npc.scale;
			projectile.width = (int)packet.Z;
			projectile.height = (int)packet.W;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			//Only apply the Primordial Wrath debuff if the world is in Desolation mode
			if(WorldEvents.desoMode)
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 2 * 60);
		}
	}
}
