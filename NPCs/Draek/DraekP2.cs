using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Summon = CosmivengeonMod.NPCs.Draek.DraekWyrmSummon_Head;

namespace CosmivengeonMod.NPCs.Draek{
	//Head NPC will contain any custom behaviour, as the other segments just follow it
	[AutoloadBossHead]
	public class DraekP2Head : Worm{
		public override string Texture{
			get{
				return "CosmivengeonMod/NPCs/Draek/DraekP2_Head";
			}
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
		}
		
		public override void SetDefaults(){
			head = true;
			
			npc.width = 20;
			npc.height = 20;

			npc.aiStyle = -1;
			npc.lifeMax = 2500;
			npc.defense = 25;
			npc.damage = 40;
			npc.scale = 2f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.DeathSound = SoundID.NPCDeath60;	//Phantasm Dragon death sound

			music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/SuccessorOfTheJewel");
			musicPriority = MusicPriority.BossLow;
			
			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;

			minLength = maxLength = 6;

			headType = mod.NPCType<DraekP2Head>();
			//no bodyType since we have differing body segment textures
			tailType = mod.NPCType<DraekP2Tail>();
			
			speed = speed_subphase0_normal;
			turnSpeed = turnSpeed_subphase0_normal;
			
			maxDigDistance = 16 * 40;
			customBodySegments = true;
		}

		private const float speed_subphase0_normal = 6f;
		private const float speed_subphase0_enraged_normal = 9f;
		private const float speed_subphase0_expert = 8f;
		private const float speed_subphase0_enraged_expert = 11f;
		
		private const float turnSpeed_subphase0_normal = 0.125f;
		private const float turnSpeed_subphase0_enraged_normal = 0.185f;
		private const float turnSpeed_subphase0_expert = 0.15f;
		private const float turnSpeed_subphase0_enraged_expert = 0.205f;

		private const int summonCount_normal = 3;
		private const int summonCount_enraged_normal = 5;
		private const int summonCount_expert = 4;
		private const int summonCount_enraged_expert = 6;

		private const float speed_subphase3_offset = 6f;
		private const float speed_subphase3_normal = 12f;
		private const float speed_subphase3_expert = 16f;
		
		private const int rockCount_normal = 6;
		private const int rockCount_enraged_normal = 10;
		private const int rockCount_expert = 8;
		private const int rockCount_enraged_expert = 14;

		private const int Worm_subphase0 = 0;
		private const int Summon = 1;
		private const int Worm_subphase2 = 2;
		private const int Mega_Charge = 3;
		private const int Enraged_Worm_subphase0 = 4;
		private const int Enraged_Summon = 5;
		private const int Enraged_Worm_subphase2 = 6;
		private const int Enraged_Mega_Charge = 7;

		private const int Phase_2 = 0;
		private const int Phase_2_Enraged = 1;
		private int CurrentPhase = Phase_2;

		private int attackPhase = Worm_subphase0;
		private int attackProgress = 0;
		private int attackTimer = 0;
		private long animationCounter = 0;

		private int SummonedWyrms = 0;

		private bool hasSpawned = false;
		private bool switchPhases = false;
		private bool switchSubPhases = false;

		private float prevSpeed;
		private float prevTurnSpeed;

		public override bool PreNPCLoot(){
			return false;	//Don't drop anything for now
		}

		public override void NPCLoot(){
			//Not implemented yet
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			npc.lifeMax = 2500 + 1000 * (numPlayers + 1);
			npc.damage = 55;
			npc.defense = 25;

			speed = speed_subphase0_expert;
			turnSpeed = turnSpeed_subphase0_expert;
		}

