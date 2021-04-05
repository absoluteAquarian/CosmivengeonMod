using CosmivengeonMod.Projectiles.Draek;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Summon = CosmivengeonMod.NPCs.Draek.DraekWyrmSummon_Head;

namespace CosmivengeonMod.NPCs.Draek{
	[AutoloadBossHead]
	public class Draek : ModNPC{
		public static int Value => Item.buyPrice(gold: 6, silver: 50);

		private bool dashing = false;
		private bool dashWait = true;
		private bool startDespawn = false;

		private int GraphicsOffsetX = -49;
		private int GraphicsOffsetY = -52;

		public Rectangle RealFrame;

		public Texture2D RealTexture;

		private int animationOffset;
		public int AnimationOffset{
			get{
				FindFrame(Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type]);
				return animationOffset;
			}
		}

		private DraekArm ArmLeft;
		private DraekArm ArmRight;
		
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
			Main.npcFrameCount[npc.type] = 4;
			NPCID.Sets.TrailCacheLength[npc.type] = 5;
			NPCID.Sets.TrailingMode[npc.type] = 0;
		}

		public int HealthThreshold;

		public static readonly int BaseHealth = 2500;

		public static float GetHealthAugmentation() => CosmivengeonUtils.GetModeChoice(0, 0.65f, 0.8f);

		public override void SetDefaults(){
			npc.scale = 1f;
			//Draek frame dimentions:  186x290px
			npc.width = 74;
			npc.height = 233;
			npc.aiStyle = -1;	//-1 means that this enemy has a unique AI; don't copy an existing style
			npc.damage = 45;
			npc.defense = 18;
			npc.lifeMax = BaseHealth;
			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.noGravity = true;
			npc.knockBackResist = 0f;	//100% kb resist
			npc.npcSlots = 30f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noTileCollide = true;

			npc.value = Value;

			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;
			npc.buffImmune[BuffID.Frostburn] = true;

			CosmivengeonUtils.PlayMusic(this, CosmivengeonBoss.Draek);

			//Multiplayer is cringe!
			RealFrame.Width = 1;
			RealFrame.Height = 1;
			if(!Main.dedServ){
				RealTexture = ModContent.GetTexture("CosmivengeonMod/NPCs/Draek/Draek_Animations");

				RealFrame.Width = RealTexture.Frame(4, 5).Width;
				RealFrame.Height = RealTexture.Frame(4, 5).Height;
			}
		}

		public override bool PreNPCLoot(){
			return false;	//First phase shouldn't drop anything
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			//Health increase handled in DetourNPC

			if(!CosmivengeonWorld.desoMode){
				npc.damage = 55;
				npc.defense = 22;
			}else{
				npc.damage = 80;
				npc.defense = 26;
			}
		}

		public override void SendExtraAI(BinaryWriter writer){
			BitsByte flag = new BitsByte();
			flag[0] = ForceFastCharge;
			flag[1] = FastCharge_SwordActive;
			flag[2] = FastChargeStarted;
			flag[3] = FastChargeEnded;
			flag[4] = preDash;
			flag[5] = preDashWait;
			flag[6] = noTargetsAlive;
			flag[7] = dashing;

			BitsByte flag2 = new BitsByte();
			flag2[0] = dashWait;
			flag2[1] = startDespawn;
			flag2[2] = hasSpawned;
			flag2[3] = switchPhases;
			flag2[4] = switchSubPhases;
			flag2[5] = throwingSword;
			flag2[6] = delayPhaseChange;

			writer.Write(flag);
			writer.Write(flag2);
			writer.Write((byte)CurrentPhase);
			writer.Write((byte)afterImageLength);
			writer.Write(preFastChargeAI[0]);
			writer.Write(preFastChargeAI[1]);
			writer.Write(preFastChargeAI[2]);
			writer.Write(preFastChargeAI[3]);
			writer.Write((byte)playerTarget.whoAmI);
			writer.Write((byte)SummonedWyrms);
			writer.Write((byte)animationOffset);
			writer.Write(oldOffsetX);
			writer.Write(oldOffsetY);
			writer.Write(syncOffsetX);
			writer.Write(syncOffsetY);
		}

		public override void ReceiveExtraAI(BinaryReader reader){
			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref ForceFastCharge, ref FastCharge_SwordActive, ref FastChargeStarted, ref FastChargeEnded, ref preDash, ref preDashWait, ref noTargetsAlive, ref dashing);

			BitsByte flag2 = reader.ReadByte();
			flag2.Retrieve(ref dashWait, ref startDespawn, ref hasSpawned, ref switchPhases, ref switchSubPhases, ref throwingSword, ref delayPhaseChange);

			CurrentPhase = reader.ReadByte();
			afterImageLength = reader.ReadByte();
			preFastChargeAI[0] = reader.ReadSingle();
			preFastChargeAI[1] = reader.ReadSingle();
			preFastChargeAI[2] = reader.ReadSingle();
			preFastChargeAI[3] = reader.ReadSingle();
			playerTarget = Main.player[reader.ReadByte()];
			SummonedWyrms = reader.ReadByte();
			animationOffset = reader.ReadByte();

			oldOffsetX = reader.ReadInt32();
			oldOffsetY = reader.ReadInt32();
			syncOffsetX = reader.ReadInt32();
			syncOffsetY = reader.ReadInt32();
		}

		//consts for showing what ai[] slot does what
		private const int AI_Timer_Slot = 0;
		private const int AI_Attack_Slot = 1;
		private const int AI_Attack_Progress_Slot = 2;
		private const int AI_Animation_Counter_Slot = 3;

		//states for ai[1] = AI_Attack
		public const int Attack_Idle = 0;
		public const int Attack_Shoot = 1;
		public const int Attack_Throw_Sword = 2;
		public const int Attack_Shoot_No_Sword = 3;
		public const int Attack_Retrieve_Sword = 5;
		public const int Attack_Punch = 6;

		//properties for ai[]
		public ref float AI_Timer => ref npc.ai[AI_Timer_Slot];
		public ref float AI_Attack => ref npc.ai[AI_Attack_Slot];
		public ref float AI_Attack_Progress => ref npc.ai[AI_Attack_Progress_Slot];
		public ref float AI_Animation_Counter => ref npc.ai[AI_Animation_Counter_Slot];

		//Consts for animation frames
		private const int Idle_Sword_0 = 0;
		private const int Idle_Sword_1 = 1;
		private const int Idle_Sword_2 = 2;
		private const int Idle_Sword_3 = 3;
		private const int Idle_No_Sword_0 = 4;
		private const int Idle_No_Sword_1 = 5;
		private const int Idle_No_Sword_2 = 6;
		private const int Idle_No_Sword_3 = 7;
		private const int Throw_Sword_0 = 8;
		private const int Throw_Sword_1 = 9;
		private const int Throw_Sword_2 = 10;
		private const int Throw_Sword_3 = 11;
		private const int Retrieve_Sword_0 = 12;
		private const int Retrieve_Sword_1 = 13;
		private const int Retrieve_Sword_2 = 14;
		private const int Retrieve_Sword_3 = 15;
		private const int Idle_NoArms_0 = 16;
		private const int Idle_NoArms_1 = 17;
		private const int Idle_NoArms_2 = 18;
		private const int Idle_NoArms_3 = 19;

		//Variables
		private int CurrentPhase = Phase_1;
		private int afterImageLength = 0;
		private bool hasSpawned = false;
		private bool switchPhases = false;
		private bool switchSubPhases = false;
		private bool throwingSword = false;
		private bool delayPhaseChange = false;
		private const int P1_subphase0_Attacks = 6;
		private const int P1_subphase2_Attacks = 9;
		private const int P1_Enrage_subphase0_Attacks = 7;
		private const int P1_Enrage_subphase2_Attacks = 10;
		private const int Phase_1 = 0;
		private const int Phase_1_Enrage = 1;

		private const int Punch_PrimeFirst = 0;
		private const int Punch_PunchFirst = 1;
		private const int Punch_PrimeSecond = 2;
		private const int Punch_PunchSecond = 3;
		private const int Punch_Wait = 4;
		private const int Punch_RetriveFirst = 5;
		private const int Punch_RetrieveSecond = 6;

		public bool ForceFastCharge = false;
		public bool FastCharge_SwordActive = true;
		public bool FastChargeStarted = false;
		public bool FastChargeEnded = false;

		public float[] preFastChargeAI = new float[4];
		public bool preDash = false;
		public bool preDashWait = true;
		
		public static Color TextColour = new Color(55, 148, 107);

		private bool noTargetsAlive;
		
		private Player playerTarget;

		private int SummonedWyrms = 0;

		private int howManyArms = 2;
		
		private int oldOffsetX, oldOffsetY;
		private int syncOffsetX, syncOffsetY;

		public override void FindFrame(int frameHeight){
			int CounterMod30 = (int)(AI_Animation_Counter % 30);
			int offsetX = oldOffsetX, offsetY = oldOffsetY;
			bool useSwordThrowAnimation = (CurrentPhase == Phase_1 || CurrentPhase == Phase_1_Enrage) && throwingSword;
			if(CurrentPhase == Phase_1 || CurrentPhase == Phase_1_Enrage){
				if(AI_Attack == Attack_Idle || AI_Attack == Attack_Shoot || (AI_Attack == Attack_Throw_Sword && !useSwordThrowAnimation) || (ForceFastCharge && FastCharge_SwordActive)){
					if(CounterMod30 < 7)
						offsetX = Idle_Sword_0;
					else if(CounterMod30 < 15)
						offsetX = Idle_Sword_1;
					else if(CounterMod30 < 22)
						offsetX = Idle_Sword_2;
					else
						offsetX = Idle_Sword_3;
				}else if(AI_Attack == Attack_Throw_Sword && useSwordThrowAnimation){
					if(CounterMod30 < 7)
						offsetX = Throw_Sword_0;
					else if(CounterMod30 < 15)
						offsetX = Throw_Sword_1;
					else if(CounterMod30 < 22)
						offsetX = Throw_Sword_2;
					else
						offsetX = Throw_Sword_3;
				}else if(AI_Attack == Attack_Shoot_No_Sword || (ForceFastCharge && !FastCharge_SwordActive)){
					if(CounterMod30 < 7)
						offsetX = Idle_No_Sword_0;
					else if(CounterMod30 < 15)
						offsetX = Idle_No_Sword_1;
					else if(CounterMod30 < 22)
						offsetX = Idle_No_Sword_2;
					else
						offsetX = Idle_No_Sword_3;
				}else if(AI_Attack == Attack_Retrieve_Sword){
					if(CounterMod30 < 7)
						offsetX = Retrieve_Sword_0;
					else if(CounterMod30 < 15)
						offsetX = Retrieve_Sword_1;
					else if(CounterMod30 < 22)
						offsetX = Retrieve_Sword_2;
					else
						offsetX = Retrieve_Sword_3;
				}else if(AI_Attack == Attack_Punch){
					offsetX = 0;
					offsetY = 0;

					string texture = "Draek_Punch";
					texture += npc.spriteDirection == -1 ? "Right" : "Left";
					if(PunchProgress == Punch_PrimeFirst || PunchProgress == Punch_PunchFirst){
						//Priming/punching first punch
						texture += "_Both";

						offsetY = (int)(AI_Animation_Counter / 7.5f);
					}else if(PunchProgress == Punch_PrimeSecond || PunchProgress == Punch_PunchSecond){
						offsetY = (int)(AI_Animation_Counter / 7.5f);
					}

					if(PunchProgress != Punch_Wait && PunchProgress != Punch_RetriveFirst && PunchProgress != Punch_RetrieveSecond){
						RealTexture = mod.GetTexture($"NPCs/Draek/{texture}");
						RealFrame = RealTexture.Frame(1, 8);
					}else{
						if(CounterMod30 < 7)
							offsetX = Idle_NoArms_0;
						else if(CounterMod30 < 15)
							offsetX = Idle_NoArms_1;
						else if(CounterMod30 < 22)
							offsetX = Idle_NoArms_2;
						else
							offsetX = Idle_NoArms_3;
					}
				}
			}

			if(AI_Attack != Attack_Punch || (AI_Attack == Attack_Punch && PunchProgress == Punch_Wait)){
				RealTexture = mod.GetTexture($"NPCs/Draek/Draek_Animations");
				RealFrame = RealTexture.Frame(4, 5);

				GraphicsOffsetX = -49;
				GraphicsOffsetY = -52;

				if(npc.spriteDirection == 1)
					GraphicsOffsetX = -65;

				offsetY = offsetX / 4;
				offsetX %= 4;

				animationOffset = offsetY * 4 + offsetX;
			}else{
				GraphicsOffsetX = -131;
				GraphicsOffsetY = -6;

				if(npc.spriteDirection == -1)
					GraphicsOffsetX = -55;

				animationOffset = offsetX;
			}
			
			RealFrame.X = offsetX * RealFrame.Width;
			RealFrame.Y = offsetY * RealFrame.Height;

			oldOffsetX = offsetX;
			oldOffsetY = offsetY;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			if(CosmivengeonWorld.desoMode)
				target.AddBuff(ModContent.BuffType<Buffs.PrimordialWrath>(), 150);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			//Only apply afterimage effects while dashing
			if(afterImageLength > 1){
				//Afterimage effect
				Vector2 drawOrigin = RealFrame.Size() / 2f;
				SpriteEffects effect = (npc.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				for (int k = 0; k < afterImageLength / 2; k++){
					Vector2 drawPos = npc.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(GraphicsOffsetX, GraphicsOffsetY);
					
					Color color = npc.GetAlpha(lightColor) * (((float)npc.oldPos.Length - k) / npc.oldPos.Length);
					color.A = (byte)(0.75f * 255f * (npc.oldPos.Length - k) / npc.oldPos.Length);	//Apply transparency

					spriteBatch.Draw(RealTexture, drawPos, RealFrame, color, npc.rotation, drawOrigin, npc.scale, effect, 0f);
				}
			}
			return false;	//Prevent sprite from being drawn normally
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor){
			//We're going to manually draw Draek to better fit his hitbox size
			SpriteEffects effect = (npc.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 drawOrigin = RealFrame.Size() / 2f;
			Vector2 drawPos = npc.position - Main.screenPosition + drawOrigin + new Vector2(GraphicsOffsetX, GraphicsOffsetY);

			spriteBatch.Draw(RealTexture, drawPos, RealFrame, drawColor, npc.rotation, drawOrigin, npc.scale, effect, 0f);

#pragma warning disable CS0162
			if(ShowDebug){
				Vector2 pos = npc.Top + new Vector2(-npc.width - 50, -70) - Main.screenPosition;
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText,
					$"AI: [ {(int)npc.ai[0],3}, {(int)npc.ai[1]}, {(int)npc.ai[2],2}, {(int)npc.ai[3],3} ]",
					pos.X,
					pos.Y,
					Color.White,
					Color.Black,
					Vector2.Zero);
				pos.Y += 18;
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText,
					$"Animation: [ {RealFrame.X / RealFrame.Width}, {RealFrame.Y / RealFrame.Height} ]",
					pos.X,
					pos.Y,
					Color.White,
					Color.Black,
					Vector2.Zero);
				pos.Y += 18;
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText,
					$"Texture: \"{(AI_Attack == Attack_Punch ? "Punch Sheet" : "Main Sheet")}\"",
					pos.X,
					pos.Y,
					Color.White,
					Color.Black,
					Vector2.Zero);
			}
