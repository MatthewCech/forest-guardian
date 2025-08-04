using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace forest
{
    public class Combat07EvaluateTurn : CombatState
    {
        private bool firstStep = false;

        public Combat07EvaluateTurn(PlayfieldCore stateMachine) : base(stateMachine) { }

        public override void Update()
        {
            if(!firstStep)
            {
                firstStep = true;
                Loam.CoroutineObject.Instance.StartCoroutine(Evaluate());
            }
        }

        private IEnumerator Evaluate()
        {
            yield return new WaitForSeconds(StateMachine.turnDelay);

            bool hasPlayerUnit = false;
            for (int i = 0; i < StateMachine.Playfield.units.Count; ++i)
            {
                PlayfieldUnit cur = StateMachine.Playfield.units[i];
                if (cur.team == Team.Player)
                {
                    hasPlayerUnit = true;
                    break;
                }
            }

            if(!hasPlayerUnit)
            {
                yield return null;
                StateMachine.SetState<Combat09Defeat>();
                yield break;
            }

            bool didFindPortal = false;
            for(int i = 0; i < StateMachine.Playfield.portals.Count; ++i)
            {
                PlayfieldPortal portal = StateMachine.Playfield.portals[i];
                if(StateMachine.Playfield.TryGetUnitAt(portal.location, out PlayfieldUnit unit))
                {
                    if(unit.team == Team.Player)
                    {
                        yield return null;
                        StateMachine.SetState<Combat08Victory>();
                        didFindPortal = true;
                    }
                }
            }

            if (!didFindPortal)
            {
                // If we have no portals, then exit if all items are collected.
                if (StateMachine.Playfield.portals.Count == 0 && StateMachine.Playfield.items.Count == 0 && !HasEnemies())
                {
                    yield return null;
                    StateMachine.SetState<Combat08Victory>();
                }
                else
                {
                    yield return null;
                    StateMachine.SetState<Combat02PrepareTurn>();
                }
            }
        }
    }
}