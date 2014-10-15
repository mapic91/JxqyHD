using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            if (magic.FlyingSound != null)
                magic.FlyingSound.Play(Globals.SoundEffectVolume, 0f, 0f);
            switch (magic.MoveKind)
            {
                case 1:
                    AddFixedPositionMagicSprite(user, magic, destination, true);
                    break;
                case 2:
                    AddMagicSprite(GetMoveMagicSprite(user, magic, origin, destination, false));
                    break;
                case 3:
                    AddLineMoveMagicSprite(user, magic, origin, destination, false);
                    break;
                case 4:
                    AddCircleMoveMagicSprite(user, magic, origin, false);
                    break;
                case 5:
                    AddHeartMoveMagicSprite(user, magic, origin, false);
                    break;
                case 6:
                    AddSpiralMoveMagicSprite(user, magic, origin, destination, false);
                    break;
                case 7:
                    AddSectorMoveMagicSprite(user, magic, origin, destination, false);
                    break;
                case 8:
                    AddRandomSectorMoveMagicSprite(user, magic, origin, destination, false);
                    break;
            }
        }

        private static MagicSprite GetMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd, float speedRatio = 1f)
        {
            var speed = (int)(Globals.MagicBasespeed * magic.Speed * speedRatio);
            return new MagicSprite(
                magic,
                user,
                origin,
                speed,
                destination - origin,
                destroyOnEnd);
        }

        private static MagicSprite GetMoveMagicSpriteOnDirection(Character user, Magic magic, Vector2 origin, Vector2 direction, bool destroyOnEnd, float speedRatio = 1f)
        {
            var speed = (int)(Globals.MagicBasespeed * magic.Speed * speedRatio);
            return new MagicSprite(
                magic,
                user,
                origin,
                speed,
                direction,
                destroyOnEnd);
        }

        private static void AddFixedPositionMagicSprite(Character user, Magic magic, Vector2 destination, bool destroyOnEnd)
        {
            var sprite = new MagicSprite(
                magic,
                user,
                destination,
                0,
                destroyOnEnd);
            AddMagicSprite(sprite);
        }

        private static void AddMagicSprite(MagicSprite sprite)
        {
            if (sprite.BelongCharacter.IsPlayer) AddPlayerMagicSprite(sprite);
            else AddNpcMagicSprite(sprite);
        }

        private static void AddLineMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd)
        {
            AddMagicSprite(GetMoveMagicSprite(user, magic, origin, destination, destroyOnEnd));
            for (var i = 1; i < magic.CurrentLevel; i++)
            {
                const float magicDelayMilliseconds = 60f;
                _workList.AddLast(new WorkItem(
                    magicDelayMilliseconds * i,
                    GetMoveMagicSprite(user, magic, origin, destination, destroyOnEnd)));
            }
        }

        private static void AddCircleMoveMagicSprite(Character user, Magic magic, Vector2 origin, bool destroyOnEnd)
        {
            var list = Utils.GetDirection32List();
            foreach (var dir in list)
            {
                var speed = Globals.MagicBasespeed*magic.Speed;
                speed = (int) (speed*(1 - 0.5*Math.Abs(dir.Y)));
                var sprite = new MagicSprite(magic, 
                    user, 
                    origin, 
                    speed, 
                    dir, 
                    destroyOnEnd);
                AddMagicSprite(sprite);
            }
        }

        private static void AddHeartMoveMagicSprite(Character user, Magic magic, Vector2 origin, bool destroyOnEnd)
        {
            var list = Utils.GetDirection32List();
            const float delayTime = 30;
            //First one
            var direction = list[0];
            var speedRatio = 1f - 0.5f*Math.Abs(direction.Y);
            AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio));
            for (var i = 1; i < 1 + 15; i++)
            {
                var delay = i*delayTime;
                direction = list[i];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                _workList.AddLast(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
                direction = list[32 - i];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                _workList.AddLast(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            }
            direction = list[16];
            speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
            _workList.AddLast(new WorkItem(16*delayTime,
                GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            //Second
            _workList.AddLast(new WorkItem(17 * delayTime,
                GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            for (var j = 15; j > 0; j--)
            {
                var delay = (18 + 15 - j) * delayTime;
                direction = list[j];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                _workList.AddLast(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
                direction = list[32 - j];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                _workList.AddLast(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            }
            _workList.AddLast(new WorkItem((18 + 15) * delayTime,
                GetMoveMagicSpriteOnDirection(user, magic, origin, list[0], destroyOnEnd, 1f - 0.5f * Math.Abs(list[0].Y))));
        }

        private static void AddSpiralMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            var direction = destination - origin;
            var directionIndex = Utils.GetDirection(direction, 32);
            var list = Utils.GetDirection32List();
            AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, list[directionIndex], false));
            const float magicDelayMilliseconds = 30f;
            for (var i = 1; i < 32; i++)
            {
                var dir = (directionIndex + i)%32;
                var delay = i*magicDelayMilliseconds;
                _workList.AddLast(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, list[dir], false)));
            }
        }

        private static void AddSectorMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            var direction = destination - origin;
            var directionIndex = Utils.GetDirection(direction, 8);
            directionIndex = 4*directionIndex;
            var list = Utils.GetDirection32List();
            var count = 1;
            if (magic.CurrentLevel > 0)
            {
                count += (magic.CurrentLevel - 1) / 3;
            }
            direction = list[directionIndex];
            var speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
            AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, false, speedRatio));
            for (var i = 1; i <= count; i++)
            {
                direction = list[(directionIndex + i)%32];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, false, speedRatio));
                direction = list[(directionIndex + 32 - i)%32];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, false, speedRatio));
            }
        }

        private static void AddRandomSectorMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            const float magicDelayMilliseconds = 80f;
            var raandom = new Random();

            var direction = destination - origin;
            var directionIndex = Utils.GetDirection(direction, 8);
            directionIndex = 4 * directionIndex;
            var list = Utils.GetDirection32List();
            var count = 1;
            if (magic.CurrentLevel > 0)
            {
                count += (magic.CurrentLevel - 1) / 3;
            }
            direction = list[directionIndex];
            var speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
            var sprite = GetMoveMagicSpriteOnDirection(user, magic, origin, direction, false, speedRatio);
            if (raandom.Next(2) == 0)
                AddMagicSprite(sprite);
            else
                _workList.AddLast(new WorkItem(magicDelayMilliseconds, sprite));
            for (var i = 1; i <= count; i++)
            {
                direction = list[(directionIndex + i) % 32];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                sprite = GetMoveMagicSpriteOnDirection(user, magic, origin, direction, false, speedRatio);
                if (raandom.Next(2) == 0)
                    AddMagicSprite(sprite);
                else
                    _workList.AddLast(new WorkItem(magicDelayMilliseconds, sprite));
                direction = list[(directionIndex + 32 - i) % 32];
                speedRatio = 1f - 0.5f * Math.Abs(direction.Y);
                sprite = GetMoveMagicSpriteOnDirection(user, magic, origin, direction, false, speedRatio);
                if (raandom.Next(2) == 0)
                    AddMagicSprite(sprite);
                else
                    _workList.AddLast(new WorkItem(magicDelayMilliseconds, sprite));
            }
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
                    if (item.TheSprite.BelongCharacter.IsPlayer)
                        _playerSprites.AddLast(item.TheSprite);
                    else _npcSprites.AddLast(item.TheSprite);
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
            public MagicSprite TheSprite;

            public WorkItem(float leftMilliseconds, MagicSprite theSprite)
            {
                LeftMilliseconds = leftMilliseconds;
                TheSprite = theSprite;
            }
        }
    }
}