#pragma warning restore CS0162
		}

		private const bool ShowDebug = false;

		public override bool CheckActive(){
			return Vector2.Distance(npc.Center, playerTarget.Center) > 200 * 16;
		}

		public override void HitEffect(int hitDirection, double damage){
			if(npc.life - damage < HealthThreshold){
				npc.life = 0;

				int newNPC = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DraekP2Head>());
				Main.npc[newNPC].modNPC.music = music;
				Main.npc[newNPC].modNPC.musicPriority = musicPriority;

				//Spawn 8 gores, 4 per arm
				Vector2 goreTop = npc.Center;
				goreTop.X += (npc.TopLeft.X - npc.Center.X) / 2f;
				goreTop.Y += 20f;
				if((npc.spriteDirection == 1 && howManyArms == 2) || (npc.spriteDirection == -1 && howManyArms > 0)){
					for(int i = 0; i < 4; i++){
						int gore = Gore.NewGore(goreTop + new Vector2(0, 16 * i), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, 5)), mod.GetGoreSlot("Gores/DraekArm"));
						Main.gore[gore].numFrames = 3;
						Main.gore[gore].frame = (byte)Main.rand.Next(3);
					}
				}
				if((npc.spriteDirection == 1 && howManyArms > 0) || (npc.spriteDirection == -1 && howManyArms == 2)){
					goreTop.X = npc.Center.X;
					goreTop.X += (npc.TopRight.X - npc.Center.X) / 2f;
					for(int i = 0; i < 4; i++){
						int gore = Gore.NewGore(goreTop + new Vector2(0, 16 * i), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, 5)), mod.GetGoreSlot("Gores/DraekArm"));
						Main.gore[gore].numFrames = 3;
						Main.gore[gore].frame = (byte)Main.rand.Next(3);
					}
				}

				ArmLeft.State = DraekArm.State_Return;
				ArmRight.State = DraekArm.State_Return;
			}
		}

		public override void AI(){
			//AI:  https://docs.google.com/document/d/13IlpNUdO2X_elLPwsYcB1mzd_FMxQkwAcRKq1oSWgLg

			if(!hasSpawned){
				npc.TargetClosest(true);
				
				hasSpawned = true;
				curPattern = 0;
				AI_Attack = pattern[curPattern];
				CurrentPhase = Phase_1;

				//Spawn the arms
				int newNPC = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DraekArm>(), ai0: npc.whoAmI);
				if(newNPC == Main.maxNPCs){
					npc.active = false;
					return;
				}

				Main.npc[newNPC].realLife = npc.whoAmI;
				ArmLeft = Main.npc[newNPC].modNPC as DraekArm;
				ArmLeft.drawAboveBoss = true;
				ArmLeft.npc.ai[3] = npc.target;

				newNPC = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DraekArm>(), ai0: npc.whoAmI);
				if(newNPC == Main.maxNPCs){
					npc.active = false;
					return;
				}

				Main.npc[newNPC].realLife = npc.whoAmI;
				ArmRight = Main.npc[newNPC].modNPC as DraekArm;
				ArmRight.npc.ai[3] = npc.target;

				//Spawn the lasting projectile
				int proj = CosmivengeonUtils.SpawnProjectileSynced(npc.Center, Vector2.Zero, ModContent.ProjectileType<DraekP1ExtraProjectile>(),
					npc.damage, 6f, npc.whoAmI);
				if(proj == Main.maxProjectiles){
					npc.active = false;
					return;
				}

				npc.netUpdate = true;
				
				CosmivengeonUtils.SendMessage("<Draek> So, a new challenger has arisen to take my domain, hm?", TextColour);

				npc.TargetClosest();
			}

			if(npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead){
				npc.TargetClosest(true);
				npc.netUpdate = true;
			}

			//Why is this here?
			if(npc.spriteDirection == 1){
				ArmLeft.drawAboveBoss = true;
				ArmRight.drawAboveBoss = false;
			}else{
				ArmLeft.drawAboveBoss = false;
				ArmRight.drawAboveBoss = true;
			}

			playerTarget = Main.player[npc.target];
			noTargetsAlive = playerTarget.dead || !playerTarget.active;
			CheckTargetIsDead();
			
			if(noTargetsAlive)
				return;

			SummonedWyrms = UpdateWyrmCount();

			if(SummonedWyrms > 0 && Main.expertMode)
				npc.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance += CosmivengeonWorld.desoMode ? 0.5f : 0.25f;

			if(!dashing){
				npc.spriteDirection = (npc.Center.X > Main.player[npc.target].Center.X) ? 1 : -1;
				afterImageLength--;
			}

			AI_Animation_Counter++;

			if(npc.life - HealthThreshold < (npc.lifeMax - HealthThreshold) / 2f && !switchPhases && CurrentPhase == Phase_1){
				switchPhases = true;

				CosmivengeonUtils.SendMessage("<Draek> You're stronger than I expected, aren't you?  No matter.", TextColour);

				AI_Timer = 120;
				delayPhaseChange = true;

				ArmRight.State = DraekArm.State_Return;
				ArmLeft.State = DraekArm.State_Return;

				npc.dontTakeDamage = true;

				if(ForceFastCharge){
					FastChargeEnded = false;
					ForceFastCharge = false;
					FastChargeStarted = false;
					AI_Timer = preFastChargeAI[0];
					AI_Attack = preFastChargeAI[1];
					AI_Attack_Progress = preFastChargeAI[2];
					AI_Animation_Counter = preFastChargeAI[3];
					dashing = preDash;
					dashWait = preDashWait;

					oldOffsetX = syncOffsetX;
					oldOffsetY = syncOffsetY;

					npc.netUpdate = true;
				}
			}

			AI_Check_Phase_Switch();

			afterImageLength.Clamp(0, npc.oldPos.Length * 2);

			if(CosmivengeonWorld.desoMode && ForceFastCharge){
				if(!FastChargeStarted){
					FastChargeStarted = true;
					ForceFastCharge = false;
					FastChargeEnded = false;
					preFastChargeAI[0] = AI_Timer;
					preFastChargeAI[1] = AI_Attack;
					preFastChargeAI[2] = AI_Attack_Progress;
					preFastChargeAI[3] = AI_Animation_Counter;

					syncOffsetX = oldOffsetX;
					syncOffsetY = oldOffsetY;

					preDash = dashing;
					preDashWait = dashWait;

					AI_Timer = 0;
					AI_Attack_Progress = 0;
					AI_Animation_Counter = 0;
					dashing = false;
					dashWait = true;

					oldOffsetX = 0;
					oldOffsetY = 0;

					if(Main.netMode != NetmodeID.MultiplayerClient){
						string text = Main.rand.Next(new string[]{
							"<Draek> Get back here, coward!",
							"<Draek> Where do you think you're going?",
							"<Draek> You won't get away that easily!"
						});

						CosmivengeonUtils.SendMessage(text, TextColour);
					}

					npc.netUpdate = true;
				}

				AI_Dash(16.5f, 1, 60, 0, true);

				if(FastChargeEnded){
					FastChargeEnded = false;
					ForceFastCharge = false;
					FastChargeStarted = false;
					AI_Timer = preFastChargeAI[0];
					AI_Attack = preFastChargeAI[1];
					AI_Attack_Progress = preFastChargeAI[2];
					AI_Animation_Counter = preFastChargeAI[3];
					dashing = preDash;
					dashWait = preDashWait;

					oldOffsetX = syncOffsetX;
					oldOffsetY = syncOffsetY;

					npc.netUpdate = true;
				}
				return;
			}

			if(CurrentPhase == Phase_1){
				if(AI_Attack == Attack_Shoot)
					AI_Hover_Shoot(CosmivengeonUtils.GetModeChoice(60, 42, 28), P1_subphase0_Attacks + CosmivengeonUtils.GetModeChoice(0, 0, 9));
				else if(AI_Attack == Attack_Throw_Sword)
					AI_Hover_Throw_Sword(1, 60);
				else if(AI_Attack == Attack_Shoot_No_Sword){
					float maxXY = CosmivengeonUtils.GetModeChoice(4f, 5.55f, 6.375f);
					AI_Charge_Shoot(CosmivengeonUtils.GetModeChoice(40, 25, 15),
						P1_subphase2_Attacks + CosmivengeonUtils.GetModeChoice(0, 0, 12),
						maxXY, maxXY
					);
				}else if(AI_Attack == Attack_Retrieve_Sword)
					AI_Retrieve_Sword();
				else if(AI_Attack == Attack_Punch)
					AI_Punch();
			}else if(CurrentPhase == Phase_1_Enrage){
				if(AI_Attack == Attack_Shoot)
					AI_Hover_Shoot(CosmivengeonUtils.GetModeChoice(40, 28, 20), P1_Enrage_subphase0_Attacks + CosmivengeonUtils.GetModeChoice(0, 2, 11));
				else if(AI_Attack == Attack_Throw_Sword)
					AI_Hover_Throw_Sword(3, 45);
				else if(AI_Attack == Attack_Shoot_No_Sword){
					float maxXY = CosmivengeonUtils.GetModeChoice(5.85f, 8.325f, 10.75f);
					AI_Charge_Shoot(CosmivengeonUtils.GetModeChoice(20, 16, 10),
						P1_Enrage_subphase2_Attacks + CosmivengeonUtils.GetModeChoice(0, 0, 17),
						maxXY, maxXY
					);
				}else if(AI_Attack == Attack_Retrieve_Sword)
					AI_Retrieve_Sword();
				else if(AI_Attack == Attack_Punch)
					AI_Punch(enraged: true);
			}
		}

		/// <summary>
		/// Gets the size and position of the extra projectile hitbox relative to Draek's animation frame.
		/// </summary>
		public Vector4 GetLastingProjectileHitbox(){
			Vector2 newSize = Vector2.Zero;
			Vector2 offsetPos = Vector2.Zero;

			if(AI_Attack != Attack_Punch){
				switch(AnimationOffset){
					case Idle_Sword_0:
						offsetPos = new Vector2(37, 206);
						newSize = new Vector2(125, 23);
						break;
					case Idle_Sword_1:
						offsetPos = new Vector2(37, 206);
						newSize = new Vector2(125, 23);
						break;
					case Idle_Sword_2:
						offsetPos = new Vector2(37, 209);
						newSize = new Vector2(125, 23);
						break;
					case Idle_Sword_3:
						offsetPos = new Vector2(37, 209);
						newSize = new Vector2(125, 23);
						break;
					case Idle_No_Sword_0:
					case Idle_No_Sword_1:
					case Idle_No_Sword_2:
					case Idle_No_Sword_3:
						break;
					case Throw_Sword_0:
						offsetPos = new Vector2(19, 53);
						newSize = new Vector2(35, 109);
						break;
					case Throw_Sword_1:
						offsetPos = new Vector2(32, 22);
						newSize = new Vector2(25, 102);
						break;
					case Throw_Sword_2:
						offsetPos = new Vector2(14, 48);
						newSize = new Vector2(88, 196);
						break;
					case Throw_Sword_3:
					case Retrieve_Sword_0:
					case Retrieve_Sword_1:
						break;
					case Retrieve_Sword_2:
						offsetPos = new Vector2(34, 45);
						newSize = new Vector2(85, 193);
						break;
					case Retrieve_Sword_3:
						offsetPos = new Vector2(53, 195);
						newSize = new Vector2(119, 31);
						break;
					case Idle_NoArms_0:
					case Idle_NoArms_1:
					case Idle_NoArms_2:
					case Idle_NoArms_3:
						break;
				}
			}

			ExtraHurtboxHelper(ref offsetPos, ref newSize);

			return new Vector4(offsetPos.X, offsetPos.Y, newSize.X, newSize.Y);
		}

		private void ExtraHurtboxHelper(ref Vector2 offset, ref Vector2 size){
			if(npc.spriteDirection == -1)
				offset.X = 186 - offset.X - size.X + 16;

			offset += new Vector2(GraphicsOffsetX, GraphicsOffsetY);

			offset += new Vector2(16, 8);
		}

		//Thanks jopojelly for helping me figure out how to make the Parent-Child link work for the two hooks below:
		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit){
			if(ArmLeft != null && ArmLeft.State != DraekArm.State_Punched)
				ArmRight.npc.immune[player.whoAmI] = player.itemAnimation;
			if(ArmRight != null && ArmRight.State != DraekArm.State_Punched)
				ArmRight.npc.immune[player.whoAmI] = player.itemAnimation;
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit){
			if(ArmLeft != null && ArmLeft.State != DraekArm.State_Punched){
				ArmLeft.npc.immune[projectile.owner] = npc.immune[projectile.owner];

				if(projectile.usesLocalNPCImmunity)
					projectile.localNPCImmunity[ArmLeft.npc.whoAmI] = projectile.localNPCHitCooldown;
			}
			if(ArmRight != null && ArmRight.State != DraekArm.State_Punched){
				ArmRight.npc.immune[projectile.owner] = npc.immune[projectile.owner];

				if(projectile.usesLocalNPCImmunity)
					projectile.localNPCImmunity[ArmLeft.npc.whoAmI] = projectile.localNPCHitCooldown;
			}
		}

		private static readonly int[] pattern = new int[]{
			Attack_Shoot,
			Attack_Throw_Sword,
			Attack_Shoot_No_Sword,
			Attack_Punch,
			Attack_Shoot_No_Sword,
			Attack_Retrieve_Sword
		};

		private int curPattern = 0;

		private void AI_Check_Phase_Switch(){
			if(delayPhaseChange){
				if(AI_Timer == 0){
					delayPhaseChange = false;
					npc.dontTakeDamage = false;
					npc.velocity = Vector2.Zero;

					npc.TargetClosest();
				}else{
					AI_Attack = Attack_Idle;
					npc.dontTakeDamage = true;
					npc.velocity *= 0.91f;
					dashing = false;
					dashWait = false;
					AI_Timer--;
					return;
				}
			}
			
			if(switchPhases){
				switchPhases = false;
				dashing = false;
				dashWait = true;
				npc.dontTakeDamage = false;
				throwingSword = false;
				AI_Animation_Counter = 0;
				AI_Timer = 0;
				AI_Attack_Progress = 0;
				AI_Attack = Attack_Shoot;
				CurrentPhase++;

				syncOffsetX = 0;
				syncOffsetY = 0;

				npc.netUpdate = true;

				npc.TargetClosest();
			}

			if(switchSubPhases){
				switchSubPhases = false;
				dashing = false;
				dashWait = true;
				npc.dontTakeDamage = false;
				AI_Animation_Counter = 0;
				AI_Timer = 0;
				AI_Attack_Progress = 0;

				syncOffsetX = 0;
				syncOffsetY = 0;

				//Move to the next subphase
				curPattern++;
				if(curPattern >= pattern.Length)  //Repeat P1 subphases
					curPattern = 0;
					
				AI_Attack = pattern[curPattern];
				npc.netUpdate = true;
				PunchProgress = -1;
			}

			if(!CosmivengeonWorld.desoMode || ForceFastCharge || AI_Attack == Attack_Retrieve_Sword || delayPhaseChange || AI_Attack == Attack_Punch)
				return;

			float maxDistance = 70 * 16;
			if((AI_Attack == Attack_Shoot || (AI_Attack == Attack_Throw_Sword && !throwingSword)) && npc.Distance(playerTarget.Center) > maxDistance){
				ForceFastCharge = true;
				FastCharge_SwordActive = true;

				npc.netUpdate = true;
			}else if(AI_Attack == Attack_Shoot_No_Sword && npc.Distance(playerTarget.Center) > maxDistance){
				ForceFastCharge = true;
				FastCharge_SwordActive = false;

				npc.netUpdate = true;
			}
		}

		private void CheckTargetIsDead(){
			//If the target is dead or not active, slow down the NPC
			//Then, plummet to hell and despawn naturally
			if(noTargetsAlive){
				if(Math.Abs(npc.velocity.X) > 0)
					npc.velocity.X *= 0.5f;
				if(Math.Abs(npc.velocity.Y) > 0)
					npc.velocity.Y *= 0.5f;
				
				if(npc.velocity.Length() < 1 && npc.velocity != Vector2.Zero){
					npc.velocity = Vector2.Zero;
				}

				if(npc.velocity.X == 0){
					npc.velocity.Y += 15f;
				}

				if(!startDespawn){
					startDespawn = true;
					npc.timeLeft = (int)(0.5f * 60);

					npc.netUpdate = true;
				}
			}
		}

		private void AI_Hover(Player target, int facingDirection, out Vector2 npcTarget){
			npc.velocity *= 0.86f;

			//Generate a Vector2 point about 12 blocks up, 20 blocks left/right from the player
			//X will be negative if the player is facing left

			npcTarget = target.Center;		//Get the player's coordinates
			npcTarget.X += 20 * 16 * facingDirection;		//Add the offset
			npcTarget.Y += -12 * 16;

			if(Vector2.Distance(npcTarget, npc.Center) > 16)	//If the boss isn't near the target point
				npc.velocity += Vector2.Normalize(npcTarget - npc.Center) * 0.8f;
			else
				npc.velocity = Vector2.Zero;
			
			float clampVal = CosmivengeonUtils.GetModeChoice(8f, 10f, 13f);
			npc.velocity.X.Clamp(-clampVal, clampVal);
			npc.velocity.Y.Clamp(-clampVal, clampVal);
		}

		private void AI_Shoot_Laser(int delay, int times){
			AI_Timer++;

			Vector2 positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 4f * 16;

			int npcDamage = npc.damage;
			npc.damage = 0;

			int laserDamage = 20 + CosmivengeonUtils.GetModeChoice(0, 20, 30);

			if(AI_Timer % delay == 0 && Main.netMode != NetmodeID.MultiplayerClient){
				CosmivengeonUtils.SpawnProjectileSynced(npc.Bottom - new Vector2(0, 0.667f * npc.height),
					Vector2.Zero,
					ModContent.ProjectileType<DraekLaser>(),
					laserDamage,
					6f,
					playerTarget.Center.X + positionOffset.X,
					playerTarget.Center.Y + positionOffset.Y
				);
				AI_Attack_Progress++;
				//Play "boss laser" sound effect
				Main.PlaySound(SoundID.Item33, npc.Bottom - new Vector2(0, 0.667f * npc.height));
			}

			npc.damage = npcDamage;

			if(AI_Attack_Progress >= times)
				switchSubPhases = true;
		}

		private void AI_Hover_Shoot(int delay, int times){
			AI_Hover(playerTarget, playerTarget.direction, out _);
					
			AI_Shoot_Laser(delay, times);
		}

		private void AI_Hover_Throw_Sword(int times, int waitDuration){
			AI_Hover(playerTarget, playerTarget.direction, out Vector2 npcTarget);
					
			//If the sword was thrown, wait until the animation has played
			if(throwingSword){
				if(AI_Timer == 40){
					AI_Animation_Counter = 0;
					oldOffsetX = 0;
					oldOffsetY = 0;

					AI_Timer = 30;
				}
						
				AI_Timer--;
				if(AI_Timer == 30 - 22){
					//Throw the sword on the next-to-last frame
					Vector2 dir = (npc.spriteDirection == 1) ? npc.BottomRight : npc.BottomLeft;

					int swordDamage = 35 + CosmivengeonUtils.GetModeChoice(0, 20, 55);

					 CosmivengeonUtils.SpawnProjectileSynced(dir,
						Vector2.Zero,
						ModContent.ProjectileType<DraekSword>(),
						swordDamage,
						12f,
						npc.target,
						npc.whoAmI
					);

					//Play sword swing sound effect
					Main.PlaySound(SoundID.Item1, dir);
				}

				if(AI_Timer == 0){
					throwingSword = false;

					if(AI_Attack_Progress == times){
						switchSubPhases = true;

						//Summon wyrms if he's thrown all swords and we're not in Normal Mode
						if(CosmivengeonWorld.desoMode)
							AI_Summon(6);
						else if(Main.expertMode)
							AI_Summon(2);
					}
				}
			}
			if(!throwingSword){
				if(Vector2.Distance(npcTarget, npc.Center) < 100 * 16)
					AI_Timer++;
				else
					AI_Timer = 0;
						
				//If Draek has been within the 100-tile radius for sword throw for X seconds
				if(AI_Timer >= waitDuration && AI_Animation_Counter % 30 == 0){
					//Set the timer for sword throw
					AI_Timer = 40;
					AI_Attack_Progress++;
					throwingSword = true;
				}
			}
		}

		private void AI_Charge_Shoot(int delay, int times, float maxXvel, float maxYvel){
			npc.velocity *= 0.86f;

			if(Vector2.Distance(playerTarget.Center, npc.Center) > 16)		//If the boss isn't near the target
				npc.velocity += Vector2.Normalize(playerTarget.Center - npc.Center) * 0.7f;
			else
				npc.velocity = Vector2.Zero;
					
			AI_Shoot_Laser(delay, times);

			npc.velocity.X.Clamp(-maxXvel, maxXvel);
			npc.velocity.Y.Clamp(-maxYvel, maxYvel);
		}

		private void AI_Dash(float velocity, int times, int duration, int wait, bool fastCharge = false){
			//Dash happens during the punch attack only

			if(dashWait){
				//Delay dash for 30 ticks
				AI_Timer = wait;
				dashWait = false;
			}else if(!dashWait && (AI_Attack_Progress % 2 == 0)){
				//Keep following the player until the wait duration has passed
				if(AI_Attack_Progress == 0){
					npc.velocity += Vector2.Normalize(playerTarget.Center - npc.Center) * 0.7f;
					npc.velocity *= 0.52f;
				}else if(AI_Attack_Progress / 2f < times){
					if(Vector2.Distance(playerTarget.Center, npc.Center) > 16)		//If the boss isn't near the target
						npc.velocity += Vector2.Normalize(playerTarget.Center - npc.Center) * 0.7f;
					else
						npc.velocity = Vector2.Zero;
				}

				AI_Timer--;
				if(AI_Timer <= 0){
					AI_Attack_Progress++;
					AI_Timer = 0;
				}
			}else if(AI_Attack_Progress % 2 == 1){
				if(dashing){
					AI_Timer--;

					afterImageLength++;
							
					if(AI_Timer == 0){
						dashing = false;
						dashWait = true;
						AI_Attack_Progress++;
					}
				}else if(!dashing){
					if(AI_Timer == 0){
						//Start dashing if we haven't already
						dashing = true;
						AI_Timer = duration;

						Vector2 newVel = npc.DirectionTo(playerTarget.Center) * velocity;

						if(fastCharge)
							newVel = newVel.RotatedByRandom(MathHelper.ToRadians(30));

						npc.velocity = newVel;

						Main.PlaySound(SoundID.ForceRoar, npc.Center, fastCharge ? -1 : 0);

						afterImageLength++;
					}
				}
			}

			if(AI_Attack_Progress == 2 * times){
				ForceFastCharge = false;
				FastChargeEnded = true;
			}
			
			if(!dashing){
				npc.velocity.X.Clamp(-6, 6);
				npc.velocity.Y.Clamp(-8, 8);
			}
		}

		private void AI_Retrieve_Sword(){
			npc.velocity *= 0.7f;
			
			npc.dontTakeDamage = true;

			AI_Timer++;

			if(AI_Timer >= 30)
				switchSubPhases = true;
		}

		private void AI_Summon(int times){
			while(SummonedWyrms < times && Main.netMode != NetmodeID.MultiplayerClient){
				Vector2 range = npc.Center + new Vector2(Main.rand.NextFloat(-8 * 16, 8 * 16), Main.rand.NextFloat(-8 * 16, 8 * 16));
			
				NPC.NewNPC((int)range.X, (int)range.Y, ModContent.NPCType<Summon>(), ai2: npc.whoAmI);
			
				SummonedWyrms++;

				npc.netUpdate = true;
			}
		}

		private int UpdateWyrmCount(){
			int wyrms = 0;
			
			for(int i = 0; i < Main.npc.Length; i++){
				if(Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<Summon>() && (int)Main.npc[i].ai[2] == npc.whoAmI)
					wyrms++;
			}
			
			return wyrms;
		}

		public ref float PunchProgress => ref npc.localAI[1];

		private void AI_Punch(bool enraged = false){
			/*  Punch attack:
			 *  
			 *  - punch left arm
			 *  - punch right arm
			 *  - Wait 3/1.5/0.5 seconds, then charge at the player 2/3/3 times
			 *  - retrieve arms
			 */
			int dashCount = !enraged ? CosmivengeonUtils.GetModeChoice(2, 3, 3) : CosmivengeonUtils.GetModeChoice(3, 4, 4);
			float dashVelocity = !enraged ? CosmivengeonUtils.GetModeChoice(6f, 8f, 9f) : CosmivengeonUtils.GetModeChoice(7f, 9f, 10f);
			int dashDuration = !enraged ? 75 : 60;
			int dashWait = (int)(CosmivengeonUtils.GetModeChoice(3, 1.5f, 0.5f) * 60);

			void PunchArm(DraekArm arm){
				Vector2 spawnCenter = npc.Bottom - new Vector2(0, 0.667f * npc.height);
				spawnCenter += new Vector2(-8 * 16 * npc.spriteDirection, 20);

				arm.State = DraekArm.State_Punched;
				arm.npc.ai[3] = npc.target;
				arm.Timer = 0;
				arm.ShootTimer = 0;

				arm.npc.Center = spawnCenter;
				arm.npc.velocity = new Vector2(-20f * npc.spriteDirection, 0).RotatedByRandom(MathHelper.ToRadians(15f));

				Main.PlaySound(SoundID.Item1, spawnCenter);
			}

			if(PunchProgress == -1)
				PunchProgress = Punch_PunchFirst;

			if(PunchProgress == Punch_PunchFirst){
				if((int)(AI_Animation_Counter / 7.5f) == 6){
					//Un-hide the first arm
					if(npc.spriteDirection == 1){
						PunchArm(ArmRight);
					}else
						PunchArm(ArmLeft);

					howManyArms = 1;
				}

				if(AI_Animation_Counter == 60){
					PunchProgress = Punch_PunchSecond;

					//First frame of the other punch
					AI_Animation_Counter = (int)(4 * 7.5f);
				}

				AI_Hover(playerTarget, playerTarget.direction, out _);
			}else if(PunchProgress == Punch_PunchSecond){
				if((int)(AI_Animation_Counter / 7.5f) == 6){
					//Un-hide the second arm
					if(ArmRight.State != DraekArm.State_Punched)
						PunchArm(ArmRight);
					else
						PunchArm(ArmLeft);

					howManyArms = 0;
				}

				if(AI_Animation_Counter == 60)
					PunchProgress = Punch_Wait;

				AI_Hover(playerTarget, playerTarget.direction, out _);
			}else if(PunchProgress == Punch_Wait){
				AI_Dash(dashVelocity, dashCount, dashDuration, dashWait, fastCharge: false);

				if(AI_Attack_Progress == 2 * dashCount){
					switchSubPhases = true;

					ArmRight.State = DraekArm.State_Return;
					ArmLeft.State = DraekArm.State_Return;

					howManyArms = 2;

					Vector2 goreTop = npc.Center;
					goreTop.X += (npc.TopLeft.X - npc.Center.X) / 2f;
					goreTop.Y += 20f;
					for(int i = 0; i < 4; i++){
						for(int d = 0; d < 20; d++){
							Dust dust = Dust.NewDustDirect(goreTop + new Vector2(0, 16 * i), 0, 0, 82);
							dust.noGravity = true;
							dust.velocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 5.5f;
						}
					}
					goreTop.X = npc.Center.X;
					goreTop.X += (npc.TopRight.X - npc.Center.X) / 2f;
					for(int i = 0; i < 4; i++){
						for(int d = 0; d < 20; d++){
							Dust dust = Dust.NewDustDirect(goreTop + new Vector2(0, 16 * i), 0, 0, 82);
							dust.noGravity = true;
							dust.velocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 5.5f;
						}
					}
				}
			}
		}
	}
}