using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    using StateMapList = Dictionary<NpcState, NpcStateInfo>;
    public class Character
    {
        private StateMapList _stateList;
        private Sprite _image;

        public bool LoadCharacter(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                return LoadCharacter(lines);
            }
            catch (Exception exception)
            {
                Log.LogMessageToFile("Character load failed [" + filePath +"]." + exception);
                return false;
            }
        }

        public bool LoadCharacter(string[] lines)
        {
            return true;
        }


    }
}
