using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void Play(string path)
        {
            try
            {
                var song = (Song)SongCtr.Invoke(new object[] { "BackgroundMusic", path, 0 });
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(song);
            }
            catch (Exception exception)
            {
                Log.LogMessageToFile("Play music [" + path + "] failed." + exception);
            }
        }

        public static void SetVolume(float volume)
        {
            MediaPlayer.Volume = volume;
        }
    }
}
