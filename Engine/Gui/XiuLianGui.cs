using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class XiuLianGui : GuiItem
    {
        private DragDropItem _infoItem;
        private TextGui _levelText;
        private TextGui _expText;
        private TextGui _nameText;
        private TextGui _introText;
        private const int ItemIndex = 49;
        private bool _isShow = false;

        public new bool IsShow
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
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "panel6.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f - Width,
                0f);
            _infoItem = new DragDropItem(this,
                new Vector2(115, 75),
                60,
                75,
                null,
                new MagicGui.MagicItemData(ItemIndex));
            _infoItem.Drop += MagicGui.DropHandler;
            _infoItem.MouseStayOver += MagicGui.MouseStayOverHandler;
            _infoItem.MouseLeave += MagicGui.MouseLeaveHandler;
            _levelText = new TextGui(this,
                new Vector2(126, 224),
                80, 
                12,
                Globals.FontSize7, 
                0,
                0,
                "",
                Color.Black*0.8f);
            _expText = new TextGui(this,
                new Vector2(126, 243),
                80,
                12,
                Globals.FontSize7,
                0,
                0,
                "",
                Color.Black * 0.8f);
            _nameText = new TextGui(this,
                new Vector2(105, 256),
                200,
                20,
                Globals.FontSize12,
                0,
                0,
                "",
                new Color(88, 32, 32)*0.9f);
            _introText = new TextGui(this,
                new Vector2(75, 275),
                145,
                120,
                Globals.FontSize10,
                2,
                0,
                "",
                new Color(47, 32, 88)*0.9f);
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
                _infoItem.BaseTexture = MagicListManager.GetTexture(ItemIndex);
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