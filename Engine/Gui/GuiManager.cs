using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public static class GuiManager
    {
        private static SoundEffect _dropSound;
        private static SoundEffect _interfaceShow;
        private static SoundEffect _interfaceMiss;
        private static LinkedList<GuiItem> _allGuiItems = new LinkedList<GuiItem>(); 

        public static MagicGui MagicInterface;
        public static XiuLianGui XiuLianInterface;
        public static GoodsGui GoodsInterface;
        public static MemoGui MemoInterface;
        public static StateGui StateInterface;
        public static EquipGui EquipInterface;
        public static BottomGui BottomInterface;
        public static ColumnGui ColumnInterface;
        public static TopGui TopInterface;

        public static ToolTipGui ToolTipInterface;
        public static MessageGui MessageInterface;
        public static MouseGui MouseInterface;

        public static bool IsMouseStateEated;
        public static DragDropItem DragDropSourceItem;
        public static Texture DragDropSourceTexture;
        public static bool IsDropped;

        public static void Starting()
        {
            _dropSound = Utils.GetSoundEffect("界-拖放.wav");
            _interfaceShow = Utils.GetSoundEffect("界-弹出菜单.wav");
            _interfaceMiss = Utils.GetSoundEffect("界-缩回菜单.wav");

            TopInterface = new TopGui();
            _allGuiItems.AddLast(TopInterface);
            BottomInterface = new BottomGui();
            _allGuiItems.AddLast(BottomInterface);
            ColumnInterface = new ColumnGui();
            _allGuiItems.AddLast(ColumnInterface);
            MagicInterface = new MagicGui();
            _allGuiItems.AddLast(MagicInterface);
            XiuLianInterface = new XiuLianGui();
            _allGuiItems.AddLast(XiuLianInterface);
            GoodsInterface = new GoodsGui();
            _allGuiItems.AddLast(GoodsInterface);
            MemoInterface = new MemoGui();
            _allGuiItems.AddLast(MemoInterface);
            StateInterface = new StateGui();
            _allGuiItems.AddLast(StateInterface);
            EquipInterface = new EquipGui();
            _allGuiItems.AddLast(EquipInterface);
            ToolTipInterface = new ToolTipGui();
            _allGuiItems.AddLast(ToolTipInterface);
            MouseInterface = new MouseGui();
            MessageInterface = new MessageGui();
            _allGuiItems.AddLast(MessageInterface);

            MagicListManager.RenewList();
            GoodsListManager.RenewList();
        }

        public static void Load(string magicListPath, string goodsListPath, string memoListPath)
        {
            MagicListManager.LoadList(magicListPath);
            GoodsListManager.LoadList(goodsListPath);
            MemoListManager.LoadList(memoListPath);
        }

        public static void PlayInterfaceShowMissSound(bool isShow)
        {
            if (isShow) _interfaceMiss.Play();
            else _interfaceShow.Play();
        }

        public static void ToggleMagicGuiShow()
        {
            PlayInterfaceShowMissSound(MagicInterface.IsShow);
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
            PlayInterfaceShowMissSound(GoodsInterface.IsShow);
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
            PlayInterfaceShowMissSound(MemoInterface.IsShow);
            if (MemoInterface.IsShow)
                MemoInterface.IsShow = false;
            else
            {
                MemoInterface.IsShow = true;
                GoodsInterface.IsShow = false;
                MagicInterface.IsShow = false;
            }
        }

        public static void ToggleXiuLianGuiShow()
        {
            PlayInterfaceShowMissSound(XiuLianInterface.IsShow);
            if (XiuLianInterface.IsShow)
                XiuLianInterface.IsShow = false;
            else
            {
                XiuLianInterface.IsShow = true;
                StateInterface.IsShow = false;
                EquipInterface.IsShow = false;
            }
        }

        public static void ToggleStateGuiShow()
        {
            PlayInterfaceShowMissSound(StateInterface.IsShow);
            if (StateInterface.IsShow)
                StateInterface.IsShow = false;
            else
            {
                StateInterface.IsShow = true;
                XiuLianInterface.IsShow = false;
                EquipInterface.IsShow = false;
            }
        }

        public static void ToggleEquipGuiShow()
        {
            PlayInterfaceShowMissSound(StateInterface.IsShow);
            if (EquipInterface.IsShow)
                EquipInterface.IsShow = false;
            else
            {
                EquipInterface.IsShow = true;
                XiuLianInterface.IsShow = false;
                StateInterface.IsShow = false;
            }
        }

        public static void UpdateMagicView()
        {
            MagicInterface.UpdateItems();
            BottomInterface.UpdateMagicItems();
            XiuLianInterface.UpdateItem();
        }

        public static void UpdateGoodsView()
        {
            GoodsInterface.UpdateItems();
            BottomInterface.UpdateGoodsItems();
            EquipInterface.UpdateItems();
        }

        public static void UpdateGoodItemView(int listIndex)
        {
            if (GoodsListManager.IndexInStoreRange(listIndex))
            {
                GoodsInterface.UpdateListItem(listIndex);
            }
            else if (GoodsListManager.IndexInBottomGoodsRange(listIndex))
            {
                BottomInterface.UpdateGoodItem(listIndex);
            }
        }

        public static void UpdateMemoView()
        {
            MemoInterface.UpdateTextShow();
        }

        public static void Show(bool isShow = true)
        {
            foreach (var item in _allGuiItems)
            {
                item.IsShow = isShow;
            }
        }

        public static void ShowMessage(string message)
        {
            MessageInterface.ShowMessage(message);
        }

        public static void Update(GameTime gameTime)
        {
            IsMouseStateEated = false;
            TopInterface.Update(gameTime);
            BottomInterface.Update(gameTime);
            ColumnInterface.Update(gameTime);
            MagicInterface.Update(gameTime);
            XiuLianInterface.Update(gameTime);
            GoodsInterface.Update(gameTime);
            MemoInterface.Update(gameTime);
            StateInterface.Update(gameTime);
            EquipInterface.Update(gameTime);
            ToolTipInterface.Update(gameTime);

            MouseInterface.Update(gameTime);

            MessageInterface.Update(gameTime);

            if (IsDropped)
            {
                IsDropped = false;
                if (DragDropSourceItem != null)
                {
                    DragDropSourceItem.IsShow = true;             
                }
                if (DragDropSourceTexture != null &&
                    DragDropSourceTexture.Data != null)
                {
                    _dropSound.Play();
                }
                DragDropSourceItem = null;
                DragDropSourceTexture = null;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            TopInterface.Draw(spriteBatch);
            BottomInterface.Draw(spriteBatch);
            ColumnInterface.Draw(spriteBatch);
            MagicInterface.Draw(spriteBatch);
            XiuLianInterface.Draw(spriteBatch);
            GoodsInterface.Draw(spriteBatch);
            MemoInterface.Draw(spriteBatch);
            StateInterface.Draw(spriteBatch);
            EquipInterface.Draw(spriteBatch);
            ToolTipInterface.Draw(spriteBatch);

            MouseInterface.Draw(spriteBatch);

            MessageInterface.Draw(spriteBatch);
        }
    }
}