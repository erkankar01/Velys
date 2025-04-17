using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeeZyyGames
{
    public class CrossHairTarget : MonoBehaviour
    {
        Camera mainCamera;
        Ray ray;
        public RaycastHit hitInfo;
        public Transform lookAim;
        public LayerMask layerMask;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            ray.origin = mainCamera.transform.position;
            ray.direction = mainCamera.transform.forward;
            Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore);

            if (hitInfo.transform == null && lookAim != null)
            {
                transform.position = lookAim.position;
            }

            else
            {
                transform.position = hitInfo.point;
            }
        }
    }
}
