using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Draek{
	public class DraekWyrmSummon_Head : Worm{
		private bool hasSpawned = false;
		public int bossID = 0;
		private int AcidSpitTimer = -1;
		
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Young Wyrm");
		}

		public override void SetDefaults(){
			head = true;

			npc.width = 25;
			npc.height = 25;
			
			npc.aiStyle = -1;
			npc.lifeMax = 200;
			npc.defense = 12;
			npc.damage = 20;
			npc.scale = 1f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			minLength = maxLength = 4;

			headType = ModContent.NPCType<DraekWyrmSummon_Head>();
			//no body type b/c differing body segments
			tailType = ModContent.NPCType<DraekWyrmSummon_Tail>();

			speed = 7f;
			turnSpeed = 0.1f;

			fly = false;
			maxDigDistance = 16 * 15;
			customBodySegments = true;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound

			bossID = (int)npc.ai[1];
		}

		public override int SetCustomBodySegments(int startDistance){
			int latestNPC = npc.whoAmI;
			latestNPC = NewBodySegment(ModContent.NPCType<DraekWyrmSummon_Body0>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekWyrmSummon_Body1>(), latestNPC);
			return latestNPC;
		}

		public override void AI(){
			if(!hasSpawned){
				hasSpawned = true;
				npc.TargetClosest(false);
			}

			if(npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead){
				npc.TargetClosest(true);
			}

			if(Vector2.Distance(npc.Center, Main.player[npc.target].Center) > 100 * 16){
				npc.life = 0;
				npc.active = false;
			}

			//Occasionally spit acid
			if(CosmivengeonWorld.desoMode){
				if(AcidSpitTimer < 0){
					npc.SpawnProjectile(npc.position,
						Vector2.Zero,
						ModContent.ProjectileType<Projectiles.Draek.DraekAcidSpit>(),
						20,
						3f,
						Main.myPlayer,
						Main.player[npc.target].Center.X,
						Main.player[npc.target].Center.Y
					);

					AcidSpitTimer = Main.rand.Next(60, 150);
				}
				AcidSpitTimer--;
			}
		}
	}

	internal class DraekWyrmSummon_Body0 : Worm{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Young Wyrm");
		}

		public override void SetDefaults(){
			npc.width = 30;
			npc.height = 30;
			
			npc.aiStyle = -1;
			npc.lifeMax = 250;
			npc.defense = 12;
			npc.damage = 20;
			npc.scale = 1f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.dontCountMe = true;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
		}
	}
	internal class DraekWyrmSummon_Body1 : DraekWyrmSummon_Body0{
		public override void SetDefaults(){
			npc.CloneDefaults(ModContent.NPCType<DraekWyrmSummon_Body0>());

			npc.width = 25;
			npc.height = 25;
		}
	}

	internal class DraekWyrmSummon_Tail : Worm{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Young Wyrm");
		}

		public override void SetDefaults(){
			tail = true;
			
			npc.width = 20;
			npc.height = 20;
			
			npc.aiStyle = -1;
			npc.lifeMax = 250;
			npc.defense = 12;
			npc.damage = 20;
			npc.scale = 1f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.dontCountMe = true;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
		}
	}
}
