using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class MagicManager
    {
        private static LinkedList<MagicSprite> _npcSprites = new LinkedList<MagicSprite>();
        private static LinkedList<MagicSprite> _playerSprites = new LinkedList<MagicSprite>();

        public static void AddNpcMagicSprite(MagicSprite magicSprite)
        {
            if (magicSprite == null) return;
            _npcSprites.AddLast(magicSprite);
        }

        public static void AddPlayerMagicSprite(MagicSprite magicSprite)
        {
            if (magicSprite == null) return;
            _playerSprites.AddLast(magicSprite);
        }

        public static void Update(GameTime gameTime)
        {
            for (var node = _npcSprites.First; node != null;)
            {
                var sprite = node.Value;
                var next = node.Next;
                if (sprite.IsDestroyed)
                {
                    _npcSprites.Remove(node);
                }
                else
                {
                    sprite.Update(gameTime);
                }
                node = next;
            }

            for (var node = _playerSprites.First; node != null;)
            {
                var sprite = node.Value;
                var next = node.Next;
                if (sprite.IsDestroyed)
                {
                    _playerSprites.Remove(node);
                }
                else
                {
                    sprite.Update(gameTime);
                }
                node = next;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var sprite in _npcSprites)
            {
                sprite.Draw(spriteBatch);
            }
            foreach (var sprite in _playerSprites)
            {
                sprite.Draw(spriteBatch);
            }
        }
    }
}
