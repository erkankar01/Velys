using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeeZyyGames
{
    public class BodyLookController : MonoBehaviour
    {
        private GameObject aimLook;
        private LookForEnemy lookForEnemy;
        public bool isEnemy;

        private void Start()
        {
            if (isEnemy)
            {
                lookForEnemy = GameObject.FindObjectOfType<LookForEnemy>();
            }

            else
            {
                aimLook = GameObject.FindGameObjectWithTag("Aim");
            }
        }

        private void FixedUpdate()
        {
            if (isEnemy)
            {
                transform.position = lookForEnemy.transform.position;
            }

            else
            {
                transform.position = aimLook.transform.position;
            }
        }
    }
}
