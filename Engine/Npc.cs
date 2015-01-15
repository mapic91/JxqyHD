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
        private int _currentLoopWalkPathIndex;

        private List<Vector2> ActionPathTilePositionList
        {
            get
            {
                if (_actionPathTilePositionList == null)
                {
                    _actionPathTilePositionList = GetRandTilePath(8, 
                        Kind != (int)CharacterType.Flyer);
                }
                return _actionPathTilePositionList;
            }
        }

        #region public properties
        public override PathFinder.PathType PathType
        {
            get
            {
                if (Kind == (int)CharacterType.Flyer)
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

        private void MoveToPlayer()
        {
            if (!Globals.ThePlayer.IsStanding())
            {
                MoveTo(Globals.ThePlayer.TilePosition);
            }
        }

        /// <summary>
        /// Walk or run to destination.
        /// If distance greater than 5, run to destination.
        /// If distance greater than 2, and is running, run to destination, else walk to destination.
        /// </summary>
        /// <param name="destinationTilePosition">Destination tile position</param>
        public void MoveTo(Vector2 destinationTilePosition)
        {
            var distance = Engine.PathFinder.GetTileDistance(TilePosition,
                destinationTilePosition);
            if (distance > 5)
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
            if (Kind == (int)CharacterType.Flyer) return false;

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

            //Follow target not found, do something else.
            if (FollowTarget == null)
            {
                if (Kind == (int)CharacterType.Flyer &&
                    !string.IsNullOrEmpty(FixedPos) &&
                    MoveAlongFixedPath(FixedPathTilePositionList, ref _currentFixedPosIndex))
                {
                    //FixedPos setted and flyer can move along it.
                    //Do nothing.
                }
                else
                {
                    var isFlyer = Kind == (int) CharacterType.Flyer;
                    switch ((CharacterType)Kind)
                    {
                        case CharacterType.Normal:
                        case CharacterType.Fighter:
                        case CharacterType.GroundAnimal:
                        case CharacterType.Eventer:
                        case CharacterType.Flyer:
                            {
                                switch ((ActionType)Action)
                                {
                                    case ActionType.RandWalk:
                                        const int randWalkPosibility = 400;
                                        const int flyerRandWalkPosibility = 20;
                                        RandWalk(ActionPathTilePositionList,
                                            isFlyer ? flyerRandWalkPosibility : randWalkPosibility,
                                            isFlyer);
                                        break;
                                    case ActionType.LoopWalk:
                                        LoopWalk(ActionPathTilePositionList,
                                            ref _currentLoopWalkPathIndex,
                                            isFlyer);
                                        break;
                                }
                            }
                            break;
                        case CharacterType.AfraidPlayerAnimal:
                            break;
                    }
                }
            }

            base.Update(gameTime);
        }

        private void RandWalk(List<Vector2> tilePositionList, int randMaxValue, bool isFlyer)
        {
            if (tilePositionList == null ||
                tilePositionList.Count < 2 ||
                !IsStanding()) return;
            if (Globals.TheRandom.Next(0, randMaxValue) == 0)
            {
                var tilePosition = tilePositionList[Globals.TheRandom.Next(0, tilePositionList.Count)];
                MoveTo(tilePosition, isFlyer);
            }
        }

        private void LoopWalk(List<Vector2> tilePositionList, ref int currentPathIndex, bool isFlyer)
        {
            if (tilePositionList == null ||
                tilePositionList.Count < 2) return;
            if (IsStanding())
            {
                currentPathIndex++;
                if (currentPathIndex > tilePositionList.Count - 1)
                {
                    currentPathIndex = 0;
                }
                MoveTo(tilePositionList[currentPathIndex], isFlyer);
            }
        }

        private void MoveTo(Vector2 tilePosition, bool isFlyer)
        {
            if (isFlyer)
            {
                //Flyer can move in straight line use fixed path move style.
                FixedPathMoveToDestination(tilePosition);
            }
            else
            {
                //Find path and walk to destionation.
                WalkTo(tilePosition);
            }
        }

        /// <summary>
        /// Get rand path, path first step is current character tile position.
        /// </summary>
        /// <param name="count">Path step count.</param>
        /// <param name="checkObstacle">If true, tile position is obstacle for character is no added to path.</param>
        /// <returns>The rand path.</returns>
        private List<Vector2> GetRandTilePath(int count, bool checkObstacle)
        {
            var path = new List<Vector2>() { TilePosition };

            int maxTry = count * 3;//For performace, otherwise method may run forever.
            const int maxOffset = 15;

            for (var i = 1; i < count; i++)
            {
                Vector2 tilePosition;
                do
                {
                    if (--maxTry < 0) return path;

                    tilePosition = Globals.TheMap.GetRandPositon(TilePosition, maxOffset);
                } while (tilePosition == Vector2.Zero ||
                    (checkObstacle && Globals.TheMap.IsObstacleForCharacter(tilePosition)));
                path.Add(tilePosition);
            }

            return path;
        }
    }
}
