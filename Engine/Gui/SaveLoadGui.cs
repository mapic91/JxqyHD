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

        public Texture2D Snapshot;
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
                    if (Snapshot != null)
                    {
                        Snapshot.Dispose();
                        Snapshot = null;
                    }
                }
            } 
        }

        public SaveLoadGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\saveload", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f,
                (Globals.WindowHeight - Height) / 2f);

            //Save load list
            var itemText = new String[]
            {
                "进度一",
                "进度二",
                "进度三",
                "进度四",
                "进度五",
                "进度六",
                "进度七"
            };
            _list = new ListTextItem(this,
                new Vector2(135, 118),
                80,
                189,
                null,
                7,
                itemText,
                3,
                0,
                25,
                new Color(91, 31, 27) * 0.8f,
                new Color(102, 73, 212) * 0.8f,
                Utils.GetSoundEffect("界-浏览.wav"));

            //Save snapshot
            _saveSnapshot = new Bmp(this,
                new Vector2(256, 94),
                "",
                267,
                200);

            //Buttons
            var asf = Utils.GetAsf(@"asf\ui\saveload", "btnLoad.asf");
            var clickedSound = Utils.GetSoundEffect("界-大按钮.wav");
            _loadButton = new GuiItem(this,
                new Vector2(248, 355),
                64, 
                72,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);
            asf = Utils.GetAsf(@"asf\ui\saveload", "btnSave.asf");
            _saveButton = new GuiItem(this,
                new Vector2(366, 355),
                64,
                72,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);
            asf = Utils.GetAsf(@"asf\ui\saveload", "btnExit.asf");
            _exitButton = new GuiItem(this, 
                new Vector2(464, 355),
                64, 
                72,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            //Save time
            _saveTime = new TextGui(this, 
                new Vector2(254, 310),
                350,
                30,
                Globals.FontSize10,
                1,
                0,
                "",
                new Color(182, 219, 189) * 0.7f);

            //Message
            _message = new LineText(this,
                new Vector2(0, 440),
                BaseTexture.Width,
                40,
                LineText.Align.Center, 
                string.Empty,
                Color.Gold * 0.8f,
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
                    Saver.SaveGame(index, Snapshot);

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