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
        public static void Play(string fileName)
        {
            if(string.IsNullOrEmpty(fileName)) return;
            var path = @"music\" + Path.GetFileNameWithoutExtension(fileName);
            try
            {
                var song = Globals.TheGame.Content.Load<Song>(path);
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