		public override void AI(){
			/*	AI (Normal Mode):
			 *	- Phase 2, 30-100% HP
			 *		- worm.mp4
			 *			- Basic worm attack
			 *			- Burrows through blocks and lunges at the player occasionally
			 *		- Young Wyrm Summon
			 *			- Summons three Young Wyrm enemies which are essentially just worms too
			 *		- worm.mp4 2: Electric Boogaloo
			 *			- Repeats first attack subphase
			 *		- Mega Charge
			 *			- Jewel crackles to signify start of attack
			 *			- Launches himself 20-40 blocks vertically in the direction he was facing
			 *			- Will always be facing the player before attack starts
			 *			- Drops "Draek Rocks" directly downwards as he flies overhead
			 *			- Flies overhead for 2-4 seconds once max height has been reached
			 *				- Needs to fly faster than player can run
			 *			- Falls back down to ground
			 *		- Repeat
			 *	- Phase 2 (enraged), 0-29% HP
			 *		- Same attacks, but faster
			 *		- Spawns five Young Wyrms in summon subphase instead of three
			 *		- Drops more Draek Rocks in subphase 4
			 *		- Flies for 3-5 seconds in subphase 4
			 *	
			 *	AI (Expert Mode):
			 *	- same as normal mode AI except:
			 *		- spawns one additional Young Wyrm in both subphases
			 *		- fires more Draek Rocks during the "Mega Charge" subphase
			 *		- faster move speed in general
			 *		- waits for a shorter amount of time before "Jewel Explosion"
			 *		- flies for longer
			 */
			if(!hasSpawned){
				npc.TargetClosest(true);
				hasSpawned = true;

				Main.NewText("MY JEWEL!", Draek.TextColour);
				Main.NewText("AARG, YOU'LL PAY FOR THAT, YOU WRETCHED LITTLE WORM!", Draek.TextColour);
			}

			if(npc.life < npc.lifeMax * 0.3 && CurrentPhase == Phase_2){
				switchPhases = true;

				Main.NewText("YOU ACCURSED INSECT!  I'LL BURY YOU ALIVE!", Draek.TextColour);
			}

			CheckSubphaseChange();
			CheckPhaseChange();

			if(CurrentPhase == Phase_2){
				float wormSpeed = Main.expertMode ? speed_subphase0_expert : speed_subphase0_normal;
				float wormTurnSpeed = Main.expertMode ? turnSpeed_subphase0_expert : turnSpeed_subphase0_normal;

				if(attackPhase == Worm_subphase0){
					AI_Worm(wormSpeed, wormTurnSpeed, 6 * 60);
				}else if(attackPhase == Summon){
					int count = Main.expertMode ? summonCount_expert : summonCount_normal;

					AI_Summon(count, 15, true, 0f);
				}else if(attackPhase == Worm_subphase2){
					AI_Worm(wormSpeed, wormTurnSpeed, 4 * 60);
				}else if(attackPhase == Mega_Charge){
					int delay = Main.expertMode ? 45 : 60;
					float duration = Main.rand.NextFloat(2, 4) + (Main.expertMode ? 1 : 0);
					int rockCount = Main.expertMode ? rockCount_expert : rockCount_normal;

					AI_MegaCharge(delay, (int)(60 * duration), rockCount);
				}
			}else if(CurrentPhase == Phase_2_Enraged){
				float wormSpeed = Main.expertMode ? speed_subphase0_enraged_expert : speed_subphase0_enraged_normal;
				float wormTurnSpeed = Main.expertMode ? turnSpeed_subphase0_enraged_expert : turnSpeed_subphase0_enraged_normal;

				if(attackPhase == Enraged_Worm_subphase0){
					AI_Worm(wormSpeed, wormTurnSpeed, 5 * 60);
				}else if(attackPhase == Enraged_Summon){
					int count = Main.expertMode ? summonCount_enraged_expert : summonCount_enraged_normal;

					AI_Summon(count, 10, true, 0f);
				}else if(attackPhase == Enraged_Worm_subphase2){
					AI_Worm(wormSpeed, wormTurnSpeed, 3 * 60);
				}else if(attackPhase == Enraged_Mega_Charge){
					int delay = Main.expertMode ? 30 : 45;
					float duration = Main.rand.NextFloat(3, 5) + (Main.expertMode ? 1 : 0);
					int rockCount = Main.expertMode ? rockCount_enraged_expert : rockCount_enraged_normal;

					AI_MegaCharge(delay, (int)(60 * duration), rockCount);
				}
			}

			attackTimer++;
		}

