using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if(instance.State == SoundState.Playing) return;
            instance.Volume = Globals.SoundEffectVolume;

            var length = direction.Length();
            if ((int) length == 0)
            {
                instance.Play();
            }
            else if(length < Globals.SoundMaxDistance)
            {
                direction.Normalize();
                var percent = length / Globals.SoundMaxDistance;
                direction *= (percent * Globals.Sound3DMaxDistance);
                var listener = new AudioListener();
                var emitter = new AudioEmitter();
                listener.Position = Vector3.Zero;
                emitter.Position = new Vector3(direction.X, 0, direction.Y);
                instance.Apply3D(listener, emitter);
                instance.Play();
            }
        }

        public static void PlaySoundEffectOnce(SoundEffect soundEffect)
        {
            if(soundEffect != null)
                soundEffect.Play(Globals.SoundEffectVolume, 0f, 0f);
        }

        public static void ClearCache()
        {
            _soundEffectInstances.Clear();
        }
    }
}
