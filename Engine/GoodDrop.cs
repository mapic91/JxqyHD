using System;
using System.Collections.Generic;

namespace Engine
{
    public static class GoodDrop
    {
        static private Obj GetObj(GoodType type, Character character)
        {
            if (character == null) return null;
            var fileName = string.Empty;
            switch (type)
            {
                case GoodType.Weapon:
                    fileName = "可捡武器.ini";
                    break;
                case GoodType.Armor:
                    fileName = "可捡防具.ini";
                    break;
                case GoodType.Money:
                    fileName = "可捡钱.ini";
                    break;
                case GoodType.Drug:
                    fileName = "可捡药品.ini";
                    break;
                default:
                    return null;
            }
            var obj = new Obj(@"ini\obj\" + fileName);
            obj.TilePosition = character.TilePosition;
            obj.ScriptFile = GetScriptFileName(type, character.Level);
            return obj;
        }

        static private string GetScriptFileName(GoodType type, int characterLevel)
        {
            var fileName = string.Empty;
            switch (type)
            {
                case GoodType.Weapon:
                case GoodType.Armor:
                case GoodType.Money:
                {
                    var level = characterLevel/12 + 1;
                    if (level > 7) level = 7;
                    switch (type)
                    {
                        case GoodType.Weapon:
                            fileName = level + "级武器.txt";
                            break;
                        case GoodType.Armor:
                            fileName = level + "级防具.txt";
                            break;
                        case GoodType.Money:
                            fileName = level + "级钱.txt";
                            break;
                    }
                }
                    break;
                case GoodType.Drug:
                    if (characterLevel <= 10)
                    {
                        fileName = "低级药品.txt";
                    }
                    else if (characterLevel <= 30)
                    {
                        fileName = "中级药品.txt";
                    }
                    else if (characterLevel <= 60)
                    {
                        fileName = "高级药品.txt";
                    }
                    else
                    {
                        fileName = "特级药品.txt";
                    }
                    break;
            }

            return fileName;
        }

        /// <summary>
        /// Get drap obj.If character not drop obj return null.
        /// </summary>
        /// <param name="characterLevel">Character level</param>
        /// <returns>Droped obj.If not drop return null.</returns>
        static public Obj GetDropObj(Character character)
        {
            //Just enemy can drop
            if (!character.IsEnemy) return null;

            var goodType = (GoodType) Globals.TheRandom.Next(0, (int) GoodType.MaxType);
            var maxRandValue = 4;
            switch (goodType)
            {
                case GoodType.Weapon:
                case GoodType.Armor:
                    maxRandValue = 25;
                    break;
            }

            if (Globals.TheRandom.Next(maxRandValue) == 0)
            {
                return GetObj(goodType, character);
            }
            return null;
        }

        public enum GoodType
        {
            Weapon,
            Armor,
            Money,
            Drug,
            MaxType
        }
    }
}