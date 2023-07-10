using System.Collections.Generic;
using UnityEngine;

namespace RTS.AI
{
    public class TargetBuildingMap : Singleton<TargetBuildingMap>
    {
        private TargetBuilding[] _buildings;

        public TargetBuilding[] Buildings => _buildings ??= FindObjectsOfType<TargetBuilding>();

        public List<TargetBuilding> GetAvailableTargetBuildings(ETeam team)
        {
            ETeam opponentTeam = GameServices.GetOpponent(team);
            List<TargetBuilding> neutralBuildings = new List<TargetBuilding>();
            List<TargetBuilding> opponentBuildings = new List<TargetBuilding>();
            foreach (TargetBuilding targetBuilding in Buildings)
            {
                if (targetBuilding.GetTeam() == ETeam.Neutral)
                    neutralBuildings.Add(targetBuilding);
                else if (targetBuilding.GetTeam() == opponentTeam)
                    opponentBuildings.Add(targetBuilding);
            }
            neutralBuildings.AddRange(opponentBuildings);
            return neutralBuildings;
        }


        public TargetBuilding GetNearestAvailableTargetBuilding(Vector3 worldPos, ETeam team)
        {
            TargetBuilding target = GetNearestNeutralTargetBuilding(worldPos);

            if (target == null)
            {
                ETeam opponentTeam = GameServices.GetOpponent(team);
                target = GetNearestTeamTargetBuilding(worldPos, opponentTeam);
            }

            return target;
        }

        public TargetBuilding GetNearestNeutralTargetBuilding(Vector3 worldPos)
        {
            float minDist = float.MaxValue;
            TargetBuilding target = null;
            foreach (TargetBuilding targetBuilding in Buildings)
            {
                if (targetBuilding.GetTeam() != ETeam.Neutral)
                    continue;

                float dist = (worldPos - targetBuilding.transform.position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    target = targetBuilding;
                }
            }
            return target;
        }

        public TargetBuilding GetNearestTeamTargetBuilding(Vector3 worldPos, ETeam team)
        {
            float minDist = float.MaxValue;
            TargetBuilding target = null;
            foreach (TargetBuilding targetBuilding in Buildings)
            {
                if (targetBuilding.GetTeam() != team)
                    continue;

                float dist = (worldPos - targetBuilding.transform.position).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    target = targetBuilding;
                }
            }
            return target;
        }
    }   
}