using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Media;

namespace Engine
{
    static public class BackgroundMusic
    {
        private static ConstructorInfo SongCtr = typeof(Song).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance, null,
            new[] { typeof(string), typeof(string), typeof(int) }, null);

        public static void Play(string fileName)
        {
            var path = @"music\" + fileName;
            try
            {
                var song = (Song)SongCtr.Invoke(new object[] { "BackgroundMusic", path, 0 });
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(song);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Music file", path, exception);
            }
        }

        public static void Stop()
        {
            MediaPlayer.Stop();
        }

        public static void SetVolume(float volume)
        {
            MediaPlayer.Volume = volume;
        }
    }
}
