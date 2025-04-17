using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace GeeZyyGames
{
    public class EnemyController : MonoBehaviour
    {
        private RigBuilder rigBuilder;
        private AudioSource audioSource;

        private void Start()
        {
            SetupRig();
            AudioSetup();
        }

        private void SetupRig()
        {

            if (rigBuilder == null)
            {
                rigBuilder = gameObject.AddComponent<RigBuilder>();
            }
            

            RigControl rigControl = GetComponentInChildren<RigControl>();

            if (rigBuilder.layers == null)
            {
                rigBuilder.layers = new System.Collections.Generic.List<RigLayer>();
            }

            if (rigControl.rig1 != null)
            {
                RigLayer newLayer1 = new RigLayer(rigControl.rig1, true);
                rigBuilder.layers.Add(newLayer1);
            }

            if (rigControl.rig2 != null)
            {
                RigLayer newLayer2 = new RigLayer(rigControl.rig2, true);
                rigBuilder.layers.Add(newLayer2);
            }

            if (rigControl.rig3 != null)
            {
                RigLayer newLayer2 = new RigLayer(rigControl.rig3, true);
                rigBuilder.layers.Add(newLayer2);
            }

            rigBuilder.Build();

            rigControl.rig1.weight = 1;
            rigControl.rig2.weight = 1;
            rigControl.rig3.weight = 1;

        }

        private void AudioSetup()
        {
            if(audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.5f;
            }
        }
    }
}
