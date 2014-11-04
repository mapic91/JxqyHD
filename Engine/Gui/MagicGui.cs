using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class MagicGui
    {
        private ListView _magicList;
        private bool _isShow = true;

        public bool IsShow
        {
            get { return _isShow; }
            set { _isShow = value; }
        }

        public MagicGui()
        {
            var magicListBaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\panel2.asf"));
            var magicListPosition = new Vector2(
                Globals.WindowWidth/2f,
                0f);
            _magicList = new ListView(null,
                magicListPosition,
                magicListBaseTexture.Width,
                magicListBaseTexture.Height,
                magicListBaseTexture,
                12);
            _magicList.Scrolled += delegate
            {
                UpdateItems();
            };
            _magicList.RegisterItemDragHandler((arg1, arg2) =>
            {
                var item = (DragDropItem) arg1;
                item.BaseTexture = null;
            });
            _magicList.RegisterItemDropHandler((object arg1, DragDropItem.DropEvent arg2) =>
            {
                var data = (MagicItemData)(((DragDropItem)arg1).Data);
                var sourceData = (MagicItemData) arg2.Source.Data;
                MagicListManager.ExchangeMagicListItem(data.Index, sourceData.Index);
            });
            //_magicList.RegisterItemMouseRightClickeHandler((arg1, arg2) =>
            //{
            //    var data = (MagicItemData)(((DragDropItem)arg1).Data);
            //    var info = GuiManager.GetMagicItemInfo(data.Index);
            //    if (info != null)
            //    {
            //        Globals.ThePlayer.CurrentMagicInUse = info;
            //    } 
            //});
        }

        public void UpdateItems()
        {
            for (var i = 0; i < 9; i++)
            {
                var index = _magicList.ToListIndex(i);
                var magic = MagicListManager.GetMagic(index);
                var image = magic == null ? null : magic.Image;
                _magicList.SetListItem(i, new Texture(image), new MagicItemData(index));
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsShow) return;

            _magicList.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;

            _magicList.Draw(spriteBatch);
        }

        public class MagicItemData
        {
            public int Index { private set; get; }

            public MagicItemData(int index)
            {
                Index = index;
            }
        }
    }
}