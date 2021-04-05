using SubworldLibrary;
using System.Collections.Generic;
using Terraria.World.Generation;

namespace CosmivengeonMod.Library.Dimensions{
	public class PrismWorld : Subworld{
		public override int height => 1000;
		public override int width => 1000;

		public override List<GenPass> tasks => new List<GenPass>(){
			// TODO: generate structures via an image
		};
	}
}
