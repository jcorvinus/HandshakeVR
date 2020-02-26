using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CatchCo;

namespace HandshakeVR
{
    public class ImpactEffect : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] AnimationCurve fadeCurve = AnimationCurve.Linear(0,0,1,1);

        SkinnedMeshRenderer skinnedRenderer;
        [SerializeField] Color defaultColor;

        int activateHash;
        int impactAnimStateHash;

        // Use this for initialization
        void Start()
        {
            activateHash = Animator.StringToHash("Activate");
            impactAnimStateHash = Animator.StringToHash("impactAnim");

            skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        [ExposeMethodInEditor]
        public void ActivateImpact()
        {
            animator.SetTrigger(activateHash);
        }

        private void Update()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == impactAnimStateHash)
            {
                skinnedRenderer.sharedMaterial.color = Color.Lerp(defaultColor, Color.clear,
                    fadeCurve.Evaluate(stateInfo.normalizedTime));
            }
            else
            {
                skinnedRenderer.sharedMaterial.color = Color.clear;
            }
        }
    }
}