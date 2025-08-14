using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace forest
{
    /// <summary>
    /// USAGE: Per turn
    /// </summary>
    public class Combat050OpponentMove : CombatState
    {
        private bool firstStep = false;

        public Combat050OpponentMove(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if (!firstStep)
            {
                firstStep = true;
                Loam.CoroutineObject.Instance.StartCoroutine(OpponentMove());
            }
        }

        private IEnumerator OpponentMove()
        {
            const float visualDisplayDelay = .3f;
            const float visualMoveDelay = .1f;

            if (!TryGetOpponentsTarget(out PlayfieldUnit targeted))
            {
                yield return null;

                // If we're here, we're not gonna get anything done
                // since there are no players yet.
                StateMachine.SetState<Combat070EvaluateTurn>();

                yield break;
            }

            yield return null;
            for (int unitIndex = 0; unitIndex < StateMachine.Playfield.units.Count; ++unitIndex)
            {
                // Select and display the move for an opponent
                PlayfieldUnit curOpponentToMove = StateMachine.Playfield.units[unitIndex];
                if (curOpponentToMove.team != Team.Opponent)
                {
                    continue;
                }

                StateMachine.VisualPlayfield.DisplayIndicatorMovePreview(curOpponentToMove, StateMachine.Playfield);
                yield return new WaitForSeconds(visualDisplayDelay);

                // 'Walk' towards the player based on available moves
                if (BuildPlayerStepPath(StateMachine.Playfield, targeted, curOpponentToMove, out List<Tile> steps))
                {
                    StateMachine.VisualPlayfield.ShowMovePath(StateMachine.Playfield, curOpponentToMove, steps);
                    yield return new WaitForSeconds(visualMoveDelay * 3);

                    foreach (Tile step in steps)
                    {
                        bool canTakeStep = Utils.CanMovePlayfieldUnitTo(StateMachine.Playfield, curOpponentToMove, step.associatedPos);
                        if (!canTakeStep)
                        {
                            break;
                        }

                        Utils.MoveUnitToLocation(StateMachine.Playfield, StateMachine.VisualPlayfield, curOpponentToMove, step.associatedPos);
                        yield return new WaitForSeconds(visualMoveDelay);
                    }
                }

                // Show attack visuals, and let them linger for a second before attacking.
                StateMachine.VisualPlayfield.DisplayIndicatorAttackPreview(curOpponentToMove, StateMachine.Playfield);
                yield return new WaitForSeconds(visualDisplayDelay);

                // Apply damage
                Vector2Int head = curOpponentToMove.locations[PlayfieldUnit.HEAD_INDEX];
                Vector2Int closest = GetClosestOpponentPosition(curOpponentToMove);

                if (head.GridDistance(closest) <= curOpponentToMove.curAttackRange)
                {
                    if (StateMachine.Playfield.TryGetUnitAt(closest, out PlayfieldUnit targetPlayerUnit))
                    {
                        StateMachine.VisualPlayfield.DamageUnit(curOpponentToMove, targetPlayerUnit, StateMachine.Playfield);
                    }
                }

                StateMachine.VisualPlayfield.HideIndicators();
                yield return new WaitForSeconds(visualDisplayDelay);
            }

            StateMachine.SetState<Combat070EvaluateTurn>();
        }

        private Vector2Int GetClosestOpponentPosition(PlayfieldUnit curOpponent)
        {
            Vector2Int closestPos = new Vector2Int(-short.MaxValue, -short.MaxValue); // A wild distance
            Vector2Int head = curOpponent.locations[PlayfieldUnit.HEAD_INDEX];

            for (int i = 0; i < StateMachine.Playfield.units.Count; ++i)
            {
                PlayfieldUnit unit = StateMachine.Playfield.units[i];
                if (unit.team == Team.Player)
                {
                    for (int loc = 0; loc < unit.locations.Count; ++loc)
                    {
                        Vector2Int curLoc = unit.locations[loc];
                        if (head.GridDistance(curLoc) < head.GridDistance(closestPos))
                        {
                            closestPos = curLoc;
                        }
                    }
                }
            }

            return closestPos;
        }

        private bool TryGetOpponentsTarget(out PlayfieldUnit targeted)
        {
            for (int i = 0; i < StateMachine.Playfield.units.Count; ++i)
            {
                PlayfieldUnit cur = StateMachine.Playfield.units[i];
                if (cur.team == Team.Player)
                {
                    targeted = StateMachine.Playfield.units[i];
                    return true;
                }
            }

            targeted = null;
            return false;
        }


        /// <summary>
        /// Uses DFS with heuristic (greedy and frankly slightly drunk) to move towards the player.
        /// Note: Not entirely sure if this should be visual or not, but it is largely a visual operation
        /// since it's based on the existing tiles and data that's accessed through it.
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="targeted"></param>
        /// <param name="curOpponentToMove"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        private bool BuildPlayerStepPath(Playfield playfield, PlayfieldUnit targeted, PlayfieldUnit curOpponentToMove, out List<Tile> steps)
        {
            List<SearchNode<Tile>> pending = new List<SearchNode<Tile>>();
            List<SearchNode<Tile>> visited = new List<SearchNode<Tile>>();

            steps = null;
            pending.Clear();
            visited.Clear();

            // Done as head indexes
            Vector2Int startPos = curOpponentToMove.locations[PlayfieldUnit.HEAD_INDEX];
            Vector2Int goalPos = targeted.locations[PlayfieldUnit.HEAD_INDEX];

            Tile startTile = FindTile(startPos);
            Tile goalTile = FindTile(goalPos);


            pending.Add(new SearchNode<Tile>(startTile, startTile.moveDifficulty));

            bool VisitedContains(Tile toFind)
            {
                return visited.Find(f => f.data == toFind) != null;
            }

            void AddToList(SearchNode<Tile> parent, Vector2Int pos, Vector2Int offset)
            {
                Vector2Int curPos = pos + offset;
                if (playfield.world.IsPosInGrid(curPos))
                {
                    Tile newItem = FindTile(curPos);
                    if (VisitedContains(newItem))
                    {
                        return;
                    }

                    if (newItem.isImpassable)
                    {
                        return;
                    }

                    SearchNode<Tile> node = new SearchNode<Tile>(newItem, newItem.moveDifficulty);
                    node.curNodeCost = int.MaxValue;

                    int xDif = Mathf.Abs(goalPos.x - curPos.x);
                    int yDif = Mathf.Abs(goalPos.y - curPos.y);
                    int distTar = xDif + yDif + Mathf.Abs(xDif - yDif);

                    node.heuristic = newItem.moveDifficulty + distTar;
                    node.parent = parent;

                    pending.Add(node);
                }
            }

            bool foundTarget = false;
            while (pending.Count > 0)
            {
                SearchNode<Tile> item = pending[0];
                pending.RemoveAt(0);

                if (item.data == goalTile)
                {
                    foundTarget = true;
                    break;
                }

                visited.Add(item);
                Vector2Int pos = item.data.associatedPos;

                AddToList(item, pos, Vector2Int.left);
                AddToList(item, pos, Vector2Int.right);
                AddToList(item, pos, Vector2Int.up);
                AddToList(item, pos, Vector2Int.down);

                pending.Sort((a, b) => a.heuristic - b.heuristic);
            }

            if (!foundTarget)
            {
                return false;
            }

            steps = new List<Tile>();
            SearchNode<Tile> toWalk = visited[visited.Count - 1];
            while (toWalk.parent != null)
            {
                steps.Add(toWalk.data);
                toWalk = toWalk.parent;
            }
            steps.Reverse();

            return true;
        }

        private Tile FindTile(Vector2Int toFind)
        {
            return StateMachine.VisualPlayfield.FindTile(toFind);
        }
    }
}