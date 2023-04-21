using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss {
	public class DraekP1ExtraProjectile : ModProjectile {
		public override string Texture => "CosmivengeonMod/Assets/Empty";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Forsaken Oronoblade");
		}

		public override void SetDefaults() {
			Projectile.width = 125;
			Projectile.height = 23;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.npcProj = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.alpha = 255;
		}

		private bool spawned = false;
		private Draek Parent = null;

		public override bool PreDraw(ref Color lightColor) => false;

		public override void AI() {
			if (Projectile.width == 0 || Projectile.height == 0)
				Projectile.hostile = false;
			else
				Projectile.hostile = true;

			if (!spawned) {
				spawned = true;
				Parent = Main.npc[(int)Projectile.ai[0]].ModNPC as Draek;
			}

			//kill this projectile if the boss it's attached to has died or despawned
			if (Parent?.NPC.active != true || Main.npc[Parent.NPC.whoAmI].type != Parent.NPC.type) {
				Projectile.active = false;
				return;
			}

			Vector4 packet = Parent.GetLastingProjectileHitbox();
			Vector2 parentVisualPosition = Parent.NPC.Center - TextureAssets.Npc[Parent.NPC.type].Value.Frame(1, Main.npcFrameCount[Parent.NPC.type], 0, 0).Size() / 2f;

			Projectile.position = (parentVisualPosition + new Vector2(packet.X, packet.Y)) * Parent.NPC.scale;
			Projectile.width = (int)packet.Z;
			Projectile.height = (int)packet.W;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			//Only apply the Primordial Wrath debuff if the world is in Desolation mode
			if (WorldEvents.desoMode)
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 2 * 60);
		}
	}
}
