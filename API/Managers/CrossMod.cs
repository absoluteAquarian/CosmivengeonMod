using CosmivengeonMod.Items.Equippable.Accessories.Draek;
using CosmivengeonMod.Items.Equippable.Accessories.Frostbite;
using CosmivengeonMod.Items.Equippable.Pets;
using CosmivengeonMod.Items.Equippable.Vanity.BossMasks;
using CosmivengeonMod.Items.Lore;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Items.Spawning.Boss;
using CosmivengeonMod.Items.Weapons.Draek;
using CosmivengeonMod.Items.Weapons.Frostbite;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.Worlds;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Managers{
	public static class CrossMod{
		public static void Load(){
			//Set the boss's position in BossChecklist if the mod is active
			//see:  https://github.com/JavidPack/BossChecklist/wiki/Support-using-Mod-Call
			if(ModReferences.BossChecklist.Active){
				//2.7f ==> just before Eater of Worlds
				ModReferences.BossChecklist.Call("AddBoss",
					2.7f,
					new List<int>(){
						ModContent.NPCType<Draek>(),
						ModContent.NPCType<DraekP2Head>()
					},
					CoreMod.Instance,
					$"${ModContent.GetInstance<Draek>().DisplayName.Key}",
					(Func<bool>)(() => WorldEvents.downedDraekBoss),
					ModContent.ItemType<DraekSummon>(),
					new List<int>(){
						ModContent.ItemType<DraekMask>(),
						ModContent.ItemType<StoneTablet>(),
						ModContent.ItemType<TerraBolt>()
					},
					new List<int>(){
						ModContent.ItemType<BasiliskStaff>(),
						ModContent.ItemType<BoulderChunk>(),
						ModContent.ItemType<EarthBolt>(),
						ModContent.ItemType<ForsakenOronoblade>(),
						ModContent.ItemType<Rockslide>(),
						ModContent.ItemType<Scalestorm>(),
						ModContent.ItemType<SlitherWand>(),
						ModContent.ItemType<Stoneskipper>(),
						ModContent.ItemType<JewelOfOronitus>(),
						ModContent.ItemType<SnakeShield>(),
						ModContent.ItemType<DraekScales>(),
						ItemID.LesserHealingPotion
					},
					$"Use a [i:{ModContent.ItemType<DraekSummon>()}] in the Forest biome.",
					null,  //Ignoring custom despawn message
					"CosmivengeonMod/API/Managers/CrossMod/Draek_BossLog"
				);

				//1.5f ==> between Slime King and Eye of Cthulhu
				ModReferences.BossChecklist.Call("AddBoss",
					1.5f,
					ModContent.NPCType<Frostbite>(),
					CoreMod.Instance,
					$"${ModContent.GetInstance<Frostbite>().DisplayName.Key}",
					(Func<bool>)(() => WorldEvents.downedFrostbiteBoss),
					ModContent.ItemType<IcyLure>(),
					new List<int>(){
						ModContent.ItemType<BabyCloudBottle>(),
						ModContent.ItemType<FrostbiteMask>(),
						ModContent.ItemType<IceforgedRelic>()
					},
					new List<int>(){
						ModContent.ItemType<FrostbiteFlamethrower>(),
						ModContent.ItemType<SnowballFlail>(),
						ModContent.ItemType<IceDisk>(),
						ModContent.ItemType<BlizzardRod>(),
						ModContent.ItemType<FrostRifle>(),
						ModContent.ItemType<SubZero>(),
						ModContent.ItemType<IceScepter>(),
						ModContent.ItemType<FrostDemonHorn>(),
						ModContent.ItemType<SnowscaleCoat>(),
						ItemID.LesserHealingPotion
					},
					$"Use a [i:{ModContent.ItemType<IcyLure>()}] in the Snow biome.",
					null,  //Ignoring custom despawn message
					"CosmivengeonMod/API/Managers/CrossMod/Frostbite_BossLog"
				);
			}
		}
	}
}
