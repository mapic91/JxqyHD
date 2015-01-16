using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    public static class SoundManager
    {
        static private readonly Dictionary<int,SoundEffectInstance> _soundEffectInstances = new Dictionary<int, SoundEffectInstance>();
        static public void Play3DSoundOnece(SoundEffect soundEffect, Vector2 direction)
        {
            if (soundEffect == null) return;

            SoundEffectInstance instance;
            var hash = soundEffect.GetHashCode();
            if (_soundEffectInstances.ContainsKey(hash))
            {
                instance = _soundEffectInstances[hash];
            }
            else
            {
                instance = soundEffect.CreateInstance();
                _soundEffectInstances[hash] = instance;
            }
            Apply3D(instance, direction);
            instance.Play();
        }

        /// <summary>
        /// Apply 3D effect to sound effect instance.
        /// </summary>
        /// <param name="soundEffectInstance">The sound effect instance.</param>
        /// <param name="direction">The directon and distance from listenr to sound instance.</param>
        public static void Apply3D(SoundEffectInstance soundEffectInstance, Vector2 direction)
        {
            if (soundEffectInstance == null) return;

            var length = direction.Length();
            var listener = new AudioListener();
            var emitter = new AudioEmitter();
            listener.Position = Vector3.Zero;
            emitter.Position = Vector3.Zero;
            if (length > 0 &&
                length < Globals.SoundMaxDistance)
            {
                direction.Normalize();
                var percent = length / Globals.SoundMaxDistance;
                direction *= (percent * Globals.Sound3DMaxDistance);
                emitter.Position = new Vector3(direction.X, 0, direction.Y);
            }
            else if(length != 0)
            {
                emitter.Position = new Vector3(999999f);
            }
            soundEffectInstance.Apply3D(listener, emitter);
        }

        public static void PlaySoundEffectOnce(SoundEffect soundEffect)
        {
            if(soundEffect != null)
                soundEffect.Play();
        }

        public static void ClearCache()
        {
            _soundEffectInstances.Clear();
        }
    }
}
