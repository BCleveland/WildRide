using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Setpieces
{
    public class SetpieceRockslide : Setpiece
    {
        [SerializeField] private GameObject m_RockPrefab = null;
        public override void ActivateSetpiece()
        {
            Activated = true;
            for(int j = 0; j < 15; j++)
            {
                Vector3 spawnPos = transform.position + Random.onUnitSphere * 3;
                Instantiate(m_RockPrefab, spawnPos, Random.rotation);
            }
        }
    }
}
