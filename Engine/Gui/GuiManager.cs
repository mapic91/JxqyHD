using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public static class GuiManager
    {
        private static MagicGui MagicInterface;
        private static GoodsGui GoodsInterface;
        private static MemoGui MemoInterface;
        private static BottomGui BottomInterface;
        private static TopGui TopInterface;
        public static bool IsMouseStateEated;
        public static DragDropItem DragDropSourceItem;
        public static bool IsDropped;

        public static void Starting()
        {
            TopInterface = new TopGui();
            BottomInterface = new BottomGui();
            MagicInterface = new MagicGui();
            GoodsInterface = new GoodsGui();
            MemoInterface = new MemoGui();
            MagicListManager.RenewList();
            GoodsListManager.RenewList();
        }

        public static void Load(string magicListPath, string goodsListPath, string memoListPath)
        {
            MagicListManager.LoadList(magicListPath);
            GoodsListManager.LoadList(goodsListPath);
            MemoListManager.LoadList(memoListPath);
        }

        public static void ToggleMagicGuiShow()
        {
            if (MagicInterface.IsShow)
                MagicInterface.IsShow = false;
            else
            {
                MagicInterface.IsShow = true;
                GoodsInterface.IsShow = false;
                MemoInterface.IsShow = false;
            }
        }

        public static void ToggleGoodsGuiShow()
        {
            if (GoodsInterface.IsShow)
                GoodsInterface.IsShow = false;
            else
            {
                GoodsInterface.IsShow = true;
                MagicInterface.IsShow = false;
                MemoInterface.IsShow = false;
            }
        }

        public static void ToggleMemoGuiShow()
        {
            if (MemoInterface.IsShow)
                MemoInterface.IsShow = false;
            else
            {
                MemoInterface.IsShow = true;
                GoodsInterface.IsShow = false;
                MagicInterface.IsShow = false;
            }
        }

        public static void UpdateMagicView()
        {
            MagicInterface.UpdateItems();
            BottomInterface.UpdateMagicItems();
        }

        public static void UpdateGoodsView()
        {
            GoodsInterface.UpdateItems();
            BottomInterface.UpdateGoodsItems();
        }

        public static void UpdateMemoView()
        {
            MemoInterface.UpdateTextShow();
        }

        public static void Update(GameTime gameTime)
        {
            IsMouseStateEated = false;
            TopInterface.Update(gameTime);
            BottomInterface.Update(gameTime);
            MagicInterface.Update(gameTime);
            GoodsInterface.Update(gameTime);
            MemoInterface.Update(gameTime);

            if (IsDropped)
            {
                IsDropped = false;
                if(DragDropSourceItem != null)
                    DragDropSourceItem.IsShow = true;
                DragDropSourceItem = null;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            TopInterface.Draw(spriteBatch);
            BottomInterface.Draw(spriteBatch);
            MagicInterface.Draw(spriteBatch);
            GoodsInterface.Draw(spriteBatch);
            MemoInterface.Draw(spriteBatch);
        }
    }
}