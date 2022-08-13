using System;
using System.Collections.Generic;
using System.Text;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{

    public class SystemMsgGui : GuiItem
    {
        public class Item
        {
            public string Msg;
            public float LeftSeconds;

            public Item(string msg, float leftSeconds)
            {
                Msg = msg;
                LeftSeconds = leftSeconds;
            }
        }

        private LinkedList<Item> _msgs = new LinkedList<Item>();
        private static int MaxItem = 15;
        private TextGui _message;

        public SystemMsgGui()
        {
            CanEatMouseState = false;
            Width = Globals.WindowWidth - (int)ScreenPosition.X;
            Height = 300;
            Position = new Vector2(50, Globals.WindowHeight - Height - 50);
            _message = new TextGui(this,
                new Vector2(0, 0),
                Globals.WindowWidth - (int)ScreenPosition.X,
                Height,
                Globals.FontSize10,
                0,
                3,
                "",
                Color.White);
            _message.CanEatMouseState = false;
        }

        public void ShowMsg(string msg, float stayMilliSecond)
        {
            if (_msgs.Count >= MaxItem)
            {
                _msgs.Remove(_msgs.First);
            }
            _msgs.AddLast(new Item(msg, stayMilliSecond / 1000));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (var node = _msgs.First; node != null;)
            {
                var next = node.Next;

                node.Value.LeftSeconds -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (node.Value.LeftSeconds <= 0)
                {
                    _msgs.Remove(node);
                }

                node = next;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);

            if (_msgs.Count > 0)
            {
                var msg = new StringBuilder();
                for (var i = 0; i < MaxItem - _msgs.Count; i++)
                {
                    msg.AppendLine();
                }
                for (var node = _msgs.First; node != null; node = node.Next)
                {
                    msg.AppendLine("<color=Default>" + node.Value.Msg);
                }

                _message.Text = msg.ToString();
                _message.Draw(spriteBatch);
            }
            
        }
    }

}