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
        private int _idledFrame;

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
                else if(IsInLoopWalk || IsEnemy)
                {
                    return Engine.PathFinder.PathType.PathOneStep;
                }
                else if (Kind == 0 || Kind == 5)
                {
                    // Normal npc
                    return Engine.PathFinder.PathType.PerfectMaxPlayerTry;
                }
                else
                {
                    return Engine.PathFinder.PathType.PerfectMaxNpcTry;
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
            if (!Globals.PlayerKindCharacter.IsStanding())
            {
                PartnerMoveTo(Globals.PlayerTilePosition);
            }
        }

        #endregion Private method

        public override bool HasObstacle(Vector2 tilePosition)
        {
            if (Kind == (int)CharacterKind.Flyer) return false;

            return (NpcManager.IsObstacle(tilePosition) ||
                    ObjManager.IsObstacle(tilePosition) ||
                    MagicManager.IsObstacle(tilePosition) ||
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
                //Because character won't update new destination before step move end, 
                //so set MoveTargetChanged to true make sure character get new destination.
                MoveTargetChanged = true;

                if (attackCanReach)
                {
                    if (_idledFrame >= Idle)
                    {
                        _idledFrame = 0;
                        Attacking(FollowTarget.TilePosition);
                    }
                }
                else
                {
                    WalkTo(FollowTarget.TilePosition);
                }
            }
        }

        protected override void FollowTargetLost()
        {
            //just walk to last find position
            CancleAttackTarget();
            if (IsPartner)
            {
                MoveToPlayer();
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
            if (_controledMagicSprite != null)
            {
                base.Update(gameTime);
                return;
            }

            //Find follow target
            if (!IsFollowTargetFound || // Not find target.
                FollowTarget == null || // Follow target not assign.
                FollowTarget.IsDeathInvoked || //Follow target is death.
                IsAIDisabled) //Npc AI is disabled.
            {
                if (IsEnemy)
                {
                    FollowTarget = IsAIDisabled ? null : NpcManager.GetLiveClosedPlayerOrFighterFriend(PositionInWorld);
                }
                else if (IsFighterFriend)
                {
                    FollowTarget = IsAIDisabled ? null : NpcManager.GetClosedEnemyTypeCharacter(PositionInWorld);
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

            //Attack interval
            if (_idledFrame < Idle)
            {
                _idledFrame++;
            }

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
                                KeepMinTileDistance(Globals.PlayerTilePosition, VisionRadius);
                            }
                            break;
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}
