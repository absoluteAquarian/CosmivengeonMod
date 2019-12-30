using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Draek{
	public class DraekP1ExtraHurtbox : ModNPC{
		public override string Texture => "CosmivengeonMod/NPCs/Draek/Draek";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
		}

		public override void SetDefaults(){
			npc.CloneDefaults(ModContent.NPCType<Draek>());
			npc.alpha = 255;
			npc.dontCountMe = true;
		}

		private bool spawned = false;
		private Draek Parent = null;

		public override void AI(){
			if(!spawned){
				spawned = true;
				Parent = Main.npc[(int)npc.ai[0]].modNPC as Draek;
			}

			//kill this NPC if the boss it's attached to has died or despawned
			if(Parent?.npc.active != true || Main.npc[Parent.npc.whoAmI].type != Parent.npc.type){
				npc.active = false;
				return;
			}

			npc.dontTakeDamage = Parent.npc.dontTakeDamage;

			//Update the position and size of this hitbox according to the Parent we have
			Vector4 packet = Parent.GetExtraHurtbox();
			Vector2 parentVisualPosition = Parent.npc.Center - Main.npcTexture[Parent.npc.type].Frame(1, Main.npcFrameCount[Parent.npc.type], 0, 0).Size() / 2f;

			npc.position = (parentVisualPosition + new Vector2(packet.X, packet.Y)) * Parent.npc.scale;
			npc.width = (int)packet.Z;
			npc.height = (int)packet.W;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
			=> Parent?.OnHitPlayer(target, damage, crit);

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

		//Thanks jopojelly for helping me figure out how to make the Parent-Child link work for the two hooks below:
		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit){
			if(Parent != null)
				Parent.npc.immune[player.whoAmI] = player.itemAnimation;
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit){
			if(Parent != null){
				Parent.npc.immune[projectile.owner] = npc.immune[projectile.owner];

				if(projectile.usesLocalNPCImmunity)
					projectile.localNPCImmunity[Parent.npc.whoAmI] = projectile.localNPCHitCooldown;
			}
		}
	}
}
