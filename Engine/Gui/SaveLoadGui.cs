using System;
using Engine.Gui.Base;
using Engine.Script;
using Engine.Storage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class SaveLoadGui : GuiItem
    {
        private ListTextItem _list;
        private GuiItem _loadButton;
        private GuiItem _saveButton;
        private GuiItem _exitButton;
        private TextGui _saveTime;
        private Bmp _saveSnapshot;
        private LineText _message;
        private const int IndexBegin = 1;
        private const int IndexEnd = 7;

        public bool CanSave { set; get; }

        public override bool IsShow
        {
            get
            {
                return base.IsShow;
            }
            set
            {
                base.IsShow = value;
                if (value && _list.SelectionIndex != -1)
                {
                    ShowSaveIndex(_list.SelectionIndex + 1);
                }
                if (!value)
                {
                    //Clear message.
                    _message.Text = string.Empty;
                }
            } 
        }

        public SaveLoadGui()
        {
            var cfg = GuiManager.Setttings.Sections["SaveLoad"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f + int.Parse(cfg["LeftAdjust"]),
                (Globals.WindowHeight - Height) / 2f + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["SaveLoad_Text_List"];
            //Save load list
            var itemText = cfg["Text"].Split('/');
            _list = new ListTextItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                itemText.Length,
                itemText,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                int.Parse(cfg["ItemHeight"]),
                Utils.GetColor(cfg["Color"]),
                Utils.GetColor(cfg["SelectedColor"]),
                Utils.GetSoundEffect(cfg["Sound"]));

            //Save snapshot
            cfg = GuiManager.Setttings.Sections["Save_Snapshot"];
            _saveSnapshot = new Bmp(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                "",
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]));

            //Buttons
            cfg = GuiManager.Setttings.Sections["SaveLoad_Load_Btn"];
            var asf = Utils.GetAsf(null, cfg["Image"]);
            var clickedSound = Utils.GetSoundEffect(cfg["Sound"]);
            _loadButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            cfg = GuiManager.Setttings.Sections["SaveLoad_Save_Btn"];
            asf = Utils.GetAsf(null, cfg["Image"]);
            clickedSound = Utils.GetSoundEffect(cfg["Sound"]);
            _saveButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            cfg = GuiManager.Setttings.Sections["SaveLoad_Exit_Btn"];
            asf = Utils.GetAsf(null, cfg["Image"]);
            clickedSound = Utils.GetSoundEffect(cfg["Sound"]);
            _exitButton = new GuiItem(this, 
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            //Save time
            cfg = GuiManager.Setttings.Sections["SaveLoad_Save_Time_Text"];
            _saveTime = new TextGui(this, 
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));

            //Message
            cfg = GuiManager.Setttings.Sections["SaveLoad_Message_Line_Text"];
            _message = new LineText(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                (LineText.Align)int.Parse(cfg["Align"]), 
                string.Empty,
                Utils.GetColor(cfg["Color"]),
                Globals.FontSize12);

            RegisterEvent();

            SetSaveLoadIndex(Globals.SaveLoadSelectionIndex);

            IsShow = false;
        }

        private void RegisterEvent()
        {
            //List event
            _list.ItemClick += (arg1, arg2) => ShowSaveIndex(arg2.ItemIndex + 1);

            //Button event
            _saveButton.Click += (arg1, arg2) =>
            {
                if (_list.SelectionIndex != -1 &&
                    SaveLoadIndexInRange(_list.SelectionIndex + 1))
                {
                    if (!Globals.TheGame.IsSafe())
                    {
                        _message.Text = "战斗中不能存档，找个安全的地方存档吧。";
                        return;
                    }

                    var index = _list.SelectionIndex + 1;

                    var show = GuiManager.IsShow;
                    //Hide GUI when take snapshot
                    GuiManager.IsShow = false;
                    var snapshot = Globals.TheGame.TakeSnapShot();
                    //Restore
                    GuiManager.IsShow = show;

                    Saver.SaveGame(index, snapshot);

                    IsShow = false;
                    GuiManager.ShowSystem(false);
                }
            };
            _loadButton.Click += (arg1, arg2) =>
            {
                if (_list.SelectionIndex != -1 &&
                    SaveLoadIndexInRange(_list.SelectionIndex + 1))
                {
                    var index = _list.SelectionIndex + 1;
                    if (StorageBase.CanLoad(index))
                    {
                        IsShow = false;
                        Loader.LoadGame(index);
                    }
                }
            };
            _exitButton.Click += (arg1, arg2) =>
            {
                IsShow = false;
                GuiManager.ShowSaveLoad(false);
            };
        }

        private void ShowSaveIndex(int index)
        {
            if(!SaveLoadIndexInRange(index))return;
            _saveTime.Text = StorageBase.GetSaveTime(index);
            _saveSnapshot.BmpFilePath = StorageBase.GetSaveSnapShotFilePath(index);
            Globals.SaveLoadSelectionIndex = index;
        }

        private bool SaveLoadIndexInRange(int index)
        {
            return !(index < IndexBegin || index > IndexEnd);
        }

        public void SetSaveLoadIndex(int index)
        {
            if(!SaveLoadIndexInRange(index)) return;
            _list.SetSelectionIndex(index - 1);
            ShowSaveIndex(index);
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            _list.Update(gameTime);
            _saveSnapshot.Update(gameTime);
            _loadButton.Update(gameTime);
            if (CanSave)
            {
                _saveButton.Update(gameTime);
            }
            _exitButton.Update(gameTime);
            _saveTime.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow) return;
            base.Draw(spriteBatch);
            _list.Draw(spriteBatch);
            _saveSnapshot.Draw(spriteBatch);
            _loadButton.Draw(spriteBatch);
            _saveButton.Draw(spriteBatch);
            _exitButton.Draw(spriteBatch);
            _saveTime.Draw(spriteBatch);
            _message.Draw(spriteBatch);
        }
    }
}