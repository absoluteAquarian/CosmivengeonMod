using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Draek{
	public class DraekWyrmSummon_Head : Worm{
		private bool hasSpawned = false;
		public int bossID = 0;
		private int AcidSpitTimer = -1;
		private int FastChargeTimer = -1;
		public bool fastCharge = false;
		private float prevSpeed;
		private float prevTurnSpeed;
		private int baseDefense;
		
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Young Wyrm");
		}

		public override void SetDefaults(){
			head = true;

			npc.width = 25;
			npc.height = 25;
			
			npc.aiStyle = -1;
			npc.lifeMax = 200;
			npc.defense = 6;
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

			speed = CosmivengeonUtils.GetModeChoice(7, 8, 10);
			turnSpeed = 0.1f;

			prevSpeed = speed;
			prevTurnSpeed = turnSpeed;

			fly = false;
			maxDigDistance = 16 * CosmivengeonUtils.GetModeChoice(15, 10, 7);
			customBodySegments = true;

			npc.HitSound = new LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound

			bossID = (int)npc.ai[1];

			baseDefense = npc.defense;
		}

		public override int SetCustomBodySegments(int startDistance){
			int latestNPC = npc.whoAmI;
			latestNPC = NewBodySegment(ModContent.NPCType<DraekWyrmSummon_Body0>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekWyrmSummon_Body1>(), latestNPC);
			return latestNPC;
		}

		public override void SendExtraAI(BinaryWriter writer){
			BitsByte flag = new BitsByte(hasSpawned, fastCharge);
			writer.Write(flag);
			writer.Write(AcidSpitTimer);
			writer.Write((byte)bossID);
			writer.Write(FastChargeTimer);
			writer.Write((byte)baseDefense);
		}

		public override void ReceiveExtraAI(BinaryReader reader){
			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref hasSpawned, ref fastCharge);
			AcidSpitTimer = reader.ReadInt32();
			bossID = reader.ReadByte();
			FastChargeTimer = reader.ReadInt32();
			baseDefense = reader.ReadByte();
		}

		public override void AI(){
			if(!hasSpawned){
				hasSpawned = true;
				npc.TargetClosest(false);
				FastChargeTimer = Main.rand.Next(6 * 60, 10 * 60);
				npc.netUpdate = true;
			}

			if(npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead){
				npc.TargetClosest(true);
				npc.netUpdate = true;
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

					npc.netUpdate = true;
				}
				AcidSpitTimer--;
			}

			//Check for Expert+ and the fast charge
			if(Main.expertMode){
				FastChargeTimer--;

				if(!fastCharge && FastChargeTimer == 0){
					//If the timer is 0, do the thing
					fastCharge = true;
					speed = prevSpeed + 4f;
					turnSpeed = 0.25f;
					FastChargeTimer = 120;
					npc.defense = 0;

					Main.PlaySound(SoundID.Item27.WithVolume(0.35f), npc.Center);

					npc.netUpdate = true;
				}else if(fastCharge && FastChargeTimer == 0){
					fastCharge = false;
					speed = prevSpeed;
					turnSpeed = prevTurnSpeed;
					FastChargeTimer = Main.rand.Next(6 * 60, 10 * 60);
					npc.defense = baseDefense;

					npc.netUpdate = true;
				}
			}

			if(fastCharge && Main.rand.NextFloat() < 0.85f){
				Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, 74);
				dust.velocity = Vector2.Zero;
				dust.noGravity = true;
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
			npc.defense = 6;
			npc.damage = 20;
			npc.scale = 1f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.dontCountMe = true;

			npc.HitSound = new LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
		}

		public override void AI(){
			if(Main.npc[(int)npc.ai[3]].modNPC is DraekWyrmSummon_Head head){
				if(head.fastCharge && Main.rand.NextFloat() < 0.1667){
					Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, 74);
					dust.velocity = Vector2.Zero;
					dust.noGravity = true;
				}

				npc.defense = head.npc.defense;
			}
		}
	}
	internal class DraekWyrmSummon_Body1 : DraekWyrmSummon_Body0{
		public override void SetDefaults(){
			npc.CloneDefaults(ModContent.NPCType<DraekWyrmSummon_Body0>());

			npc.width = 25;
			npc.height = 25;
		}
	}

	internal class DraekWyrmSummon_Tail : DraekWyrmSummon_Body0{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Young Wyrm");
		}

		public override void SetDefaults(){
			tail = true;
			
			npc.width = 20;
			npc.height = 20;
			
			npc.aiStyle = -1;
			npc.lifeMax = 250;
			npc.defense = 6;
			npc.damage = 20;
			npc.scale = 1f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.dontCountMe = true;

			npc.HitSound = new LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
		}
	}
}
