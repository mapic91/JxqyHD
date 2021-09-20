using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Engine.Gui;
using IniParser;
using IniParser.Model;

namespace Engine
{
    public class Good
    {
        private AttrInt _cost;
        private AttrInt _sellPrice;
        private AttrInt _specialEffectValue = new AttrInt(1);
        public string FileName { set; get; }
        public string Name { set; get; }
        public GoodKind Kind { set; get; }
        public string Intro { set; get; }
        public AttrInt Effect { set; get; }
        public Asf Image { set; get; }
        public Asf Icon { set; get; }
        public AttrInt Life { set; get; }
        public AttrInt Thew { set; get; }
        public AttrInt Mana { set; get; }
        public EquipPosition Part { set; get; }
        public AttrInt LifeMax { set; get; }
        public AttrInt ThewMax { set; get; }
        public AttrInt ManaMax { set; get; }
        public AttrInt Attack { set; get; }
        public AttrInt Attack2 { set; get; }
        public AttrInt Attack3 { set; get; }
        public AttrInt Defend { set; get; }
        public AttrInt Defend2 { set; get; }
        public AttrInt Defend3 { set; get; }
        public AttrInt Evade { set; get; }
        public AttrInt EffectType { set; get; }
        public AttrInt SpecialEffect { set; get; }
        public AttrInt SpecialEffectValue
        {
            get { return _specialEffectValue; }
            set { _specialEffectValue = value; }
        }

        public string Script { set; get; }
        public bool IsOk { private set; get; }
        public string FlyIni { set; get; }
        public string FlyIni2 { set; get; }
        public string MagicIniWhenUse { set; get; }

        public string[] User { set; get; }
        public AttrInt MinUserLevel { set; get; }

        public AttrInt AddMagicEffectPercent { set; get; }
        public AttrInt AddMagicEffectAmount { set; get; }
        public string AddMagicEffectName { set; get; }
        public string AddMagicEffectType { set; get; }

        public AttrInt ChangeMoveSpeedPercent { set; get; }
        public AttrInt ColdMilliSeconds { set; get; }
        public string ReplaceMagic { set; get; }
        public Magic UseReplaceMagic { set; get; }

        public Magic MagicToUseWhenBeAttacked { set; get; }
        public AttrInt MagicDirectionWhenBeAttacked { set; get; }

        public AttrInt NoNeedToEquip { set; get; }

