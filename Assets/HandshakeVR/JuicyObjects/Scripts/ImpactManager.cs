using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CatchCo;

namespace HandshakeVR
{
    public class ImpactManager : MonoBehaviour
    {
        [SerializeField]
        GameObject impactEffectPrefab;

        [SerializeField]
        float powerMultiplier = 1;

        ImpactEffect[] objectPool;

        int maxPoolSize = 12;

        // Use this for initialization
        void Start()
        {
            objectPool = new ImpactEffect[maxPoolSize];

            for(int i=0; i < maxPoolSize; i++)
            {
                GameObject newImpactObject = GameObject.Instantiate(impactEffectPrefab);
                impactEffectPrefab.SetActive(false);
                objectPool[i] = newImpactObject.GetComponent<ImpactEffect>();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        [ExposeMethodInEditor]
        public void SpawnTestInstance()
        {
            SpawnInstance(Vector3.zero, Vector3.up, 1, 1);
        }

        public void SpawnInstance(Vector3 position, Vector3 normal, float power, float scale)
        {
            // find a free object in the pool.
            foreach(ImpactEffect impactEffect in objectPool)
            {
                if(!impactEffect.gameObject.activeInHierarchy && 
                    !impactEffect.gameObject.activeSelf)
                {
                    impactEffect.transform.position = position;
                    impactEffect.transform.rotation = Quaternion.LookRotation(normal);
                    impactEffect.transform.localScale = Vector3.one * (Mathf.Clamp((power * powerMultiplier), 0, scale * 0.5f));

                    impactEffect.gameObject.SetActive(true);
                    impactEffect.ActivateImpact();

                    break;
                }
            }
        }
    }
}