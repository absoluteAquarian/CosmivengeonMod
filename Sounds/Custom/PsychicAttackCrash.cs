using Microsoft.Xna.Framework.Audio;
using Terraria;

namespace CosmivengeonMod.Sounds.Custom {
	public class PsychicAttackCrash : ModSound {
		//Uses the "freeze3" sound effect from http://starmen.net/mother2/soundfx/

		public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type) {
			//An optional variable to make controlling the volume easier
			float volumeFactor = 0.75f;

			if (soundInstance is null) {
				//This is a new sound instance

				soundInstance = sound.CreateInstance();
				soundInstance.Volume = volume * volumeFactor;
				soundInstance.Pan = pan;
				Main.PlaySoundInstance(soundInstance);
				return soundInstance;
			}

			//This is an existing sound instance that's still playing
			soundInstance.Volume = volume * volumeFactor;
			soundInstance.Pan = pan;
			return soundInstance;
		}
	}
}