        public bool HasRandAttr
        {
            get
            {
                foreach (var p in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.PropertyType == typeof(AttrInt))
                    {
                        var v = (AttrInt)p.GetValue(this, null);
                        if (v.IsRand())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public GoodEffectType TheEffectType
        {
            get
            {
                var type = GoodEffectType.None;
                switch (Kind)
                {
                    case GoodKind.Drug:
                    {
                        switch (EffectType.GetOneValue())
                        {
                            case 1:
                                type = GoodEffectType.ClearFrozen;
                                break;
                            case 2:
                                type = GoodEffectType.ClearPoison;
                                break;
                            case 3:
                                type = GoodEffectType.ClearPetrifaction;
                                break;
                        }
                    }
                        break;
                    case GoodKind.Equipment:
                    {
                        switch (EffectType.GetOneValue())
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

        public int CostRaw
        {
            get
            {
                int cost = _cost.GetMaxValue();
                if (cost == 0)
                {
                    switch (Kind)
                    {
                        case GoodKind.Drug:
                            cost = (Thew.GetMaxValue() * 4 + Life.GetMaxValue() * 2 + Mana.GetMaxValue() * 2) *
                                   (1 + (EffectType.GetMaxValue() == 0 ? 0 : 1));
                            break;
                        case GoodKind.Equipment:
                            if (NoNeedToEquip.GetMaxValue() > 0)
                            {
                                cost = 0;
                            }
                            else
                            {
                                cost = (Attack.GetMaxValue() * 20 + Attack2.GetMaxValue() * 20 + Attack3.GetMaxValue() * 20 + Defend.GetMaxValue() * 20 + Defend2.GetMaxValue() * 20 + Defend3.GetMaxValue() * 20 + Evade.GetMaxValue() * 40 + LifeMax.GetMaxValue() * 2 + ThewMax.GetMaxValue() * 3 + ManaMax.GetMaxValue() * 2) *
                                       (1 + (EffectType.GetMaxValue() == 0 ? 0 : 1));
                            }
                            break;
                    }
                }

                return cost;
            }
        }

        public AttrInt Cost
        {
            set { _cost = value; }
            get
            {
                return new AttrInt(CostRaw * GuiManager.BuyInterface.BuyPercent / 100);
            }
        }

        public AttrInt SellPrice
        {
            get
            {
                return new AttrInt((_sellPrice.GetMaxValue() > 0 ? _sellPrice.GetMaxValue() : CostRaw / 2) * GuiManager.BuyInterface.RecyclePercent / 100);
            }
            set { _sellPrice = value; }
        }

        public bool IsSellPriceSetted
        {
            get { return _sellPrice.GetMaxValue() > 0; }
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
                    case "FlyIni":
                    case "FlyIni2":
                    case "MagicIniWhenUse":
                    case "AddMagicEffectName":
                    case "AddMagicEffectType":
                    case "ReplaceMagic":
                        info.SetValue(this, value, null);
                        break;
                    case "Kind":
                        info.SetValue(this, (GoodKind)int.Parse(value), null);
                        break;
                    case "Image":
                    case "Icon":
                        info.SetValue(this, Utils.GetAsf(@"asf\goods\", value), null);
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
                    case "User":
                        if (!string.IsNullOrEmpty(value))
                        {
                            User = value.Split(',');
                        }
                        break;
                    case "UseReplaceMagic":
                        UseReplaceMagic = Utils.GetMagic(value, false);
                        break;
                    case "MagicToUseWhenBeAttacked":
                        MagicToUseWhenBeAttacked = Utils.GetMagic(value, false);
                        break;
                    default:
                        info.SetValue(this, new AttrInt(value), null);
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
                parser.Parser.Configuration.NotTrimValue = true;
                var data = parser.ReadFile(filePath, Globals.LocalEncoding);
                foreach (var keyValue in data["Init"])
                {
                    AssignToValue(keyValue.KeyName, keyValue.Value);
                }
            }
            catch (Exception ecxeption)
            {
                Log.LogFileLoadError("Good", filePath, ecxeption);
                return false;
            }
            IsOk = true;
            return true;
        }

        public void Save(string filePath)
        {
            var data = new IniData();
            data.Sections.AddSection("Init");
            Save(data["Init"]);
            File.WriteAllText(filePath, data.ToString(), Globals.LocalEncoding);
        }

        public void Save(KeyDataCollection keyDataCollection)
        {
            foreach (var p in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.Name == "FileName")
                {
                    continue;
                }
                if (p.PropertyType == typeof(AttrInt))
                {
                    if (p.Name == "Cost")
                    {
                        keyDataCollection.AddKey("Cost", _cost.GetString());
                    }
                    else if (p.Name == "SellPrice")
                    {
                        keyDataCollection.AddKey("SellPrice", _sellPrice.GetString());
                    }
                    else
                    {
                        var v = (AttrInt)p.GetValue(this, null);
                        keyDataCollection.AddKey(p.Name, v.GetString());
                    }
                    
                }
                else if (p.PropertyType == typeof(Asf))
                {
                    var v = (Asf)p.GetValue(this, null);
                    if (v != null)
                    {
                        keyDataCollection.AddKey(p.Name, v.FileName);
                    }
                }
                else if (p.PropertyType == typeof(string))
                {
                    var v = (string)p.GetValue(this, null);
                    if (!string.IsNullOrEmpty(v))
                    {
                        keyDataCollection.AddKey(p.Name, v);
                    }
                }
                else if (p.PropertyType == typeof(Magic))
                {
                    var v = (Magic)p.GetValue(this, null);
                    if (v != null)
                    {
                        keyDataCollection.AddKey(p.Name, v.FileName);
                    }
                }
                else if (p.PropertyType == typeof(GoodKind))
                {
                    var v = (GoodKind)p.GetValue(this, null);
                    keyDataCollection.AddKey(p.Name, ((int)v).ToString());
                }
                else if (p.PropertyType == typeof(EquipPosition))
                {
                    var v = (EquipPosition)p.GetValue(this, null);
                    keyDataCollection.AddKey(p.Name, v.ToString());
                }
            }
        }

        public Good GetOneNonRandom()
        {
            var good = (Good)MemberwiseClone();
            var names = new List<string>();
            var kv = new Dictionary<string, int>();
            foreach (var p in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.PropertyType == typeof(AttrInt))
                {
                    var v = (AttrInt)p.GetValue(this, null);
                    if (p.Name == "SellPrice")
                    {
                        v = _sellPrice;
                    }
                    else if (p.Name == "Cost")
                    {
                        v = _cost;
                    }
                    if (v.IsRand())
                    {
                        var value = v.GetOneValue();
                        names.Add(p.Name);
                        kv[p.Name] = value;
                        p.SetValue(good, new AttrInt(value), null);
                    }
                }
            }
            if (names.Count == 0)
            {
                throw new Exception("Good has no random value");
            }
            names.Sort();
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(FileName);
            var ext = Path.GetExtension(FileName);
            foreach (var name in names)
            {
                fileNameWithoutExt += name;
                fileNameWithoutExt += kv[name].ToString();
            }

            good.FileName = fileNameWithoutExt + ext;
            return good;
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
            ClearPoison,
            EnemyPetrified,
            ClearPetrifaction
        }

        public enum EquipPosition
        {
            None,
            Head,
            Neck,
            Body,
            Back,
            Hand,
            Wrist,
            Foot
        }
        #endregion Enum
    }
}