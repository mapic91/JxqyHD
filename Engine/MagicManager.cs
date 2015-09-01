using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class MagicManager
    {
        private static LinkedList<MagicSprite> _magicSprites = new LinkedList<MagicSprite>();
        private static LinkedList<WorkItem> _workList = new LinkedList<WorkItem>();
        private static LinkedList<Sprite> _effectSprites = new LinkedList<Sprite>(); 
        private static LinkedList<Kind19MagicInfo> _kind19Magics = new LinkedList<Kind19MagicInfo>(); 
        private static LinkedList<MagicSprite> _solideMagicSprites = new LinkedList<MagicSprite>(); 
        private static List<Sprite> _magicSpritesInView = new List<Sprite>();

        public static int MaxMagicUnit;

        public static LinkedList<MagicSprite> MagicSpritesList
        {
            get { return _magicSprites; }
        }

        public static LinkedList<WorkItem> WorkList
        {
            get { return _workList; }
        }

        public static LinkedList<Sprite> EffectSprites
        {
            get { return _effectSprites; }
        }

        public static List<Sprite> MagicSpritesInView
        {
            get { return _magicSpritesInView; }
        }

        private static List<Sprite> GetMagicSpritesInView()
        {
            var viewRegion = Globals.TheCarmera.CarmerRegionInWorld;
            var list = new List<Sprite>(_magicSprites.Count + _effectSprites.Count);
            list.AddRange(_magicSprites.Where(sprite => viewRegion.Intersects(sprite.RegionInWorld)));
            list.AddRange(_effectSprites.Where(effectSprite => viewRegion.Intersects(effectSprite.RegionInWorld)));
            return list;
        }

        private static void RemoveMagicSprite(LinkedListNode<MagicSprite> node)
        {
            _magicSprites.Remove(node);
        }

        private static void AddWorkItem(WorkItem item)
        {
            if (item.LeftMilliseconds < 1)
                AddMagicSprite(item.TheSprite);
            else _workList.AddLast(item);
        }

        private static Vector2 GetDirectionOffsetOf8(Vector2 direction)
        {
            var directionIndex = Utils.GetDirectionIndex(direction, 8);
            switch (directionIndex)
            {
                case 0:
                case 4:
                    direction = new Vector2(64, 0);
                    break;
                case 2:
                case 6:
                    direction = new Vector2(0, 32);
                    break;
                case 1:
                case 5:
                    direction = new Vector2(32, 16);
                    break;
                case 3:
                case 7:
                    direction = new Vector2(-32, 16);
                    break;
            }
            return direction;
        }

        private static MagicSprite GetMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd, float speedRatio = 1f)
        {
            var speed = Globals.MagicBasespeed * magic.Speed * speedRatio;
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
            var speed = Globals.MagicBasespeed * magic.Speed * speedRatio;
            return new MagicSprite(
                magic,
                user,
                origin,
                speed,
                direction,
                destroyOnEnd);
        }

        private static MagicSprite GetFixedPositionMagicSprite(Character user, Magic magic, Vector2 destination, bool destroyOnEnd)
        {
            return new MagicSprite(
                magic,
                user,
                destination,
                0,
                destroyOnEnd);
        }

        private static void AddFixedPositionMagicSprite(Character user, Magic magic, Vector2 destination, bool destroyOnEnd)
        {
            AddMagicSprite(GetFixedPositionMagicSprite(
                user,
                magic,
                destination,
                destroyOnEnd));
        }

        private static void AddMagicSprite(MagicSprite sprite)
        {
            if (sprite != null)
            {
                if (MaxMagicUnit > 0 && _magicSprites.Count >= MaxMagicUnit)
                {
                    switch (sprite.BelongMagic.MoveKind)
                    {
                        case 13:
                        case 20:
                        case 21:
                            //Can or must skip those
                            break;
                        default:
                            //Restrict max magic sprites for performence
                            return;
                    }   
                }

                _magicSprites.AddLast(sprite);

                if (sprite.BelongMagic.Solid > 0)
                {
                    _solideMagicSprites.AddLast(sprite);
                }
            }
        }

        private static void AddLineMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd)
        {
            var speedRatio = GetSpeedRatio(destination - origin);
            var level = magic.CurrentLevel < 1 ? 1 : magic.CurrentLevel;
            for (var i = 0; i < level; i++)
            {
                const float magicDelayMilliseconds = 60f;
                AddWorkItem(new WorkItem(
                    magicDelayMilliseconds * i,
                    GetMoveMagicSprite(user, magic, origin, destination, destroyOnEnd, speedRatio)));
            }
        }

        private static void AddCircleMoveMagicSprite(Character user, Magic magic, Vector2 origin, bool destroyOnEnd)
        {
            var list = Utils.GetDirection32List();
            foreach (var dir in list)
            {
                AddMagicSprite(GetMoveMagicSpriteOnDirection(
                    user,
                    magic,
                    origin,
                    dir,
                    destroyOnEnd,
                    GetSpeedRatio(dir.Y)));
            }
        }

        private static void AddHeartMoveMagicSprite(Character user, Magic magic, Vector2 origin, bool destroyOnEnd)
        {
            var list = Utils.GetDirection32List();
            const float delayTime = 30;
            Vector2 direction;
            float speedRatio;
            //First one
            for (var i = 0; i < 1 + 15; i++)
            {
                var delay = i * delayTime;
                direction = list[i];
                speedRatio = GetSpeedRatio(direction.Y);
                AddWorkItem(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
                direction = list[32 - i];
                speedRatio = GetSpeedRatio(direction.Y);
                AddWorkItem(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            }
            direction = list[16];
            speedRatio = GetSpeedRatio(direction.Y);
            AddWorkItem(new WorkItem(16 * delayTime,
                GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            //Second
            AddWorkItem(new WorkItem(17 * delayTime,
                GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            for (var j = 15; j > 0; j--)
            {
                var delay = (18 + 15 - j) * delayTime;
                direction = list[j];
                speedRatio = GetSpeedRatio(direction.Y);
                AddWorkItem(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
                direction = list[32 - j];
                speedRatio = GetSpeedRatio(direction.Y);
                AddWorkItem(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio)));
            }
            AddWorkItem(new WorkItem((18 + 15) * delayTime,
                GetMoveMagicSpriteOnDirection(user, magic, origin, list[0], destroyOnEnd, 1f - 0.5f * Math.Abs(list[0].Y))));
        }

        private static void AddSpiralMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            var direction = destination - origin;
            var directionIndex = Utils.GetDirectionIndex(direction, 32);
            var list = Utils.GetDirection32List();
            const float magicDelayMilliseconds = 30f;
            for (var i = 0; i < 32; i++)
            {
                var dir = (directionIndex + i) % 32;
                var delay = i * magicDelayMilliseconds;
                AddWorkItem(new WorkItem(delay,
                    GetMoveMagicSpriteOnDirection(user, magic, origin, list[dir], destroyOnEnd, GetSpeedRatio(list[dir].Y))));
            }
        }

        private static void AddSectorMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            var direction = destination - origin;
            var directionIndex = Utils.GetDirectionIndex(direction, 8);
            directionIndex = 4 * directionIndex;//8 to 32
            var list = Utils.GetDirection32List();
            var count = 1;
            if (magic.CurrentLevel > 0)
            {
                count += (magic.CurrentLevel - 1) / 3;
            }
            direction = list[directionIndex];
            var speedRatio = GetSpeedRatio(direction.Y);
            AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio));
            for (var i = 1; i <= count; i++)
            {
                direction = list[(directionIndex + i * 2) % 32];
                speedRatio = GetSpeedRatio(direction.Y);
                AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio));
                direction = list[(directionIndex + 32 - i * 2) % 32];
                speedRatio = GetSpeedRatio(direction.Y);
                AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio));
            }
        }

        private static void AddRandomSectorMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            const float magicDelayMilliseconds = 80f;
            var raandom = new Random();

            var direction = destination - origin;
            var directionIndex = Utils.GetDirectionIndex(direction, 8);
            directionIndex = 4 * directionIndex;//8 to 32
            var list = Utils.GetDirection32List();
            var count = 1;
            if (magic.CurrentLevel > 0)
            {
                count += (magic.CurrentLevel - 1) / 3;
            }
            direction = list[directionIndex];
            var speedRatio = GetSpeedRatio(direction.Y);
            var sprite = GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio);
            AddWorkItem(new WorkItem(raandom.Next(2) * magicDelayMilliseconds, sprite));
            for (var i = 1; i <= count; i++)
            {
                direction = list[(directionIndex + i * 2) % 32];
                speedRatio = GetSpeedRatio(direction.Y);
                sprite = GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio);
                AddWorkItem(new WorkItem(raandom.Next(2) * magicDelayMilliseconds, sprite));
                direction = list[(directionIndex + 32 - i * 2) % 32];
                speedRatio = GetSpeedRatio(direction.Y);
                sprite = GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio);
                AddWorkItem(new WorkItem(raandom.Next(2) * magicDelayMilliseconds, sprite));
            }
        }

        private static void AddFixedWallMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            var offset = GetDirectionOffsetOf8(destination - origin);

            var count = 3;
            if (magic.CurrentLevel > 1) count += (magic.CurrentLevel - 1) * 2;
            AddFixedWallMagicSprite(user, magic, destination, offset, count, destroyOnEnd);
        }

        private static void AddFixedWallMagicSprite(Character user, Magic magic, Vector2 wallMiddle, Vector2 offset,
            int count, bool destroyOnEnd)
        {
            count = (count - 1) / 2;
            AddMagicSprite(GetFixedPositionMagicSprite(user, magic, wallMiddle, destroyOnEnd));
            for (var i = 1; i <= count; i++)
            {
                var position = wallMiddle + i * offset;
                AddMagicSprite(GetFixedPositionMagicSprite(user, magic, position, destroyOnEnd));
                position = wallMiddle - i * offset;
                AddMagicSprite(GetFixedPositionMagicSprite(user, magic, position, destroyOnEnd));
            }
        }

        private static void AddFixedWallMagicSprite(Character user, Magic magic, Vector2 wallMiddle, Vector2 offset,
            int count, bool destroyOnEnd, float delay)
        {
            count = (count - 1) / 2;
            AddWorkItem(new WorkItem(delay, GetFixedPositionMagicSprite(user, magic, wallMiddle, destroyOnEnd)));
            for (var i = 1; i <= count; i++)
            {
                var position = wallMiddle + i * offset;
                AddWorkItem(new WorkItem(delay, GetFixedPositionMagicSprite(user, magic, position, destroyOnEnd)));
                position = wallMiddle - i * offset;
                AddWorkItem(new WorkItem(delay, GetFixedPositionMagicSprite(user, magic, position, destroyOnEnd)));
            }
        }

        private static void AddWallMoveMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination,
            bool destroyOnEnd)
        {
            var direction = destination - origin;
            var offset = GetDirectionOffsetOf8(direction);
            direction = Utils.GetDirection8(Utils.GetDirectionIndex(direction, 8));
            var speedRatio = GetSpeedRatio(direction);

            var count = 1;
            if (magic.CurrentLevel > 1) count += magic.CurrentLevel - 1;
            AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, origin, direction, destroyOnEnd, speedRatio));
            for (var i = 1; i <= count; i++)
            {
                var position = origin + i * offset;
                AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, position, direction, destroyOnEnd, speedRatio));
                position = origin - i * offset;
                AddMagicSprite(GetMoveMagicSpriteOnDirection(user, magic, position, direction, destroyOnEnd, speedRatio));
            }
        }

        private static void AddSquareFixedPositionMagicSprite(Character user, Magic magic, Vector2 destination, bool destroyOnEnd)
        {
            var count = 3;
            if (magic.CurrentLevel > 3) count += ((magic.CurrentLevel - 1) / 3) * 2;
            var offsetRow = new Vector2(32, 16);
            var offsetColumn = new Vector2(32, -16);
            var halfCount = count / 2;
            destination -= halfCount * offsetRow;
            for (var i = 0; i < count; i++)
            {
                AddFixedWallMagicSprite(user, magic, destination, offsetColumn, count, destroyOnEnd);
                destination += offsetRow;
            }
        }

        private static readonly List<Vector2> CrossOffset = new List<Vector2>()
        {
            new Vector2(32, 16), new Vector2(32, -16),
            new Vector2(-32, 16), new Vector2(-32, -16)
        };
        private static void AddCrossFixedPositionMagicSprite(Character user, Magic magic, Vector2 origion, bool destroyOnEnd)
        {
            var count = 3;
            if (magic.CurrentLevel > 3) count += ((magic.CurrentLevel - 1) / 3) * 2;
            const float magicDelayMilliseconds = 60f;
            for (var i = 0; i < count; i++)
            {
                var sprite0 = GetFixedPositionMagicSprite(user, magic, origion + (i + 1) * CrossOffset[0], destroyOnEnd);
                var sprite1 = GetFixedPositionMagicSprite(user, magic, origion + (i + 1) * CrossOffset[1], destroyOnEnd);
                var sprite2 = GetFixedPositionMagicSprite(user, magic, origion + (i + 1) * CrossOffset[2], destroyOnEnd);
                var sprite3 = GetFixedPositionMagicSprite(user, magic, origion + (i + 1) * CrossOffset[3], destroyOnEnd);
                AddWorkItem(new WorkItem(i * magicDelayMilliseconds, sprite0));
                AddWorkItem(new WorkItem(i * magicDelayMilliseconds, sprite1));
                AddWorkItem(new WorkItem(i * magicDelayMilliseconds, sprite2));
                AddWorkItem(new WorkItem(i * magicDelayMilliseconds, sprite3));
            }
        }

        private static void AddHorizontalFixedWallMagicSprite(Character user, Magic magic, Vector2 wallMiddle,
            int count, bool destroyOnEnd, float delay)
        {
            count = count / 2;
            var position = wallMiddle;
            AddWorkItem(new WorkItem(delay, GetFixedPositionMagicSprite(user, magic, position, destroyOnEnd)));
            var newPositionLeft = position;
            var newPositionRight = position;
            for (var i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    newPositionLeft += new Vector2(-32, -16);
                    newPositionRight += new Vector2(32, -16);
                }
                else
                {
                    newPositionLeft += new Vector2(-32, 16);
                    newPositionRight += new Vector2(32, 16);
                }
                AddWorkItem(new WorkItem(delay, GetFixedPositionMagicSprite(user, magic, newPositionLeft, destroyOnEnd)));
                AddWorkItem(new WorkItem(delay, GetFixedPositionMagicSprite(user, magic, newPositionRight, destroyOnEnd)));
            }
        }

        private static void AddRegtangleFixedPositionMagicSprite(Character user, Magic magic, Vector2 origin,
            Vector2 destination, bool destroyOnEnd)
        {
            var direction = destination - origin;
            var directionIndex = Utils.GetDirectionIndex(direction, 8);
            const int columnCount = 5;
            var count = 3;
            if (magic.CurrentLevel > 3) count += ((magic.CurrentLevel - 1) / 3) * 2;
            const float magicDelayMilliseconds = 60f;
            switch (directionIndex)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                    {
                        Vector2 beginPosition = origin,
                            offsetColumn = new Vector2(),
                            offsetRow = new Vector2();
                        switch (directionIndex)
                        {
                            case 1:
                                offsetColumn = new Vector2(32, 16);
                                offsetRow = new Vector2(-32, 16);
                                break;
                            case 3:
                                offsetColumn = new Vector2(32, -16);
                                offsetRow = new Vector2(-32, -16);
                                break;
                            case 5:
                                offsetColumn = new Vector2(32, 16);
                                offsetRow = new Vector2(32, -16);
                                break;
                            case 7:
                                offsetColumn = new Vector2(32, -16);
                                offsetRow = new Vector2(32, 16);
                                break;
                        }
                        for (var i = 0; i < count; i++)
                        {
                            beginPosition += offsetRow;
                            AddFixedWallMagicSprite(user, magic, beginPosition, offsetColumn, columnCount, destroyOnEnd, i * magicDelayMilliseconds);
                        }
                    }
                    break;
                case 0:
                case 4:
                    {
                        var offsetRow = new Vector2(0, -32);
                        if (directionIndex == 0)
                        {
                            offsetRow = new Vector2(0, 32);
                        }
                        var beginPosition = origin;
                        for (var i = 0; i < count; i++)
                        {
                            beginPosition += offsetRow;
                            AddHorizontalFixedWallMagicSprite(user, magic, beginPosition, columnCount, destroyOnEnd, i * magicDelayMilliseconds);
                        }
                    }
                    break;
                case 2:
                    {
                        var beginPostion = origin;
                        var offsetColumn = new Vector2(0, 32);
                        for (var i = 0; i < count; i++)
                        {
                            if (i % 2 == 0) beginPostion += new Vector2(-32, -16);
                            else beginPostion += new Vector2(-32, 16);
                            AddFixedWallMagicSprite(user, magic, beginPostion, offsetColumn, columnCount, destroyOnEnd, i * magicDelayMilliseconds);
                        }
                    }
                    break;
                case 6:
                    {
                        var beginPostion = origin;
                        var offsetColumn = new Vector2(0, 32);
                        for (var i = 0; i < count; i++)
                        {
                            if (i % 2 == 0) beginPostion += new Vector2(32, 16);
                            else beginPostion += new Vector2(32, -16);
                            AddFixedWallMagicSprite(user, magic, beginPostion, offsetColumn, columnCount, destroyOnEnd, i * magicDelayMilliseconds);
                        }
                    }
                    break;
            }
        }

        private static List<Vector2> _rowOffsetOf8 = new List<Vector2>()
        {
            new Vector2(0, 32),  new Vector2(-32, 16),
            new Vector2(-64, 0), new Vector2(-32, -16),
            new Vector2(0, -32), new Vector2(32, -16),
            new Vector2(64, 0), new Vector2(32, 16)
        };
        private static List<Vector2> _columOffsetOf8 = new List<Vector2>()
        {
            new Vector2(64, 0),  new Vector2(-32, -16),
            new Vector2(0, 32), new Vector2(-32, 16),
            new Vector2(64, 0), new Vector2(32, 16),
            new Vector2(0, 32), new Vector2(32, -16)
        };
        private static void AddIsoscelesTriangleMagicSprite(Character user, Magic magic, Vector2 origin,
            Vector2 destination, bool destroyOnEnd)
        {
            var direction = destination - origin;
            var directionIndex = Utils.GetDirectionIndex(direction, 8);
            var rowOffset = _rowOffsetOf8[directionIndex];
            var columnOffset = _columOffsetOf8[directionIndex];
            var count = 3;
            if (magic.CurrentLevel > 3) count += ((magic.CurrentLevel - 1) / 3) * 2;
            const float magicDelayMilliseconds = 60f;
            var beginPosition = origin;
            for (var i = 0; i < count; i++)
            {
                beginPosition += rowOffset;
                AddFixedWallMagicSprite(user, magic, beginPosition, columnOffset, 1 + i * 2, destroyOnEnd, i * magicDelayMilliseconds);
            }
        }

        private static void AddVTypeFixedPOsitionMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd)
        {
            var direction = destination - origin;
            var directionIndex = Utils.GetDirectionIndex(direction, 8);
            var count = 3;
            if (magic.CurrentLevel > 3) count += ((magic.CurrentLevel - 1) / 3) * 2;
            var orgTile = PathFinder.FindNeighborInDirection(Map.ToTilePosition(origin), directionIndex);
            AddMagicSprite(GetFixedPositionMagicSprite(user, magic, Map.ToPixelPosition(orgTile), destroyOnEnd));
            const float magicDelayMilliseconds = 60f;
            var leftVTile = orgTile;
            var rightVTile = orgTile;
            for (var i = 1; i < count; i++)
            {
                leftVTile = PathFinder.FindNeighborInDirection(leftVTile, (directionIndex + 7)%8);
                rightVTile = PathFinder.FindNeighborInDirection(rightVTile, (directionIndex + 1) % 8);
                AddWorkItem(new WorkItem(i*magicDelayMilliseconds,
                    GetFixedPositionMagicSprite(user, magic, Map.ToPixelPosition(leftVTile), destroyOnEnd)));
                AddWorkItem(new WorkItem(i*magicDelayMilliseconds,
                    GetFixedPositionMagicSprite(user, magic, Map.ToPixelPosition(rightVTile), destroyOnEnd)));
            }
        }

        private static void AddFollowCharacterMagicSprite(Character user, Magic magic, Vector2 origin, bool destroyOnEnd, Character target)
        {
            if (target != null && user.Kind == (int) Character.CharacterKind.Player && target.IsFighterFriend)
            {
                user = target;
            }
            var sprite = GetFixedPositionMagicSprite(user, magic, origin, destroyOnEnd);
            switch (magic.SpecialKind)
            {
                case 1:
                    user.Life += (magic.Effect == 0 ? user.Attack : magic.Effect);
                    AddMagicSprite(sprite);
                    break;
                case 2:
                    user.Thew += (magic.Effect == 0 ? user.Attack : magic.Effect);
                    AddMagicSprite(sprite);
                    break;
                default:
                    {
                        MagicSprite spriteInEffect = null;
                        foreach (var item in user.MagicSpritesInEffect)
                        {
                            if (item.BelongMagic.Name == magic.Name)
                                spriteInEffect = item;
                        }
                        if (spriteInEffect != null && spriteInEffect.IsLive)
                        {
                            spriteInEffect.ResetPlay();
                        }
                        else
                        {
                            user.MagicSpritesInEffect.AddLast(sprite);
                            AddMagicSprite(sprite);
                        }
                    }
                    break;
            }
        }

        private static void AddSuperModeMagic(Character user, Magic magic, Vector2 origin, bool destroyOnEnd)
        {
            Globals.IsInSuperMagicMode = true;
            Globals.SuperModeMagicSprite = GetFixedPositionMagicSprite(user, magic, origin, destroyOnEnd);
        }

        private static void AddFollowEnemyMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd)
        {
            AddMagicSprite(GetMoveMagicSprite(user, magic, origin, destination, destroyOnEnd));
        }

        private static void AddThrowMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd)
        {
            var count = 1;
            if (magic.CurrentLevel > 1)
            {
                count += (magic.CurrentLevel - 1) / 3;
            }
            var columnOffset = new Vector2(-32, 16);
            var rowOffset = new Vector2(32, 16);
            var halfCount = count / 2;
            destination -= rowOffset * halfCount;
            for (var r = 0; r < count; r++)
            {
                var rowDestination = destination - columnOffset * halfCount;
                for (var c = 0; c < count; c++)
                {
                    AddMagicSprite(GetThrowMagicSprite(user, magic, origin, rowDestination, destroyOnEnd));
                    rowDestination += columnOffset;
                }
                destination += rowOffset;
            }
        }

        private static MagicSprite GetThrowMagicSprite(Character user, Magic magic, Vector2 origin, Vector2 destination, bool destroyOnEnd)
        {
            var sprite = new MagicSprite(magic, 
                user,
                origin,
                magic.Speed*Globals.MagicBasespeed,
                Vector2.Zero,
                destroyOnEnd);
            var path = new LinkedList<Vector2>();
            path.AddLast(origin);
            var distance = Vector2.Distance(origin, destination);
            var count = (int)distance/64;
            if (count < 4) count = 4;
            var halfCount = count / 2;
            var pathUnit = (destination - origin)/count;
            var offset = new Vector2[count - 1];
            var offsetUnit = distance/count;
            for (var i = 0; i < count - 1; i++)
            {
                if (i < halfCount - 1)
                {
                    offset[i] = new Vector2(0, -(i+1)*offsetUnit);
                }
                else
                {
                    offset[i] = new Vector2(0, -(count - i + 1)*offsetUnit);
                }
            }
            for (var i = 0; i < count - 1; i++)
            {
                path.AddLast(origin + (i + 1)*pathUnit + offset[i]);
            }
            path.AddLast(destination);
            sprite.SetPath(path);
            return sprite;
        }

        public static float GetSpeedRatio(Vector2 direction)
        {
            var speedRatio = 1f;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                speedRatio = GetSpeedRatio(direction.Y);
            }
            return speedRatio;
        }

        public static float GetSpeedRatio(float normalizedDirectionY)
        {
            return 1f - 0.5f * Math.Abs(normalizedDirectionY);
        }

        /// <summary>
        /// Clear all magic sprite.
        /// </summary>
        public static void Clear()
        {
            _magicSprites.Clear();
            _workList.Clear();
            _effectSprites.Clear();
        }

        public static int GetEffectAmount(Magic magic, Character belongCharacter)
        {
            //If magic effect not set(equal 0) use belong character attack value as amount.
            //Because npc just can use FlyIni FlyIni2, 
            //so if belong character is a npc not a player use character attack value as amount also.
            if (magic == null || belongCharacter == null) return 0;
            return (magic.Effect == 0 || !belongCharacter.IsPlayer) ?
                belongCharacter.Attack :
                magic.Effect;
        }

        /// <summary>
        /// Use magic.
        /// </summary>
        /// <param name="user">The magic user.</param>
        /// <param name="magic">Magic to use.</param>
        /// <param name="origin">Magic initial pixel postion in world.</param>
        /// <param name="destination">Magic destination pixel postiont in world.</param>
        /// <param name="target">Magic target</param>
        public static void UseMagic(Character user, Magic magic, Vector2 origin, Vector2 destination, Character target = null)
        {
            if (user == null || magic == null) return;

            if (magic.FlyingSound != null)
            {
                magic.FlyingSound.Play();
            }

            if (magic.BeginAtMouse > 0)
            {
                var dir = origin - destination;
                if (dir != Vector2.Zero)
                {
                    dir.Normalize();
                }
                origin = destination + dir;
            }
            else if (magic.BeginAtUser > 0)
            {
                destination = origin;
            }

            if (magic.MeteorMove > 0)
            {
                origin = destination;
            }

            switch (magic.MoveKind)
            {
                case 1:
                    AddFixedPositionMagicSprite(user, magic, destination, true);
                    break;
                case 2:
                    AddMagicSprite(GetMoveMagicSprite(user, magic, origin, destination, false, GetSpeedRatio(destination - origin)));
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
                case 9:
                    AddFixedWallMagicSprite(user, magic, origin, destination, true);
                    break;
                case 10:
                    AddWallMoveMagicSprite(user, magic, origin, destination, false);
                    break;
                case 11:
                    {
                        switch (magic.Region)
                        {
                            case 1:
                                AddSquareFixedPositionMagicSprite(user, magic, destination, true);
                                break;
                            case 2:
                                AddCrossFixedPositionMagicSprite(user, magic, origin, true);
                                break;
                            case 3:
                                AddRegtangleFixedPositionMagicSprite(user, magic, origin, destination, true);
                                break;
                            case 4:
                                AddIsoscelesTriangleMagicSprite(user, magic, origin, destination, true);
                                break;
                            case 5:
                                AddVTypeFixedPOsitionMagicSprite(user, magic, origin, destination, true);
                                break;
                        }
                        break;
                    }
                case 13:
                    AddFollowCharacterMagicSprite(user, magic, origin, true, target);
                    break;
                case 15:
                    AddSuperModeMagic(user, magic, origin, true);
                    break;
                case 16:
                    AddFollowEnemyMagicSprite(user, magic, origin, destination, false);
                    break;
                case 17:
                    AddThrowMagicSprite(user, magic, origin, destination, true);
                    break;
                case 18:
                    //Empty
                    break;
                case 19:
                {
                    var info = new Kind19MagicInfo(magic.KeepMilliseconds, magic, user);
                    _kind19Magics.AddLast(info);
                }
                    break;
                case 20:
                {
                    //Can't use transport magic when in transport
                    if (!user.IsInTransport)
                    {
                        user.IsInTransport = true;
                        AddFixedPositionMagicSprite(user, magic, destination, true);
                    }
                }
                    break;
                case 21:
                {
                    var player = user as Player;
                    if (player != null && 
                        target != null &&
                        !target.IsDeathInvoked && // Can't control dead people
                        target.Level <= magic.MaxLevel
                        )
                    {
                        player.ControledCharacter = target;
                        AddFixedPositionMagicSprite(user, magic, user.PositionInWorld, true);
                    }
                }
                    break;
                case 22:
                {
                    AddFixedPositionMagicSprite(user, magic, destination, true);
                }
                    break;
            }

            //Magic side effect
            if (magic.SideEffectProbability > 0 && 
                Globals.TheRandom.Next(0, 100) < magic.SideEffectProbability)
            {
                var amount = (GetEffectAmount(magic, user) * magic.SideEffectPercent) / 100;
                switch ((Magic.SideEffectDamageType)magic.SideEffectType)
                {
                    case Magic.SideEffectDamageType.Life:
                        user.DecreaseLifeAddHurt(amount);
                        break;
                    case Magic.SideEffectDamageType.Mana:
                        user.AddMana(-amount);
                        break;
                    case Magic.SideEffectDamageType.Thew:
                        user.AddThew(-amount);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static bool IsObstacle(Vector2 tilePosition)
        {
            return _solideMagicSprites.Any(sprite => sprite.TilePosition == tilePosition);
        }

        public static void Update(GameTime gameTime)
        {
            var elapsedMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            for (var node = _workList.First; node != null; )
            {
                var item = node.Value;
                var next = node.Next;
                item.LeftMilliseconds -= elapsedMilliseconds;
                if (item.LeftMilliseconds <= 0)
                {
                    AddMagicSprite(item.TheSprite);
                    _workList.Remove(node);
                }
                node = next;
            }

            for (var node = _magicSprites.First; node != null; )
            {
                var sprite = node.Value;
                var next = node.Next;
                if (sprite.IsDestroyed)
                {
                    RemoveMagicSprite(node);
                }
                else
                {
                    sprite.Update(gameTime);
                }
                node = next;
            }

            for (var node = _solideMagicSprites.First; node != null;)
            {
                var sprite = node.Value;
                var next = node.Next;
                if (sprite.IsInDestroy || sprite.IsDestroyed)
                {
                    _solideMagicSprites.Remove(node);
                }
                node = next;
            }

            for (var node = _effectSprites.First; node != null;)
            {
                var sprite = node.Value;
                var next = node.Next;
                sprite.Update(gameTime);
                if (!sprite.IsInPlaying)
                {
                    _effectSprites.Remove(node);
                }
                node = next;
            }

            for (var node = _kind19Magics.First; node != null;)
            {
                var info = node.Value;
                var next = node.Next;
                if (info.LastTilePosition != info.BelongCharacter.TilePosition)
                {
                    AddFixedPositionMagicSprite(info.BelongCharacter, info.TheMagic, Map.ToPixelPosition(info.LastTilePosition), true);
                    info.LastTilePosition = info.BelongCharacter.TilePosition;
                }
                info.KeepMilliseconds -= elapsedMilliseconds;
                if (info.KeepMilliseconds <= 0)
                {
                    _kind19Magics.Remove(node);
                }
                node = next;
            }

            //Update list of magic sprites in view
            _magicSpritesInView = GetMagicSpritesInView();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var sprite in _magicSprites)
            {
                sprite.Draw(spriteBatch);
            }

            foreach (var sprite in _effectSprites)
            {
                sprite.Draw(spriteBatch);
            }
        }

        public class WorkItem
        {
            public float LeftMilliseconds;
            public MagicSprite TheSprite;

            public WorkItem(float leftMilliseconds, MagicSprite theSprite)
            {
                LeftMilliseconds = leftMilliseconds;
                TheSprite = theSprite;
            }
        }

        public class Kind19MagicInfo
        {
            public double KeepMilliseconds;
            public Magic TheMagic;
            public Character BelongCharacter;
            public Vector2 LastTilePosition;

            public Kind19MagicInfo(double keepMilliseconds, Magic magic, Character belongCharacter)
            {
                KeepMilliseconds = keepMilliseconds;
                TheMagic = magic;
                BelongCharacter = belongCharacter;
                LastTilePosition = belongCharacter.TilePosition;
            }
        }
    }
}
