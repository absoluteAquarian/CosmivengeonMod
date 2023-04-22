using CosmivengeonMod.Utility;
using StructureHelper;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.World.Generation;

namespace CosmivengeonMod.API.Subworlds{
	public class CelestialPeaks : Subworld{
		public override int height => 1500;

		public override int width => 500;

		public override List<GenPass> tasks => new List<GenPass>(){
			new SubworldGenPass(progress => {
				//Initialize the world
				progress.Message = "Initializing Subworld";

				//Hides the underground layer just out of bounds
				Main.worldSurface = Main.maxTilesY - 42;

				//Hides the cavern layer way out of bounds
				Main.rockLayer = Main.maxTilesY;
			}),
			new SubworldGenPass(progress => GenerateSpawnIsland(progress)),
			new SubworldGenPass(progress => GenerateIslands(progress)),
			new SubworldGenPass(progress => GenerateBigIsland(progress))
		};

		private void GenerateSpawnIsland(GenerationProgress progress){
			progress.Message = "Creating Spawn Island";

			Point16 dims = MiscUtils.GetStructureDimensions("Assets/Subworlds/Prism/spawnisland");

			Generator.GenerateStructure("Assets/Subworlds/Prism/spawnisland",
				new Point16(width / 2 - dims.X / 2, height - 260 - dims.Y / 2),
				CoreMod.Instance);
		}

		const int totalIslands = 20;

		private void GenerateIslands(GenerationProgress progress){
			progress.Message = "Creating Smaller Islands";

			int curIsland = 0;

			int y = height - 260 - 40;

			GenerateIsland(progress, x: 100, rangeX: 20, y: y, rangeY: 10, ref curIsland);
		}

		private void GenerateIsland(GenerationProgress progress, int x, int rangeX, int y, int rangeY, ref int curIsland){
			if(curIsland + 1 > totalIslands)
				throw new Exception("Too many sub-islands were generated");

			// TODO: spawn regular sky island, but randomize the thing generated in the middle
			//Random things:  broken house with a chest inside, empty broken house, altar (rare, contains good item, at least 1 will generate)
			int genX = x + WorldGen.genRand.Next(rangeX);
			int genY = y + WorldGen.genRand.Next(rangeY);

			API.ExternalMethods.Vanilla.PrismCloudIsland(genX, genY);

			progress.Set(++curIsland / (float)totalIslands);
		}

		private void GenerateBigIsland(GenerationProgress progress){
			progress.Message = "Creating Arena Island";

			Point16 dims = MiscUtils.GetStructureDimensions("Assets/Subworlds/Prism/arenaisland");

			Generator.GenerateStructure("Assets/Subworlds/Prism/arenaisland", new Point16(width / 2 - dims.X / 2, 350 - dims.Y / 2), CoreMod.Instance);

			progress.Set(1f);
		}
	}
}
