using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Utility;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Monsters.Purity{
	public class Raechon : RaechonAI{
		public override void SetStaticDefaults(){
			Main.npcFrameCount[NPC.type] = 4;
		}

		public override void SetDefaults(){
			NPC.lifeMax = 375;
			NPC.defense = 13;
			NPC.damage = 65;
			NPC.width = 32;
			NPC.height = 32;
			NPC.knockBackResist = 0.4f;
			NPC.value = Item.buyPrice(gold: 1);

			NPC.buffImmune[BuffID.Poisoned] = true;

			NPC.HitSound = SoundID.NPCHit33;
			NPC.DeathSound = SoundID.NPCDeath27;
		}

		public override void OnKill(){
			if(Main.rand.NextFloat() < 0.4f)
				Item.NewItem(NPC.Hitbox, ModContent.ItemType<RaechonShell>());
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo){
			//Player is in the Forest biome, it's daytime and the Eye of Cthulhu has been defeated
			//5% chance to spawn
			return MiscUtils.PlayerIsInForest(spawnInfo.Player) && Main.dayTime && NPC.downedBoss1 ? 0.05f : 0;
		}

		public const float vel_CanSee = 8.335f;

		public override float JumpStrength => -5.5f;

		public override float GetWalkSpeed(bool cantSee, bool noTarget) {
			if(cantSee && noTarget)
				return vel_CanSee * 0.62f;
			if(cantSee && !noTarget)
				return vel_CanSee * 0.8f;
			return vel_CanSee;
		}
	}
}
