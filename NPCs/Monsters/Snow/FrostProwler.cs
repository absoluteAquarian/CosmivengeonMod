using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.NPCs.Monsters.Purity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Monsters.Snow{
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
			if(Main.rand.NextFloat() < 0.1f)
				Item.NewItem(npc.Hitbox, ModContent.ItemType<ProwlerFang>());
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo){
			//Player is in the Snow biome (surface or underground, it doesn't matter)
			return spawnInfo.player.ZoneSnow ? 0.153f : 0;
		}

		public override float JumpStrength => -7.85f;

		public const float vel_CanSee = 8.335f;

		public override float GetWalkSpeed(bool cantSee, bool noTarget){
			if(cantSee && noTarget)
				return vel_CanSee * 0.72f;
			if(cantSee && !noTarget)
				return vel_CanSee * 0.9f;
			return vel_CanSee;
		}
	}
}
