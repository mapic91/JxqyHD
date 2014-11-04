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
            MagicListManager.RenewMagicList();
        }

        public static void Load(string magicListPath)
        {
            MagicListManager.LoadMagicList(magicListPath);
        }
        public static void Update(GameTime gameTime)
        {
            IsMouseStateEated = false;
            TopInterface.Update(gameTime);
            BottomInterface.Update(gameTime);
            MagicInterface.Update(gameTime);

            if (IsDropped)
            {
                IsDropped = false;
                DragDropSourceItem = null;
                UpdateMagicView();
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            TopInterface.Draw(spriteBatch);
            BottomInterface.Draw(spriteBatch);
            MagicInterface.Draw(spriteBatch);
        }

        public static void UpdateMagicView()
        {
            MagicInterface.UpdateItems();
            BottomInterface.UpdateMagicItems();
        }
    }
}