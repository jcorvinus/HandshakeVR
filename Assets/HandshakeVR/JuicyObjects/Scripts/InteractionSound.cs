using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;

namespace HandshakeVR
{
    public class InteractionSound : MonoBehaviour
    {
        InteractionBehaviour interactionBehaviour;
        Rigidbody rigidBody;
        [SerializeField] AudioSource throwSource;
        [SerializeField] AudioSource impactSource;
		[SerializeField] AudioSource grabSource;
		[SerializeField] AudioSource slideSource;

		public AudioSource ThrowSource { get { return throwSource; } }
		public AudioSource ImpactSource { get { return impactSource; } }
		public AudioSource GrabSource { get { return grabSource; } }
		public AudioSource SlideSource { get { return slideSource; } }		

        ImpactManager impactManager;

        // component acquisition state. Storing these because
        // checking isnull on monobehaviour has overhead we don't want to incur every frame.
        bool hasInteractionBehaviour = false;
        bool hasRigidBody = false;
        bool hasImpactSource = false;
        bool hasThrowSource = false;
		bool hasGrabSource = false;
		bool hasImpactManager=false;

		// enable/disable feedback options
		[SerializeField] bool enableImpactVfx = true;
		[SerializeField] bool enableImpactSfx = true;

        float clashDuration = 0.03f;
        float clashTimer = 0;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            interactionBehaviour = GetComponent<InteractionBehaviour>();
            hasInteractionBehaviour = interactionBehaviour; // storing these as bools because checking isnull on monobehaviour has overhead.
            hasRigidBody = rigidBody;
            hasThrowSource = throwSource;
            hasImpactSource = impactSource;
			hasGrabSource = grabSource;

            impactManager = FindObjectOfType<ImpactManager>();
			hasImpactManager = impactManager;
        }

        void Start()
        {
			if (hasInteractionBehaviour)
			{
				interactionBehaviour.OnGraspBegin += (PlayGrabSound);
				interactionBehaviour.OnGraspEnd += (PlayThrowSound);
			}
        }

        private void Update()
        {
            clashTimer -= Time.deltaTime;
            clashTimer = (clashTimer < 0) ? 0 : clashTimer;
        }

        void OnDestroy()
        {
            hasInteractionBehaviour = false;
            hasRigidBody = false;
            hasImpactSource = false;
            hasThrowSource = false;
        }

		void PlayGrabSound()
		{
			if(hasGrabSource)
			{
				grabSource.Play();
			}
		}

        [CatchCo.ExposeMethodInEditor]
        void PlayThrowSound()
        {
            if (hasThrowSource)
            {
                throwSource.volume = MathSupplement.Exerp(0, 1, Mathf.InverseLerp(0.06f, 8, rigidBody.velocity.sqrMagnitude));
                throwSource.Play();
            }
        }

        [CatchCo.ExposeMethodInEditor]
        void PlayImpactSound()
        {
            if (hasImpactSource)
            {
                impactSource.volume = 1;
                impactSource.Play();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (clashTimer > 0) return;

            if (hasImpactSource && hasRigidBody)
            {
                float force = rigidBody.velocity.sqrMagnitude * rigidBody.mass;

				if (enableImpactSfx)
				{
					impactSource.volume = force;
					impactSource.Play();
				}

                if (impactManager && enableImpactVfx)
                {
                    impactManager.SpawnInstance(collision.contacts[0].point, collision.contacts[0].normal,
                        force, transform.localScale.x);
                }
            }

            clashTimer = clashDuration;
        }
    }
}