		private void CheckSubphaseChange(){
			if(!switchSubPhases)
				return;
			
			if(attackPhase == Mega_Charge && CurrentPhase == Phase_2)
				attackPhase = Worm_subphase0;
			else if(attackPhase == Enraged_Mega_Charge && CurrentPhase == Phase_2_Enraged)
				attackPhase = Enraged_Worm_subphase0;
			else
				attackPhase++;
			
			attackTimer = 0;
			attackProgress = 0;
			switchSubPhases = false;
		}

		private void CheckPhaseChange(){
			if(!switchPhases)
				return;
			
			CurrentPhase = Phase_2_Enraged;
			attackPhase = Enraged_Worm_subphase0;

			attackTimer = 0;
			attackProgress = 0;
			switchPhases = false;
			switchSubPhases = false;
		}

		private int UpdateWyrmCount(){
			int wyrms = 0;
			
			for(int i = 0; i < Main.npc.Length; i++){
				if(Main.npc[i].type == mod.NPCType<Summon>() && (int)Main.npc[i].ai[1] == npc.whoAmI && Main.npc[i].active)
					wyrms++;
			}
			
			return wyrms;
		}

		private void AI_Worm(float speed, float turnSpeed, int wait){
			fly = false;
			this.speed = speed;
			this.turnSpeed = turnSpeed;
			prevSpeed = speed;
			prevTurnSpeed = turnSpeed;
			if(attackTimer == wait)
				switchSubPhases = true;
		}

		private void AI_Summon(int times, int waitBetween, bool randomAngle, params float[] angles){
			SummonedWyrms = UpdateWyrmCount();

			if(attackProgress >= times || SummonedWyrms == times){
				switchSubPhases = true;
				return;
			}

			if(attackTimer < waitBetween)
				return;
			
			float rotation;
			Vector2 range = npc.Center + new Vector2(Main.rand.NextFloat(-5 * 16, 5 * 16), Main.rand.NextFloat(-5 * 16, 5 * 16));
			
			if(randomAngle)
				rotation = Main.rand.NextFloat(0, 2 * MathHelper.Pi);
			else
				rotation = angles[attackProgress];
			
			NPC.NewNPC((int)range.X, (int)range.Y, mod.NPCType<Summon>(), ai1: npc.whoAmI, ai2: rotation);
			
			SummonedWyrms++;

			attackProgress++;
			attackTimer = 0;
		}

		private void AI_MegaCharge(int delay, int flyTime, int rockCount){
			/*			ATTACK PROGRESS
			 *		0: Slow down and target where player currently is
			 *		1: Move like normal until timer runs out
			 *		2: Charge towards old position and enable flying
			 *		3: Attempt to "hover" directly above player, adjusting speed
			 *			to be always faster than the player's current velocity.
			 *			Also drop "Draek Rock" projectiles
			 */
			if(attackProgress == 0){
				speed = 1f;
				turnSpeed = 0.3f;
				CustomTarget = Main.player[npc.target].Center;
				attackProgress++;
			}else if(attackProgress == 1){
				if(attackTimer < delay)
					return;
				else{
					attackProgress++;
					attackTimer = 0;
				}
			}else if(attackProgress == 2 || attackProgress == 3){
				speed = Main.expertMode ? 16f : 12f;
				fly = true;
				JewelExplosion();
				if(Vector2.Distance(CustomTarget, npc.Center) < 1 * 16)
					attackProgress++;
			}else if(attackProgress >= 4){
				CustomTarget = Main.player[npc.target].Center - new Vector2(0, 16 * 10);
				speed = Main.player[npc.target].velocity.Length() + 6f;

				float rockDelay = flyTime / (rockCount + 1);

				if(Vector2.Distance(npc.Center, CustomTarget) > 9 * 16){
					attackTimer--;
					return;
				}

				if(attackTimer > rockDelay){
					float rockSpawnOffset = Main.rand.NextFloat(-3, 3);
					
					Projectile.NewProjectile(
						npc.Center + new Vector2(rockSpawnOffset, 0),
						Vector2.Zero,
						mod.ProjectileType("DraekRock"),
						CosmivengeonMod.TrueDamage(45),
						16f,
						Main.myPlayer,
						20f,
						0.35f
					);

					Main.PlaySound(SoundID.Item69, npc.Center);

					attackTimer = 0;
					attackProgress++;

					if(attackProgress == 4 + rockCount){
						switchSubPhases = true;
						CustomTarget = Vector2.Zero;
						speed = prevSpeed;
						turnSpeed = prevTurnSpeed;
						fly = false;
					}
				}
			}
		}

