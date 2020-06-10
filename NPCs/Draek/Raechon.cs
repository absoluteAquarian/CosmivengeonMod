using CosmivengeonMod.Items.Draek;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Draek{
	public class Raechon : RaechonAI{
		public override void SetStaticDefaults(){
			Main.npcFrameCount[npc.type] = 4;
		}

		public override void SetDefaults(){
			npc.lifeMax = 375;
			npc.defense = 13;
			npc.damage = 65;
			npc.width = 32;
			npc.height = 32;
			npc.knockBackResist = 0.4f;
			npc.value = Item.buyPrice(gold: 1);

			npc.buffImmune[BuffID.Poisoned] = true;

			npc.HitSound = SoundID.NPCHit33;
			npc.DeathSound = SoundID.NPCDeath27;
		}

		public override void NPCLoot(){
			if(Main.rand.NextFloat() < 0.4f)
				Item.NewItem(npc.Hitbox, ModContent.ItemType<RaechonShell>());
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo){
			//Player is in the Forest biome, it's daytime and the Eye of Cthulhu has been defeated
			//5% chance to spawn
			return CosmivengeonUtils.PlayerIsInForest(spawnInfo.player) && Main.dayTime && NPC.downedBoss1 ? 0.05f : 0;
		}

		public const float vel_CanSee = 5.335f;

		public override float JumpStrength => -3.5f;

		public override float GetWalkSpeed(bool cantSee, bool noTarget) {
			if(cantSee && noTarget)
				return 1.35f;
			if(cantSee && !noTarget)
				return vel_CanSee * 0.85f;
			return vel_CanSee;
		}
	}
}
