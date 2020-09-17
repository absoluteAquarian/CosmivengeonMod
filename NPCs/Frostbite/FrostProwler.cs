﻿using CosmivengeonMod.Items.Frostbite;
using CosmivengeonMod.NPCs.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Frostbite{
	public class FrostProwler : RaechonAI{
		public override void SetStaticDefaults(){
			Main.npcFrameCount[npc.type] = 4;
		}

		public override void SetDefaults(){
			npc.lifeMax = 120;
			npc.defense = 6;
			npc.damage = 22;
			npc.width = 48;
			npc.height = 34;
			npc.knockBackResist = 0.72f;
			npc.value = Item.buyPrice(silver: 21);

			npc.buffImmune[BuffID.Frostburn] = true;

			npc.HitSound = SoundID.NPCHit11;	//Snow NPC hit sound
			npc.DeathSound = SoundID.NPCDeath27;
		}

		public override void NPCLoot(){
			if(Main.rand.NextFloat() < 0.4f)
				Item.NewItem(npc.Hitbox, ModContent.ItemType<FrostCrystal>(), Main.rand.Next(1, 4));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo){
			//Player is in the Forest biome, it's daytime and the Eye of Cthulhu has been defeated
			//5% chance to spawn
			return spawnInfo.player.ZoneSnow ? 0.153f : 0;
		}

		public override float JumpStrength => -7.85f;

		public const float vel_CanSee = 5.335f;

		public override float GetWalkSpeed(bool cantSee, bool noTarget) {
			if(cantSee && noTarget)
				return 2.1f;
			if(cantSee && !noTarget)
				return vel_CanSee;
			return vel_CanSee * 1.3f;
		}
	}
}