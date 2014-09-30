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
        private Sprite _image;
        private int _dir;
        public string Name;
        public int Kind;
        public int Releation;
        public int PathFinder;
        public int State;
        public int VisionRadius;
        public int DialogRadius;
        public int AttackRadius;
        public int Dir
        {
            get { return _dir; }
            set { _dir = value%8; }
        }
        public int MapX;
        public int MapY;
        public int Lum;
        public int Action;
        public int WalkSpeed;
        public int Evade;
        public int Attack;
        public int AttackLevel;
        public int Defend;
        public int Exp;
        public int LevelUpExp;
        public int Level;
        public int Life;
        public int LifeMax;
        public int Thew;
        public int ThewMax;
        public int Mana;
        public int ManaMax;
        public StateMapList NpcIni;
        public NpcStateInfo ObjIni;
        public int FlyIni;
        public int FlyIni2;
        public string ScriptFile;
        public string DeathScript;
        public int ExpBonus;
        public int FixedPos;
        public int Idle;

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
