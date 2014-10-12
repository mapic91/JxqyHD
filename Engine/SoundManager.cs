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
        static private LinkedList<SoundEffectInstance> _soundEffectInstances = new LinkedList<SoundEffectInstance>(); 
        static public void Play3DSoundOnece(SoundEffect soundEffect, Vector2 direction)
        {
            if (soundEffect == null) return;
            var length = direction.Length();
            if ((int)length == 0) soundEffect.Play(Globals.SoundEffectVolume, 0, 0);
            if (length < Globals.SoundMaxDistance)
            {
                var instance = soundEffect.CreateInstance();
                direction.Normalize();
                var percent = length / Globals.SoundMaxDistance;
                direction *= (percent * Globals.Sound3DMaxDistance);
                var listener = new AudioListener();
                var emitter = new AudioEmitter();
                listener.Position = Vector3.Zero;
                emitter.Position = new Vector3(direction.X, 0, direction.Y);
                instance.Apply3D(listener, emitter);
                instance.Play();
                _soundEffectInstances.AddLast(instance);
            }
        }

        static public void Update()
        {
            for (var node = _soundEffectInstances.First; node != null;)
            {
                var next = node.Next;
                if (node.Value.State == SoundState.Stopped)
                {
                    _soundEffectInstances.Remove(node);
                }
                node = next;
            }
        }
    }
}
