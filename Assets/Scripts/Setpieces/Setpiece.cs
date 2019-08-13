using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Setpieces
{
    public abstract class Setpiece : MonoBehaviour
    {
        [SerializeField] public float TilePercentActivation = 1.0f;
        [SerializeField] public bool Activated = false;
        public abstract void ActivateSetpiece();
    }
}