		private void JewelExplosion(){
			if(npc.ai[1] == 0f && attackProgress == 2){	//Notify the second body segment to cause the explosion
				npc.ai[1] = 1f;
				attackProgress++;
			}
		}

		public override int SetCustomBodySegments(int startDistance){
			int latestNPC = npc.whoAmI;
			latestNPC = NewBodySegment(mod.NPCType<DraekP2_Body0>(), latestNPC, ref startDistance);
			latestNPC = NewBodySegment(mod.NPCType<DraekP2_Body1>(), latestNPC, ref startDistance);
			latestNPC = NewBodySegment(mod.NPCType<DraekP2_Body2>(), latestNPC, ref startDistance);
			latestNPC = NewBodySegment(mod.NPCType<DraekP2_Body3>(), latestNPC, ref startDistance);
			return latestNPC;
		}
	}

	//Draek only has 4 body segments, so I can just hardcode the classes :chaelure:
	internal class DraekP2_Body0 : Worm{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
		}

		public override void SetDefaults(){
			npc.CloneDefaults(headType);

			npc.boss = true;
			
			npc.width = 15;
			npc.height = 15;
			
			npc.aiStyle = -1;
			npc.lifeMax = 2500;
			npc.defense = 25;
			npc.damage = 40;
			npc.scale = 2f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;

			npc.dontCountMe = true;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.DeathSound = SoundID.NPCDeath60;	//Phantasm Dragon death sound
		}

		public override bool PreNPCLoot(){
			return false;	//Don't drop anything
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			npc.lifeMax = 2500 + 1000 * (numPlayers + 1);
			npc.damage = 55;
			npc.defense = 25;
		}
	}
	internal class DraekP2_Body1 : DraekP2_Body0{
		public override void AI(){
			if(Main.npc[(int)npc.ai[3]].ai[1] == 1f){	//Head segment has flagged for this one to cause the "Jewel Explosion"
				//Play the sounds
				Main.PlaySound(SoundID.Item27, npc.Center);		//Crystal break sound effect
				Main.PlaySound(SoundID.Item70, npc.Center);		//Staff of Earth alternative sound effect

				//Spawn the dust
				for(int i = 0; i < 60; i++){
					Dust.NewDust(npc.Center, 50, 50, 74, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8));
					Dust.NewDust(npc.Center, 50, 50, 107, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8));
				}
				
				Main.npc[(int)npc.ai[3]].ai[1] = 0f;
			}
		}
	}
	internal class DraekP2_Body2 : DraekP2_Body0{}
	internal class DraekP2_Body3 : DraekP2_Body0{}

	internal class DraekP2Tail : Worm{
		public override string Texture{
			get{
				return "CosmivengeonMod/NPCs/Draek/DraekP2_Tail";
			}
		}

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
		}

		public override void SetDefaults(){
			npc.CloneDefaults(headType);

			npc.boss = true;
			
			npc.aiStyle = -1;
			npc.lifeMax = 500;
			npc.defense = 25;
			npc.damage = 40;
			npc.scale = 2f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;

			tail = true;
			npc.width = 15;
			npc.height = 15;

			npc.dontCountMe = true;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.DeathSound = SoundID.NPCDeath60;	//Phantasm Dragon death sound
		}

		public override bool PreNPCLoot(){
			return false;	//Don't drop anything
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			npc.lifeMax = 2500 + 1000 * (numPlayers + 1);
			npc.damage = 55;
			npc.defense = 25;
		}
	}
}
