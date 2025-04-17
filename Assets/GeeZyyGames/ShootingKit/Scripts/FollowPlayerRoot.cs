using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeeZyyGames
{
    public class FollowPlayerRoot : MonoBehaviour
    {
        [HideInInspector]
        public Transform player;
        public Vector3 offset;

        private void LateUpdate()
        {
            if (player != null)
            {
                transform.position = player.transform.position + offset;
            }
        }
    }
}
