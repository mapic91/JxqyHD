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

        public Npc(KeyDataCollection keyDataCollection)
            : base(keyDataCollection)
        {
            if (LevelIni == null)
            {
                LevelIni = Utils.GetLevelLists(@"ini\level\level-npc.ini");
            }
        }

        /// <summary>
        /// Walk or run to player
        /// </summary>
        private void MoveToPlayer()
        {
            var distance = Engine.PathFinder.GetTileDistance(TilePosition,
                Globals.ThePlayer.TilePosition);
            if (distance > 5)
            {
                RunToPlayer();
            }
            else if (distance > 2)
            {
                if (IsRuning())
                {
                    RunToPlayer();
                }
                else
                {
                    WalkToPlayer();
                }
            }
        }

        private void RunToPlayer()
        {
            RunTo(Globals.ThePlayer.TilePosition);
        }

        private void WalkToPlayer()
        {
            WalkTo(Globals.ThePlayer.TilePosition);
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

        protected override void FollowTargetFinded(bool attackCanReach)
        {
            if (IsAIDisabled)
            {
                CancleAttackTarget();
            }
            else
            {
                if (attackCanReach) Attacking(FollowTarget.TilePosition);
                else WalkTo(FollowTarget.TilePosition);
            }
        }

        protected override void FollowTargetLost()
        {
            //just walk to last find position
            CancleAttackTarget();
            if (IsPartner)
            {
                WalkTo(Globals.ThePlayer.TilePosition);
            }
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
            if (IsAIDisabled)
            {
                //do nothing
            }
            else
            {
                if (!IsFollowTargetFinded ||
                    FollowTarget == null ||
                    FollowTarget.IsDeathInvoked)
                {
                    if (IsEnemy)
                    {
                        FollowTarget = NpcManager.GetClosedPlayerOrFighterFriend(PositionInWorld);
                    }
                    else if (IsFighterFriend)
                    {
                        FollowTarget = NpcManager.GetClosedEnemy(PositionInWorld);
                        //Fighter friend may be parter
                        if (FollowTarget == null && IsPartner)
                        {
                            //Can't find enemy, move to player
                            MoveToPlayer();
                        }
                    }
                    else if (IsPartner)
                    {
                        MoveToPlayer();
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}
