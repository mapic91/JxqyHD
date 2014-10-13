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
        private static LinkedList<WorkItem> _workList = new LinkedList<WorkItem>();

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

        public static List<MagicSprite> GetMagicSpritesInView()
        {
            var viewRegion = Globals.TheCarmera.CarmerRegionInWorld;
            var list = new List<MagicSprite>(_npcSprites.Count + _playerSprites.Count);
            foreach (var sprite in _npcSprites)
            {
                if (viewRegion.Intersects(sprite.RegionInWorld))
                    list.Add(sprite);
            }
            foreach (var sprite in _playerSprites)
            {
                if (viewRegion.Intersects(sprite.RegionInWorld))
                    list.Add(sprite);
            }
            return list;
        }

        public static void UseMagic(Character user, Magic magic, Vector2 origin, Vector2 destination)
        {
            if (user == null || magic == null) return;
            switch (magic.MoveKind)
            {
                case 1:
                    AddFixedPositionMagicSprite(user, magic, destination, true);
                    break;
                case 2:
                    AddMoveMagicSprite(user, magic, origin, destination, false);
                    break;
                case 3:
                    AddMoveMagicSprite(user, magic, origin, destination, false);
                    for (var i = 1; i < magic.CurrentLevel; i++)
                    {
                        _workList.AddLast(new WorkItem(Globals.MagicDelayMilliseconds*i, user, magic, origin, destination, null,
                            SpriteType.Moveing, false));
                    }
                    break;
            }
        }

        public static void AddFixedPositionMagicSprite(Character user, Magic magic, Vector2 destination, bool destroyOnEnd)
        {
            if (user == null || magic == null) return;
            AddPlayerMagicSprite(new MagicSprite(
                magic,
                user,
                destination,
                0,
                destroyOnEnd));
        }

        public static void AddMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd)
        {
            if (user == null || magic == null) return;
            AddPlayerMagicSprite(new MagicSprite(
                magic,
                user,
                origin,
                Globals.MagicBasespeed * magic.Speed,
                destination - origin,
                destroyOnEnd));
        }

        public static void Update(GameTime gameTime)
        {
            var elapsedMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            for (var node = _npcSprites.First; node != null; )
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

            for (var node = _playerSprites.First; node != null; )
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

            for (var node = _workList.First; node != null; )
            {
                var item = node.Value;
                var next = node.Next;
                item.LeftMilliseconds -= elapsedMilliseconds;
                if (item.LeftMilliseconds <= 0)
                {
                    switch (item.Type)
                    {
                        case SpriteType.FixedPosition:
                            AddFixedPositionMagicSprite(item.TheUser, item.TheMagic, item.Destination, item.DestroyOnEnd);
                            break;
                        case SpriteType.Moveing:
                            AddMoveMagicSprite(item.TheUser, item.TheMagic, item.Orgion, item.Destination, item.DestroyOnEnd);
                            break;
                    }
                    _workList.Remove(node);
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

        class WorkItem
        {
            public float LeftMilliseconds;
            public Character TheUser;
            public Magic TheMagic;
            public Vector2 Orgion;
            public Vector2 Destination;
            public LinkedList<Vector2> Path;
            public SpriteType Type;
            public bool DestroyOnEnd;

            public WorkItem(float leftMilliseconds,
                Character user,
                Magic magic,
                Vector2 orgion,
                Vector2 destination,
                LinkedList<Vector2> path,
                SpriteType type,
                bool destroyOnEnd)
            {
                LeftMilliseconds = leftMilliseconds;
                TheUser = user;
                TheMagic = magic;
                Orgion = orgion;
                Destination = destination;
                Path = path;
                Type = type;
                DestroyOnEnd = destroyOnEnd;
            }
        }

        enum SpriteType
        {
            FixedPosition,
            Moveing
        }
    }
}
