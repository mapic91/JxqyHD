using System;
using System.IO;
using Microsoft.Xna.Framework.Media;

namespace Engine
{
    static public class BackgroundMusic
    {
        private static string _fileName;

        public static string FileName
        {
            get { return _fileName; }
        }

        public static void Play(string fileName)
        {
            _fileName = fileName;
            if (string.IsNullOrEmpty(fileName))
            {
                //Stop music if file name is empty
                Stop();
                return;
            }
            try
            {
                var path = @"music\" + Path.GetFileNameWithoutExtension(fileName);
                var song = Globals.TheGame.Content.Load<Song>(path);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(song);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Music file", fileName, exception);
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
