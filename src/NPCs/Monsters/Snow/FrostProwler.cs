﻿using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.NPCs.Monsters.Purity;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Monsters.Snow {
	public class FrostProwler : RaechonAI {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
		}

		public override void SetDefaults() {
			NPC.lifeMax = 120;
			NPC.defense = 6;
			NPC.damage = 22;
			NPC.width = 48;
			NPC.height = 34;
			NPC.knockBackResist = 0.72f;
			NPC.value = Item.buyPrice(silver: 21);

			NPC.buffImmune[BuffID.Frostburn] = true;

			NPC.HitSound = SoundID.NPCHit11;    //Snow NPC hit sound
			NPC.DeathSound = SoundID.NPCDeath27;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new CommonDrop(ModContent.ItemType<FrostCrystal>(), 5, 1, 3, 2));
			npcLoot.Add(new CommonDrop(ModContent.ItemType<ProwlerFang>(), 10));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			//Player is in the Snow biome (surface or underground, it doesn't matter)
			return spawnInfo.Player.ZoneSnow ? 0.153f : 0;
		}

		public override float JumpStrength => -7.85f;

		public const float vel_CanSee = 8.335f;

		public override float GetWalkSpeed(bool cantSee, bool noTarget) {
			if (cantSee && noTarget)
				return vel_CanSee * 0.72f;
			if (cantSee && !noTarget)
				return vel_CanSee * 0.9f;
			return vel_CanSee;
		}
	}
}
