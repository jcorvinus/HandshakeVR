using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandshakeVR.Demo
{
    public class Alert : MonoBehaviour
    {
        public UnityEvent OnAlert;

        [SerializeField]
        float viewAngle = 25;

        [SerializeField]
        float viewDuration = 1f;
        float viewTime = 0;

        bool hasAlerted = false;

        Transform viewCamera;

        Animator animator;

        int alertHash;

        // Use this for initialization
        void Start()
        {
            viewCamera = Camera.main.transform;

            animator = GetComponent<Animator>();
            alertHash = Animator.StringToHash("Alert");
        }

        // Update is called once per frame
        void Update()
        {
            animator.SetBool(alertHash, !hasAlerted);
            
            if(!hasAlerted)
            {
                Vector3 directionToSelf = (transform.position - viewCamera.position).normalized;

                if (Vector3.Angle(directionToSelf, viewCamera.forward) <= viewAngle)
                {
                    viewTime += Time.deltaTime;

                    if (viewTime >= viewDuration)
                    {
                        OnAlert.Invoke();
                        hasAlerted = true;
                        return;
                    }
                }
                else viewTime = 0;
            }
        }
    }
}