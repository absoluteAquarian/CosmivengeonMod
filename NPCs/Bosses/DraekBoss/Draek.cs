using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Enums;
using CosmivengeonMod.NPCs.Bosses.DraekBoss.Summons;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Utility.Extensions;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Bosses.DraekBoss{
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
				FindFrame(TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type]);
				return animationOffset;
			}
		}

		private DraekArm ArmLeft;
		private DraekArm ArmRight;
		
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.TrailCacheLength[NPC.type] = 5;
			NPCID.Sets.TrailingMode[NPC.type] = 0;
		}

		public int HealthThreshold;

		public static readonly int BaseHealth = 2500;

		public static float GetHealthAugmentation() => MiscUtils.GetModeChoice(0, 0.65f, 0.8f);

		public override void SetDefaults(){
			NPC.scale = 1f;
			//Draek frame dimentions:  186x290px
			NPC.width = 74;
			NPC.height = 233;
			NPC.aiStyle = -1;	//-1 means that this enemy has a unique AI; don't copy an existing style
			NPC.damage = 45;
			NPC.defense = 18;
			NPC.lifeMax = BaseHealth;
			NPC.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			NPC.noGravity = true;
			NPC.knockBackResist = 0f;	//100% kb resist
			NPC.npcSlots = 30f;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noTileCollide = true;

			NPC.value = Value;

			NPC.buffImmune[BuffID.Poisoned] = true;
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.Burning] = true;
			NPC.buffImmune[BuffID.Frostburn] = true;

			MiscUtils.PlayMusic(this, CosmivengeonBoss.Draek);

			//Multiplayer is cringe!
			RealFrame.Width = 1;
			RealFrame.Height = 1;
			if(!Main.dedServ){
				RealTexture = ModContent.GetTexture("CosmivengeonMod/NPCs/Bosses/DraekBoss/Draek_Animations");

				RealFrame.Width = RealTexture.Frame(4, 5).Width;
				RealFrame.Height = RealTexture.Frame(4, 5).Height;
			}
		}

		public override bool PreKill(){
			return false;	//First phase shouldn't drop anything
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			//Health increase handled in DetourNPC

			if(!WorldEvents.desoMode){
				NPC.damage = 55;
				NPC.defense = 22;
			}else{
				NPC.damage = 80;
				NPC.defense = 26;
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
		public ref float AI_Timer => ref NPC.ai[AI_Timer_Slot];
		public ref float AI_Attack => ref NPC.ai[AI_Attack_Slot];
		public ref float AI_Attack_Progress => ref NPC.ai[AI_Attack_Progress_Slot];
		public ref float AI_Animation_Counter => ref NPC.ai[AI_Animation_Counter_Slot];

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
					texture += NPC.spriteDirection == -1 ? "Right" : "Left";
					if(PunchProgress == Punch_PrimeFirst || PunchProgress == Punch_PunchFirst){
						//Priming/punching first punch
						texture += "_Both";

						offsetY = (int)(AI_Animation_Counter / 7.5f);
					}else if(PunchProgress == Punch_PrimeSecond || PunchProgress == Punch_PunchSecond){
						offsetY = (int)(AI_Animation_Counter / 7.5f);
					}

					if(PunchProgress != Punch_Wait && PunchProgress != Punch_RetriveFirst && PunchProgress != Punch_RetrieveSecond){
						RealTexture = Mod.GetTexture($"NPCs/Draek/{texture}");
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
				RealTexture = Mod.GetTexture($"NPCs/Draek/Draek_Animations");
				RealFrame = RealTexture.Frame(4, 5);

				GraphicsOffsetX = -49;
				GraphicsOffsetY = -52;

				if(NPC.spriteDirection == 1)
					GraphicsOffsetX = -65;

				offsetY = offsetX / 4;
				offsetX %= 4;

				animationOffset = offsetY * 4 + offsetX;
			}else{
				GraphicsOffsetX = -131;
				GraphicsOffsetY = -6;

				if(NPC.spriteDirection == -1)
					GraphicsOffsetX = -55;

				animationOffset = offsetX;
			}
			
			RealFrame.X = offsetX * RealFrame.Width;
			RealFrame.Y = offsetY * RealFrame.Height;

			oldOffsetX = offsetX;
			oldOffsetY = offsetY;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			if(WorldEvents.desoMode)
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 150);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor){
			//Only apply afterimage effects while dashing
			if(afterImageLength > 1){
				//Afterimage effect
				Vector2 drawOrigin = RealFrame.Size() / 2f;
				SpriteEffects effect = (NPC.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				for (int k = 0; k < afterImageLength / 2; k++){
					Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(GraphicsOffsetX, GraphicsOffsetY);
					
					Color color = NPC.GetAlpha(lightColor) * (((float)NPC.oldPos.Length - k) / NPC.oldPos.Length);
					color.A = (byte)(0.75f * 255f * (NPC.oldPos.Length - k) / NPC.oldPos.Length);	//Apply transparency

					spriteBatch.Draw(RealTexture, drawPos, RealFrame, color, NPC.rotation, drawOrigin, NPC.scale, effect, 0f);
				}
			}
			return false;	//Prevent sprite from being drawn normally
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor){
			//We're going to manually draw Draek to better fit his hitbox size
			SpriteEffects effect = (NPC.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 drawOrigin = RealFrame.Size() / 2f;
			Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(GraphicsOffsetX, GraphicsOffsetY);

			spriteBatch.Draw(RealTexture, drawPos, RealFrame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effect, 0f);

#pragma warning disable CS0162
			if(ShowDebug){
				Vector2 pos = NPC.Top + new Vector2(-NPC.width - 50, -70) - Main.screenPosition;
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value,
					$"AI: [ {(int)NPC.ai[0],3}, {(int)NPC.ai[1]}, {(int)NPC.ai[2],2}, {(int)NPC.ai[3],3} ]",
					pos.X,
					pos.Y,
					Color.White,
					Color.Black,
					Vector2.Zero);
				pos.Y += 18;
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value,
					$"Animation: [ {RealFrame.X / RealFrame.Width}, {RealFrame.Y / RealFrame.Height} ]",
					pos.X,
					pos.Y,
					Color.White,
					Color.Black,
					Vector2.Zero);
				pos.Y += 18;
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value,
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
			return Vector2.Distance(NPC.Center, playerTarget.Center) > 200 * 16;
		}

		public override void HitEffect(int hitDirection, double damage){
			if(NPC.life - damage < HealthThreshold){
				NPC.life = 0;

				int newNPC = NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DraekP2Head>());
				Main.npc[newNPC].ModNPC.Music = Music;
				Main.npc[newNPC].ModNPC.SceneEffectPriority = SceneEffectPriority;

				//Spawn 8 gores, 4 per arm
				Vector2 goreTop = NPC.Center;
				goreTop.X += (NPC.TopLeft.X - NPC.Center.X) / 2f;
				goreTop.Y += 20f;
				if((NPC.spriteDirection == 1 && howManyArms == 2) || (NPC.spriteDirection == -1 && howManyArms > 0)){
					for(int i = 0; i < 4; i++){
						int gore = Gore.NewGore(goreTop + new Vector2(0, 16 * i), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, 5)), Mod.GetGoreSlot("Gores/DraekArm"));
						Main.gore[gore].numFrames = 3;
						Main.gore[gore].frame = (byte)Main.rand.Next(3);
					}
				}
				if((NPC.spriteDirection == 1 && howManyArms > 0) || (NPC.spriteDirection == -1 && howManyArms == 2)){
					goreTop.X = NPC.Center.X;
					goreTop.X += (NPC.TopRight.X - NPC.Center.X) / 2f;
					for(int i = 0; i < 4; i++){
						int gore = Gore.NewGore(goreTop + new Vector2(0, 16 * i), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, 5)), Mod.GetGoreSlot("Gores/DraekArm"));
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
				NPC.TargetClosest(true);
				
				hasSpawned = true;
				curPattern = 0;
				AI_Attack = pattern[curPattern];
				CurrentPhase = Phase_1;

				//Spawn the arms
				int newNPC = NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DraekArm>(), ai0: NPC.whoAmI);
				if(newNPC == Main.maxNPCs){
					NPC.active = false;
					return;
				}

				Main.npc[newNPC].realLife = NPC.whoAmI;
				ArmLeft = Main.npc[newNPC].ModNPC as DraekArm;
				ArmLeft.drawAboveBoss = true;
				ArmLeft.NPC.ai[3] = NPC.target;

				newNPC = NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DraekArm>(), ai0: NPC.whoAmI);
				if(newNPC == Main.maxNPCs){
					NPC.active = false;
					return;
				}

				Main.npc[newNPC].realLife = NPC.whoAmI;
				ArmRight = Main.npc[newNPC].ModNPC as DraekArm;
				ArmRight.NPC.ai[3] = NPC.target;

				//Spawn the lasting projectile
				int proj = MiscUtils.SpawnProjectileSynced(NPC.Center, Vector2.Zero, ModContent.ProjectileType<DraekP1ExtraProjectile>(),
					NPC.damage, 6f, NPC.whoAmI);
				if(proj == Main.maxProjectiles){
					NPC.active = false;
					return;
				}

				NPC.netUpdate = true;
				
				MiscUtils.SendMessage("<Draek> So, a new challenger has arisen to take my domain, hm?", TextColour);

				NPC.TargetClosest();
			}

			if(NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead){
				NPC.TargetClosest(true);
				NPC.netUpdate = true;
			}

			//Why is this here?
			if(NPC.spriteDirection == 1){
				ArmLeft.drawAboveBoss = true;
				ArmRight.drawAboveBoss = false;
			}else{
				ArmLeft.drawAboveBoss = false;
				ArmRight.drawAboveBoss = true;
			}

			playerTarget = Main.player[NPC.target];
			noTargetsAlive = playerTarget.dead || !playerTarget.active;
			CheckTargetIsDead();
			
			if(noTargetsAlive)
				return;

			SummonedWyrms = UpdateWyrmCount();

			if(SummonedWyrms > 0 && Main.expertMode)
				NPC.GetGlobalNPC<StatsNPC>().endurance += WorldEvents.desoMode ? 0.5f : 0.25f;

			if(!dashing){
				NPC.spriteDirection = (NPC.Center.X > Main.player[NPC.target].Center.X) ? 1 : -1;
				afterImageLength--;
			}

			AI_Animation_Counter++;

			if(NPC.life - HealthThreshold < (NPC.lifeMax - HealthThreshold) / 2f && !switchPhases && CurrentPhase == Phase_1){
				switchPhases = true;

				MiscUtils.SendMessage("<Draek> You're stronger than I expected, aren't you?  No matter.", TextColour);

				AI_Timer = 120;
				delayPhaseChange = true;

				ArmRight.State = DraekArm.State_Return;
				ArmLeft.State = DraekArm.State_Return;

				NPC.dontTakeDamage = true;

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

					NPC.netUpdate = true;
				}
			}

			AI_Check_Phase_Switch();

			afterImageLength.Clamp(0, NPC.oldPos.Length * 2);

			if(WorldEvents.desoMode && ForceFastCharge){
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

						MiscUtils.SendMessage(text, TextColour);
					}

					NPC.netUpdate = true;
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

					NPC.netUpdate = true;
				}
				return;
			}

			if(CurrentPhase == Phase_1){
				if(AI_Attack == Attack_Shoot)
					AI_Hover_Shoot(MiscUtils.GetModeChoice(60, 42, 28), P1_subphase0_Attacks + MiscUtils.GetModeChoice(0, 0, 9));
				else if(AI_Attack == Attack_Throw_Sword)
					AI_Hover_Throw_Sword(1, 60);
				else if(AI_Attack == Attack_Shoot_No_Sword){
					float maxXY = MiscUtils.GetModeChoice(4f, 5.55f, 6.375f);
					AI_Charge_Shoot(MiscUtils.GetModeChoice(40, 25, 15),
						P1_subphase2_Attacks + MiscUtils.GetModeChoice(0, 0, 12),
						maxXY, maxXY
					);
				}else if(AI_Attack == Attack_Retrieve_Sword)
					AI_Retrieve_Sword();
				else if(AI_Attack == Attack_Punch)
					AI_Punch();
			}else if(CurrentPhase == Phase_1_Enrage){
				if(AI_Attack == Attack_Shoot)
					AI_Hover_Shoot(MiscUtils.GetModeChoice(40, 28, 20), P1_Enrage_subphase0_Attacks + MiscUtils.GetModeChoice(0, 2, 11));
				else if(AI_Attack == Attack_Throw_Sword)
					AI_Hover_Throw_Sword(3, 45);
				else if(AI_Attack == Attack_Shoot_No_Sword){
					float maxXY = MiscUtils.GetModeChoice(5.85f, 8.325f, 10.75f);
					AI_Charge_Shoot(MiscUtils.GetModeChoice(20, 16, 10),
						P1_Enrage_subphase2_Attacks + MiscUtils.GetModeChoice(0, 0, 17),
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
			if(NPC.spriteDirection == -1)
				offset.X = 186 - offset.X - size.X + 16;

			offset += new Vector2(GraphicsOffsetX, GraphicsOffsetY);

			offset += new Vector2(16, 8);
		}

		//Thanks jopojelly for helping me figure out how to make the Parent-Child link work for the two hooks below:
		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit){
			if(ArmLeft != null && ArmLeft.State != DraekArm.State_Punched)
				ArmRight.NPC.immune[player.whoAmI] = player.itemAnimation;
			if(ArmRight != null && ArmRight.State != DraekArm.State_Punched)
				ArmRight.NPC.immune[player.whoAmI] = player.itemAnimation;
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit){
			if(ArmLeft != null && ArmLeft.State != DraekArm.State_Punched){
				ArmLeft.NPC.immune[projectile.owner] = NPC.immune[projectile.owner];

				if(projectile.usesLocalNPCImmunity)
					projectile.localNPCImmunity[ArmLeft.NPC.whoAmI] = projectile.localNPCHitCooldown;
			}
			if(ArmRight != null && ArmRight.State != DraekArm.State_Punched){
				ArmRight.NPC.immune[projectile.owner] = NPC.immune[projectile.owner];

				if(projectile.usesLocalNPCImmunity)
					projectile.localNPCImmunity[ArmLeft.NPC.whoAmI] = projectile.localNPCHitCooldown;
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
					NPC.dontTakeDamage = false;
					NPC.velocity = Vector2.Zero;

					NPC.TargetClosest();
				}else{
					AI_Attack = Attack_Idle;
					NPC.dontTakeDamage = true;
					NPC.velocity *= 0.91f;
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
				NPC.dontTakeDamage = false;
				throwingSword = false;
				AI_Animation_Counter = 0;
				AI_Timer = 0;
				AI_Attack_Progress = 0;
				AI_Attack = Attack_Shoot;
				CurrentPhase++;

				syncOffsetX = 0;
				syncOffsetY = 0;

				NPC.netUpdate = true;

				NPC.TargetClosest();
			}

			if(switchSubPhases){
				switchSubPhases = false;
				dashing = false;
				dashWait = true;
				NPC.dontTakeDamage = false;
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
				NPC.netUpdate = true;
				PunchProgress = -1;
			}

			if(!WorldEvents.desoMode || ForceFastCharge || AI_Attack == Attack_Retrieve_Sword || delayPhaseChange || AI_Attack == Attack_Punch)
				return;

			float maxDistance = 70 * 16;
			if((AI_Attack == Attack_Shoot || (AI_Attack == Attack_Throw_Sword && !throwingSword)) && NPC.Distance(playerTarget.Center) > maxDistance){
				ForceFastCharge = true;
				FastCharge_SwordActive = true;

				NPC.netUpdate = true;
			}else if(AI_Attack == Attack_Shoot_No_Sword && NPC.Distance(playerTarget.Center) > maxDistance){
				ForceFastCharge = true;
				FastCharge_SwordActive = false;

				NPC.netUpdate = true;
			}
		}

		private void CheckTargetIsDead(){
			//If the target is dead or not active, slow down the NPC
			//Then, plummet to hell and despawn naturally
			if(noTargetsAlive){
				if(Math.Abs(NPC.velocity.X) > 0)
					NPC.velocity.X *= 0.5f;
				if(Math.Abs(NPC.velocity.Y) > 0)
					NPC.velocity.Y *= 0.5f;
				
				if(NPC.velocity.Length() < 1 && NPC.velocity != Vector2.Zero){
					NPC.velocity = Vector2.Zero;
				}

				if(NPC.velocity.X == 0){
					NPC.velocity.Y += 15f;
				}

				if(!startDespawn){
					startDespawn = true;
					NPC.timeLeft = (int)(0.5f * 60);

					NPC.netUpdate = true;
				}
			}
		}

		private void AI_Hover(Player target, int facingDirection, out Vector2 npcTarget){
			NPC.velocity *= 0.86f;

			//Generate a Vector2 point about 12 blocks up, 20 blocks left/right from the player
			//X will be negative if the player is facing left

			npcTarget = target.Center;		//Get the player's coordinates
			npcTarget.X += 20 * 16 * facingDirection;		//Add the offset
			npcTarget.Y += -12 * 16;

			if(Vector2.Distance(npcTarget, NPC.Center) > 16)	//If the boss isn't near the target point
				NPC.velocity += Vector2.Normalize(npcTarget - NPC.Center) * 0.8f;
			else
				NPC.velocity = Vector2.Zero;
			
			float clampVal = MiscUtils.GetModeChoice(8f, 10f, 13f);
			NPC.velocity.X.Clamp(-clampVal, clampVal);
			NPC.velocity.Y.Clamp(-clampVal, clampVal);
		}

		private void AI_Shoot_Laser(int delay, int times){
			AI_Timer++;

			Vector2 positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 4f * 16;

			int npcDamage = NPC.damage;
			NPC.damage = 0;

			int laserDamage = 20 + MiscUtils.GetModeChoice(0, 20, 30);

			if(AI_Timer % delay == 0 && Main.netMode != NetmodeID.MultiplayerClient){
				MiscUtils.SpawnProjectileSynced(NPC.Bottom - new Vector2(0, 0.667f * NPC.height),
					Vector2.Zero,
					ModContent.ProjectileType<DraekLaser>(),
					laserDamage,
					6f,
					playerTarget.Center.X + positionOffset.X,
					playerTarget.Center.Y + positionOffset.Y
				);
				AI_Attack_Progress++;
				//Play "boss laser" sound effect
				SoundEngine.PlaySound(SoundID.Item33, NPC.Bottom - new Vector2(0, 0.667f * NPC.height));
			}

			NPC.damage = npcDamage;

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
					Vector2 dir = (NPC.spriteDirection == 1) ? NPC.BottomRight : NPC.BottomLeft;

					int swordDamage = 35 + MiscUtils.GetModeChoice(0, 20, 55);

					 MiscUtils.SpawnProjectileSynced(dir,
						Vector2.Zero,
						ModContent.ProjectileType<DraekSword>(),
						swordDamage,
						12f,
						NPC.target,
						NPC.whoAmI
					);

					//Play sword swing sound effect
					SoundEngine.PlaySound(SoundID.Item1, dir);
				}

				if(AI_Timer == 0){
					throwingSword = false;

					if(AI_Attack_Progress == times){
						switchSubPhases = true;

						//Summon wyrms if he's thrown all swords and we're not in Normal Mode
						if(WorldEvents.desoMode)
							AI_Summon(6);
						else if(Main.expertMode)
							AI_Summon(2);
					}
				}
			}
			if(!throwingSword){
				if(Vector2.Distance(npcTarget, NPC.Center) < 100 * 16)
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
			NPC.velocity *= 0.86f;

			if(Vector2.Distance(playerTarget.Center, NPC.Center) > 16)		//If the boss isn't near the target
				NPC.velocity += Vector2.Normalize(playerTarget.Center - NPC.Center) * 0.7f;
			else
				NPC.velocity = Vector2.Zero;
					
			AI_Shoot_Laser(delay, times);

			NPC.velocity.X.Clamp(-maxXvel, maxXvel);
			NPC.velocity.Y.Clamp(-maxYvel, maxYvel);
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
					NPC.velocity += Vector2.Normalize(playerTarget.Center - NPC.Center) * 0.7f;
					NPC.velocity *= 0.52f;
				}else if(AI_Attack_Progress / 2f < times){
					if(Vector2.Distance(playerTarget.Center, NPC.Center) > 16)		//If the boss isn't near the target
						NPC.velocity += Vector2.Normalize(playerTarget.Center - NPC.Center) * 0.7f;
					else
						NPC.velocity = Vector2.Zero;
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

						Vector2 newVel = NPC.DirectionTo(playerTarget.Center) * velocity;

						if(fastCharge)
							newVel = newVel.RotatedByRandom(MathHelper.ToRadians(30));

						NPC.velocity = newVel;

						SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center, fastCharge ? -1 : 0);

						afterImageLength++;
					}
				}
			}

			if(AI_Attack_Progress == 2 * times){
				ForceFastCharge = false;
				FastChargeEnded = true;
			}
			
			if(!dashing){
				NPC.velocity.X.Clamp(-6, 6);
				NPC.velocity.Y.Clamp(-8, 8);
			}
		}

		private void AI_Retrieve_Sword(){
			NPC.velocity *= 0.7f;
			
			NPC.dontTakeDamage = true;

			AI_Timer++;

			if(AI_Timer >= 30)
				switchSubPhases = true;
		}

		private void AI_Summon(int times){
			while(SummonedWyrms < times && Main.netMode != NetmodeID.MultiplayerClient){
				Vector2 range = NPC.Center + new Vector2(Main.rand.NextFloat(-8 * 16, 8 * 16), Main.rand.NextFloat(-8 * 16, 8 * 16));
			
				NPC.NewNPC((int)range.X, (int)range.Y, ModContent.NPCType<DraekWyrmSummon_Head>(), ai2: NPC.whoAmI);
			
				SummonedWyrms++;

				NPC.netUpdate = true;
			}
		}

		private int UpdateWyrmCount(){
			int wyrms = 0;
			
			for(int i = 0; i < Main.npc.Length; i++){
				if(Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<DraekWyrmSummon_Head>() && (int)Main.npc[i].ai[2] == NPC.whoAmI)
					wyrms++;
			}
			
			return wyrms;
		}

		public ref float PunchProgress => ref NPC.localAI[1];

		private void AI_Punch(bool enraged = false){
			/*  Punch attack:
			 *  
			 *  - punch left arm
			 *  - punch right arm
			 *  - Wait 3/1.5/0.5 seconds, then charge at the player 2/3/3 times
			 *  - retrieve arms
			 */
			int dashCount = !enraged ? MiscUtils.GetModeChoice(2, 3, 3) : MiscUtils.GetModeChoice(3, 4, 4);
			float dashVelocity = !enraged ? MiscUtils.GetModeChoice(6f, 8f, 9f) : MiscUtils.GetModeChoice(7f, 9f, 10f);
			int dashDuration = !enraged ? 75 : 60;
			int dashWait = (int)(MiscUtils.GetModeChoice(3, 1.5f, 0.5f) * 60);

			void PunchArm(DraekArm arm){
				Vector2 spawnCenter = NPC.Bottom - new Vector2(0, 0.667f * NPC.height);
				spawnCenter += new Vector2(-8 * 16 * NPC.spriteDirection, 20);

				arm.State = DraekArm.State_Punched;
				arm.NPC.ai[3] = NPC.target;
				arm.Timer = 0;
				arm.ShootTimer = 0;

				arm.NPC.Center = spawnCenter;
				arm.NPC.velocity = new Vector2(-20f * NPC.spriteDirection, 0).RotatedByRandom(MathHelper.ToRadians(15f));

				SoundEngine.PlaySound(SoundID.Item1, spawnCenter);
			}

			if(PunchProgress == -1)
				PunchProgress = Punch_PunchFirst;

			if(PunchProgress == Punch_PunchFirst){
				if((int)(AI_Animation_Counter / 7.5f) == 6){
					//Un-hide the first arm
					if(NPC.spriteDirection == 1){
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

					Vector2 goreTop = NPC.Center;
					goreTop.X += (NPC.TopLeft.X - NPC.Center.X) / 2f;
					goreTop.Y += 20f;
					for(int i = 0; i < 4; i++){
						for(int d = 0; d < 20; d++){
							Dust dust = Dust.NewDustDirect(goreTop + new Vector2(0, 16 * i), 0, 0, 82);
							dust.noGravity = true;
							dust.velocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * 5.5f;
						}
					}
					goreTop.X = NPC.Center.X;
					goreTop.X += (NPC.TopRight.X - NPC.Center.X) / 2f;
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
