using ReLogic.Content;
using ReLogic.Content.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader.Core;

namespace CosmivengeonMod {
	// Near-copy of Terraria.ModLoader.Assets.TModContentSource
	internal class PossibleAssetsDirectoryRedirectContentSource : ContentSource {
		private readonly TmodFile file;

		public PossibleAssetsDirectoryRedirectContentSource(TmodFile file) {
			ArgumentNullException.ThrowIfNull(file);

			this.file = file;

			// Skip loading assets on servers
			if (Main.dedServ)
				return;

			// From Terraria.Initializers.AssetInitializer::CreateAssetServices()
			//     services.AddService(typeof(AssetReaderCollection), assetReaderCollection);
			var assetReaderCollection = Main.instance.Services.GetService(typeof(AssetReaderCollection)) as AssetReaderCollection;

			SetAssetNames(file
				.Select(fileEntry => fileEntry.Name)
				.Where(name => assetReaderCollection.TryGetReader(Path.GetExtension(name), out _)));
		}

		public override Stream OpenStream(string fullAssetName) {
			// File exists without a redirection.  Attempt to use it
			if (file.HasFile(fullAssetName))
				return file.GetStream(fullAssetName, newFileStream: true);

			// File might need a redirection...
			if (!fullAssetName.StartsWith("Assets/") && file.HasFile("Assets/" + fullAssetName))
				return file.GetStream("Assets/" + fullAssetName, newFileStream: true);

			// File not found
			throw new KeyNotFoundException(fullAssetName);
		}
	}
}
