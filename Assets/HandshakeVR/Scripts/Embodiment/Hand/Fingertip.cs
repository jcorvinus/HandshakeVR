using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Leap.Unity;

namespace HandshakeVR
{
    /// <summary>
    /// Sends messages to objects in the world that are touched by the fingertip.
    /// 
    /// Legend:
    /// OnFingertipTriggerEnter - sent when this fingertip enters a trigger volume.
    /// OnFingertipTriggerStay - sent when this fingertip stays in a trigger volume.
    /// OnFingertipTriggerExit - sent when this fingertip exits a trigger volume (deletion is handled, you won't have to check against the fingertip suddenly going null).
    /// </summary>
    public class Fingertip : MonoBehaviour
    {
        private FingertipData fingertipData;

        [SerializeField]
        FingerFilter finger;

        UserHand userHand;

        private List<GameObject> otherObjectList;

        void Awake()
        {
			UserRig userRig = GetComponentInParent<UserRig>();
			RigidHand rigidHand = GetComponentInParent<RigidHand>();
			userHand = (rigidHand.Handedness == Chirality.Left) ? userRig.LeftHand : userRig.RightHand;
            fingertipData.Owner = this.gameObject.GetComponent<CapsuleCollider>();
            fingertipData.HandModel = userHand;
            otherObjectList = new List<GameObject>();

            fingertipData.HandModel.OnTrackingLost += InputProvider_HandTrackingLost;

            fingertipData.finger = finger;
        }

        private void Start()
        {
            if (fingertipData.HandModel.DisableUINonIndexFingertips && fingertipData.finger != FingerFilter.index) enabled = false;
        }

        private void OnEnable()
        {
            if (fingertipData.HandModel.DisableUINonIndexFingertips && fingertipData.finger != FingerFilter.index) enabled = false;
        }

        private void InputProvider_HandTrackingLost(UserHand hand)
        {
            ClearList();
        }

        void FingerTipEnter(Collider other)
        {
            if (enabled)
            {
                other.gameObject.SendMessage("OnFingertipTriggerEnter", fingertipData, SendMessageOptions.DontRequireReceiver);
                otherObjectList.Add(other.gameObject);
            }
        }

        void FingerTipStay(Collider other)
        {
            if (enabled)
            {
                other.gameObject.SendMessage("OnFingertipTriggerStay", fingertipData, SendMessageOptions.DontRequireReceiver);
            }
        }

        void FingertipExit(Collider other)
        {
            {
                SendExitEvent(other.gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            FingerTipEnter(other);
        }

        void OnTriggerStay(Collider other)
        {
            FingerTipStay(other);
        }

        void OnTriggerExit(Collider other)
        {
            FingertipExit(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            FingerTipEnter(collision.collider);
        }

        private void OnCollisionStay(Collision collision)
        {
            FingerTipStay(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            FingertipExit(collision.collider);
        }

        private void SendExitEvent(GameObject other)
        {
            try
            {
                other.SendMessage("OnFingertipTriggerExit", fingertipData, SendMessageOptions.DontRequireReceiver);
                otherObjectList.Remove(other.gameObject);
            }
            catch(MissingReferenceException missingRef)
            {
                Debug.LogWarning("Missing Reference in Fingertip.cs. This is OK if you just deleted an object");
            }
        }

        private void ClearList()
        {
            GameObject[] objectList = otherObjectList.ToArray();

            if (objectList == null) return;

            foreach (GameObject otherObject in objectList)
            {
                if (otherObject == null) continue;
                else SendExitEvent(otherObject);
            }

            otherObjectList.Clear();
        }

        void OnDestroy()
        {
            ClearList();
        }
    }
}