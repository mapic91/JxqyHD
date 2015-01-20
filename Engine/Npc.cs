using System;
using System.Collections.Generic;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    public class Npc : Character
    {
        private List<Vector2> _actionPathTilePositionList;

        private List<Vector2> ActionPathTilePositionList
        {
            get
            {
                if (_actionPathTilePositionList == null)
                {
                    _actionPathTilePositionList = GetRandTilePath(8, 
                        Kind == (int) CharacterKind.Flyer);
                }
                return _actionPathTilePositionList;
            }
            set { _actionPathTilePositionList = value; }
        }

        #region public properties
        public override PathFinder.PathType PathType
        {
            get
            {
                if (Kind == (int)CharacterKind.Flyer)
                {
                    return Engine.PathFinder.PathType.PathStraightLine;
                }
                else if (base.PathFinder == 1 || IsPartner)
                {
                    return Engine.PathFinder.PathType.PerfectMaxNpcTry;
                }
                else
                {
                    return Engine.PathFinder.PathType.PathOneStep;
                }
            }
        }

        public static bool IsAIDisabled { protected set; get; }
        #endregion

        #region Ctor
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
        #endregion Ctor

        #region Private method

        private void MoveToPlayer()
        {
            if (!Globals.ThePlayer.IsStanding())
            {
                PartnerMoveTo(Globals.ThePlayer.TilePosition);
            }
        }

        #endregion Private method

        /// <summary>
        /// Walk or run to destination.
        /// If distance greater than 5, run to destination.
        /// If distance greater than 2, and is running, run to destination, else walk to destination.
        /// </summary>
        /// <param name="destinationTilePosition">Destination tile position</param>
        public void PartnerMoveTo(Vector2 destinationTilePosition)
        {
            if (Globals.TheMap.IsObstacleForCharacter(destinationTilePosition))
            {
                //Destination is obstacle, can't move to destination, do nothing.
                return;
            }

            var distance = Engine.PathFinder.GetTileDistance(TilePosition,
                destinationTilePosition);
            if (distance > 20)
            {
                Globals.ThePlayer.ResetPartnerPosition();
            }
            else if (distance > 5)
            {
                RunTo(destinationTilePosition);
            }
            else if (distance > 2)
            {
                if (IsRuning())
                {
                    RunTo(destinationTilePosition);
                }
                else
                {
                    WalkTo(destinationTilePosition);
                }
            }
        }

        public override bool HasObstacle(Vector2 tilePosition)
        {
            if (Kind == (int)CharacterKind.Flyer) return false;

            return (NpcManager.IsObstacle(tilePosition) ||
                    ObjManager.IsObstacle(tilePosition) ||
                    Globals.ThePlayer.TilePosition == tilePosition);
        }

        protected override void PlaySoundEffect(SoundEffect soundEffect)
        {
            SoundManager.Play3DSoundOnece(soundEffect,
                PositionInWorld - Globals.ListenerPosition);
        }

        protected override void FollowTargetFound(bool attackCanReach)
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
            //Cancle attack
            NpcManager.CancleFighterAttacking();
        }

        public static void EnableAI()
        {
            IsAIDisabled = false;
        }

        public override void Update(GameTime gameTime)
        {
            //Find follow target
            if (!IsFollowTargetFound || // Not find target.
                FollowTarget == null || // Follow target not assign.
                FollowTarget.IsDeathInvoked || //Follow target is death.
                IsAIDisabled) //Npc AI is disabled.
            {
                if (IsEnemy)
                {
                    FollowTarget = IsAIDisabled ? null : NpcManager.GetClosedPlayerOrFighterFriend(PositionInWorld);
                }
                else if (IsFighterFriend)
                {
                    FollowTarget = IsAIDisabled ? null : NpcManager.GetClosedEnemy(PositionInWorld);
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

            //Follow target
            PerformeFollow();

            if (IsFollowTargetFound)
            {
                //Character found follow target and move to follow target.
                //This may cause character move far away from the initial base tile position.
                //Assing ActionPathTilePositionList value to null,
                //when get next time, new path is generated use new base tile postion.
                ActionPathTilePositionList = null;
            }

            ////Follow target not found, do something else.
            if ((FollowTarget == null || !IsFollowTargetFound) &&
                !(IsFighter && IsAIDisabled)) //Fighter can't move when AI diaabled
            {
                var isFlyer = Kind == (int)CharacterKind.Flyer;
                const int randWalkPosibility = 400;
                const int flyerRandWalkPosibility = 20;

                if (Action == (int)ActionType.LoopWalk &&
                    FixedPathTilePositionList != null)
                {
                    //Loop walk along FixedPos
                    LoopWalk(FixedPathTilePositionList,
                        isFlyer ? flyerRandWalkPosibility : randWalkPosibility,
                        ref _currentFixedPosIndex,
                        isFlyer);
                }
                else
                {
                    switch ((CharacterKind)Kind)
                    {
                        case CharacterKind.Normal:
                        case CharacterKind.Fighter:
                        case CharacterKind.GroundAnimal:
                        case CharacterKind.Eventer:
                        case CharacterKind.Flyer:
                            {
                                switch ((ActionType)Action)
                                {
                                    case ActionType.RandWalk:
                                        RandWalk(ActionPathTilePositionList,
                                            isFlyer ? flyerRandWalkPosibility : randWalkPosibility,
                                            isFlyer);
                                        break;
                                }
                            }
                            break;
                        case CharacterKind.AfraidPlayerAnimal:
                            {
                                KeepMinTileDistance(Globals.ThePlayer.TilePosition, VisionRadius);
                            }
                            break;
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}
