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
            for(int j = 0; j < 5; j++)
            {
                Vector3 spawnPos = transform.position + Random.onUnitSphere * 3;
                Instantiate(m_RockPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
