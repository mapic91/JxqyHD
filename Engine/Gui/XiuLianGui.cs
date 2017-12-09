using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class XiuLianGui : GuiItem
    {
        private DragDropItem _infoItem;
        private TextGui _levelText;
        private TextGui _expText;
        private TextGui _nameText;
        private TextGui _introText;
        private int ItemIndex = 49;
        private bool _isShow = false;
        private bool _isItemChange;

        public override bool IsShow
        {
            get { return _isShow; }
            set
            {
                _isShow = value;
                if(value) UpdateItem();
            }
        }

        public XiuLianGui()
        {
            ItemIndex = MagicListManager.XiuLianIndex;
            var cfg = GuiManager.Setttings.Sections["XiuLian"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f - Width + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["XiuLian_Magic_Image"];
            _infoItem = new DragDropItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new MagicGui.MagicItemData(ItemIndex));
            _infoItem.Drop += MagicGui.DropHandler;
            _infoItem.Drop += (arg1, arg2) => _isItemChange = true;
            _infoItem.MouseStayOver += MagicGui.MouseStayOverHandler;
            _infoItem.MouseLeave += MagicGui.MouseLeaveHandler;

            cfg = GuiManager.Setttings.Sections["XiuLian_Level_Text"];
            _levelText = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["XiuLian_Exp_Text"];
            _expText = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["XiuLian_Name_Text"];
            _nameText = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize12,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["XiuLian_Intro_Text"];
            _introText = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
        }

        public void UpdateItem()
        {
            var info =  MagicListManager.GetItemInfo(ItemIndex);
            if (info != null)
            {
                _levelText.Text = info.Level + "/10";
                _expText.Text = info.Exp + "/" + info.TheMagic.LevelupExp;
                _nameText.Text = info.TheMagic == null ? "无" : info.TheMagic.Name;
                _introText.Text = info.TheMagic == null ? "无" : info.TheMagic.Intro;
                if (_isItemChange || _infoItem.BaseTexture == null)
                {
                    //Change texture only item changed or base texture is null.
                    //Because this method is called in Update() if change base texture every update,
                    //texture won't update it's frame index and will always stay at frame 0.
                    _isItemChange = false;
                    _infoItem.BaseTexture = MagicListManager.GetTexture(ItemIndex);
                }
            }
            else
            {
                _levelText.Text = "1/10";
                _expText.Text = "0/0";
                _nameText.Text = "";
                _introText.Text = "";
                _infoItem.BaseTexture = null;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            UpdateItem();
            _infoItem.Update(gameTime);
            _levelText.Update(gameTime);
            _expText.Update(gameTime);
            _nameText.Update(gameTime);
            _introText.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _infoItem.Draw(spriteBatch);
            _levelText.Draw(spriteBatch);
            _expText.Draw(spriteBatch);
            _nameText.Draw(spriteBatch);
            _introText.Draw(spriteBatch);
        }
    }
}