using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Npc : Character
    {
        private bool _attackTargetFinded;

        #region public properties
        public override PathFinder.PathType PathType
        {
            get
            {
                if (base.PathFinder == 1)
                    return Engine.PathFinder.PathType.PerfectMaxNpcTry;
                else
                    return Engine.PathFinder.PathType.PathOneStep;
            }
        }

        public bool IsEnemy
        {
            get { return Kind == 1 && Relation == 1; }
        }

        public bool IsFriend
        {
            get { return ((Kind == 1 && Relation == 0) || Kind == 3); }
        }

        public bool IsFollower
        {
            get { return Kind == 3; }
        }

        public bool IsInteractive
        {
            get { return (ScriptFile != null || IsEnemy || IsFriend); }
        }

        public static bool IsAIDisabled { protected set; get; }
        #endregion

        public Npc() { }

        public Npc(string filePath)
            : base(filePath)
        {
            if (LevelIni == null)
            {
                LevelIni = Utils.GetLevelLists(@"ini\level\level-npc.ini");
            }
        }

        protected override bool HasObstacle(Vector2 tilePosition)
        {
            return (NpcManager.IsObstacle(tilePosition) ||
                    ObjManager.IsObstacle(tilePosition) ||
                    Globals.ThePlayer.TilePosition == tilePosition);
        }

        protected override void PlaySoundEffect(SoundEffect soundEffect)
        {
            SoundManager.Play3DSoundOnece(soundEffect,
                PositionInWorld - Globals.ListenerPosition);
        }

        public static void DisableAI()
        {
            IsAIDisabled = true;
        }

        public static void EnableAI()
        {
            IsAIDisabled = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsEnemy)
            {
                var attackCanReach = false;
                var playerTilePosition = Globals.ThePlayer.TilePosition;
                if (IsAIDisabled)
                {
                    _attackTargetFinded = false;
                }
                else
                {
                    int tileDistance;
                    attackCanReach = Engine.PathFinder.CanMagicReach(TilePosition, playerTilePosition, out tileDistance);
                    if (IsStanding())
                    {
                        if ((attackCanReach && tileDistance <= VisionRadius) ||
                            (_attackTargetFinded && tileDistance <= VisionRadius)
                            )
                            _attackTargetFinded = true;
                        else _attackTargetFinded = false;
                    }
                }

                if (_attackTargetFinded)
                {
                    if (attackCanReach) Attacking(playerTilePosition);
                    else WalkTo(playerTilePosition);
                }
                else
                {
                    //just walk to last find position,don't attack
                    ClearTarget();
                }
            }
            base.Update(gameTime);
        }
    }
}
