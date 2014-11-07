using System;
using System.IO;
using System.Runtime.CompilerServices;
using IniParser;

namespace Engine
{
    public class Good
    {
        private int _cost;
        public string FileName { set; get; }
        public string Name { set; get; }
        public GoodKind Kind { set; get; }
        public string Intro { set; get; }
        public int Effect { set; get; }
        public Asf Image { set; get; }
        public Asf Icon { set; get; }
        public int Life { set; get; }
        public int Thew { set; get; }
        public int Mana { set; get; }
        public EquipPosition Part { set; get; }
        public int LifeMax { set; get; }
        public int ThewMax { set; get; }
        public int ManaMax { set; get; }
        public int Attack { set; get; }
        public int Defend { set; get; }
        public int Evade { set; get; }
        public int EffectType { set; get; }
        public string Script { set; get; }
        public bool IsOk { private set; get; }

        public GoodEffectType TheEffectType
        {
            get
            {
                var type = GoodEffectType.None;
                switch (Kind)
                {
                    case GoodKind.Drug:
                    {
                        switch (EffectType)
                        {
                            case 1:
                                type = GoodEffectType.ClearFrozen;
                                break;
                            case 2:
                                type = GoodEffectType.ClearPoisoned;
                                break;
                            case 3:
                                type = GoodEffectType.ClearPetrified;
                                break;
                        }
                    }
                        break;
                    case GoodKind.Equipment:
                    {
                        switch (EffectType)
                        {
                            case 1:
                            {
                                switch (Part)
                                {
                                    case EquipPosition.Foot:
                                        type = GoodEffectType.ThewNotLoseWhenRun;
                                        break;
                                    case EquipPosition.Neck:
                                        type = GoodEffectType.ManaRestore;
                                        break;
                                    case EquipPosition.Hand:
                                        type = GoodEffectType.EnemyFrozen;
                                        break;
                                }
                            }
                                break;
                            case 2:
                                switch (Part)
                                {
                                    case EquipPosition.Hand:
                                        type = GoodEffectType.EnemyPoisoned;
                                        break;
                                }
                                break;
                            case 3:
                                switch (Part)
                                {
                                    case EquipPosition.Hand:
                                        type = GoodEffectType.EnemyPetrified;
                                        break;
                                }
                                break;
                        }
                    }
                        break;
                }
                return type;
            }
        }

        public int Cost
        {
            set { _cost = value; }
            get
            {
                if (_cost == 0)
                {
                    switch (Kind)
                    {
                        case GoodKind.Drug:
                            return ( Thew*4 + Life*2 + Mana*2 ) *
                                   ( 1 + (EffectType == 0 ? 0 : 1) );
                        case GoodKind.Equipment:
                            return ( Attack*20 + Defend*20 + Evade*40 + LifeMax*2 + ThewMax*3 + ManaMax*2 ) *
                                (1 + (EffectType == 0 ? 0 : 1));
                    }
                }
                return _cost;
            }
        }

        public Good(string filePath)
        {
            Load(filePath);
        }

        private void AssignToValue(string keyName, string value)
        {
            try
            {
                var info = this.GetType().GetProperty(keyName);
                switch (keyName)
                {
                    case "Name":
                    case "Intro":
                    case "Script":
                        info.SetValue(this, value, null);
                        break;
                    case "Kind":
                        info.SetValue(this, (GoodKind)int.Parse(value), null);
                        break;
                    case "Image":
                    case "Icon":
                        info.SetValue(this, Utils.GetAsf(@"asf\goods\" + value), null);
                        break;
                    case "Part":
                        {
                            var position = EquipPosition.None;
                            switch (value)
                            {
                                case "Body":
                                    position = EquipPosition.Body;
                                    break;
                                case "Foot":
                                    position = EquipPosition.Foot;
                                    break;
                                case "Head":
                                    position = EquipPosition.Head;
                                    break;
                                case "Neck":
                                    position = EquipPosition.Neck;
                                    break;
                                case "Back":
                                    position = EquipPosition.Back;
                                    break;
                                case "Wrist":
                                    position = EquipPosition.Wrist;
                                    break;
                                case "Hand":
                                    position = EquipPosition.Hand;
                                    break;
                            }
                            info.SetValue(this, position, null);
                        }
                        break;
                    default:
                        info.SetValue(this, int.Parse(value), null);
                        break;
                }
            }
            catch (Exception)
            {
                //Do nothing
                return;
            }
        }

        public bool Load(string filePath)
        {
            try
            {
                FileName = Path.GetFileName(filePath);
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath, Globals.SimpleChinaeseEncoding);
                foreach (var keyValue in data["Init"])
                {
                    AssignToValue(keyValue.KeyName, keyValue.Value);
                }
            }
            catch (Exception ecxeption)
            {
                Log.LogMessageToFile("Good load failed [" + filePath + "]." + ecxeption);
                return false;
            }
            IsOk = true;
            return true;
        }

        public static bool CanEquip(Good good, Good.EquipPosition position)
        {
            return good != null && good.Part == position;
        }

        #region Enum
         public enum GoodKind
         {
             Drug,
             Equipment,
             Event
         }
         
        public enum GoodEffectType
        {
            None,
            ThewNotLoseWhenRun,
            ManaRestore,
            EnemyFrozen,
            ClearFrozen,
            EnemyPoisoned,
            ClearPoisoned,
            EnemyPetrified,
            ClearPetrified
        }

        public enum EquipPosition
        {
            None,
            Body,
            Foot,
            Head,
            Neck,
            Back,
            Wrist,
            Hand
        }
        #endregion Enum
    }
}