using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
    public class SteamVRRemapper : MonoBehaviour
    {
        [System.Serializable]
        public struct BoneBasis
        {
            public Vector3 Forward;
            public Vector3 Up;
        }

        public struct BoneData
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        [Tooltip("This is our leap hand data generator")]
        [SerializeField]
        SkeletalControllerHand controllerHand;

        [Tooltip("These refer to our SteamVR skeleton, which will get retargeted onto the leap skeleton")]
        [Header("SteamVR skeleton variables")]
        [SerializeField]
        Transform wrist;

        [SerializeField]
        BoneBasis wristBasis;

        [SerializeField]
        Vector3 palmOffset;

        // 0 finger_index_meta_r
        // 1 finger_middle_meta_r
        // 2 finger_ring_meta_r
        // 3 finger_pinky_meta_r
        // 4 finger_thumb_0_r
        Transform[] fingerMetacarpals;

        [SerializeField]
        BoneBasis fingerBasis;

        SteamVR_Behaviour_Pose steamVRPose;

        [Header("Debug Vars")]
        [SerializeField]
        bool drawSkeleton = false;

        private void Awake()
        {
            if(!controllerHand) controllerHand = GetComponent<SkeletalControllerHand>();
            steamVRPose = wrist.GetComponentInParent<SteamVR_Behaviour_Pose>();
        }

        private void Start()
        {
            GetMetacarpals();
        }

        void GetMetacarpals()
        {
            fingerMetacarpals = new Transform[5];

            fingerMetacarpals[0] = wrist.Find("finger_index_meta_r");
            fingerMetacarpals[1] = wrist.Find("finger_middle_meta_r");
            fingerMetacarpals[2] = wrist.Find("finger_ring_meta_r");
            fingerMetacarpals[3] = wrist.Find("finger_pinky_meta_r");
            fingerMetacarpals[4] = wrist.Find("finger_thumb_0_r");
        }

        private void Update()
        {
            // set our wrist positions to match first
            controllerHand.Wrist.transform.position = wrist.transform.position;

            Quaternion wristBoneOrientation = controllerHand.GetLocalBasis();
            controllerHand.Wrist.rotation = GlobalRotationFromBasis(wrist, wristBasis) * wristBoneOrientation;

            for(int fingerIndx=0; fingerIndx < fingerMetacarpals.Length; fingerIndx++)
            {
                Transform fingerRoot = null;

                if (fingerIndx == 0) fingerRoot = controllerHand.IndexMetacarpal;
                else if (fingerIndx == 1) fingerRoot = controllerHand.MiddleMetacarpal;
                else if (fingerIndx == 2) fingerRoot = controllerHand.RingMetacarpal;
                else if (fingerIndx == 3) fingerRoot = controllerHand.PinkyMetacarpal;
                else fingerRoot = controllerHand.ThumbMetacarpal;

                 MatchBones(fingerMetacarpals[fingerIndx], fingerRoot, fingerBasis, wristBoneOrientation);
            }
        }

        public bool IsTracking { get { return steamVRPose.isValid; } }

        void MatchBones(Transform steamVRBone, Transform leapBone, BoneBasis basis,
            Quaternion leapOrientation, int depth =0)
        {
            if (depth == 0) leapBone.transform.position = steamVRBone.transform.position;
            else
            {
                controllerHand.SetTransformWithConstraint(leapBone, steamVRBone.transform.position, GlobalRotationFromBasis(steamVRBone, basis) * leapOrientation);
                //leapBone.transform.SetPositionAndRotation(steamVRBone.transform.position, GlobalRotationFromBasis(steamVRBone, basis) * leapOrientation);
            }

            if(steamVRBone.childCount == leapBone.childCount)
            {
                if(steamVRBone.childCount == 1)
                {
                    MatchBones(steamVRBone.GetChild(0), leapBone.GetChild(0), basis, leapOrientation, depth + 1);
                }
            }
            else
            {
                Debug.LogError("Mismatch between steamVR and leap child count. Steam Bone:" + steamVRBone + " leap bone: " + leapBone);
                Debug.Break();
            }
        }

        Quaternion GlobalRotationFromBasis(Transform bone, BoneBasis basis)
        {
            return Quaternion.LookRotation(bone.TransformDirection(basis.Forward),
                bone.TransformDirection(basis.Up));
        }

        private void DrawBones(Transform parent, BoneBasis basis)
        {
            DrawBasis(parent, basis);

            for(int i=0; i < parent.childCount; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(parent.transform.position,
                    parent.GetChild(i).position);

                DrawBones(parent.GetChild(i), basis);
            }
        }

        private void DrawBasis(Transform bone, BoneBasis basis)
        {
            Quaternion rotation = GlobalRotationFromBasis(bone, basis);

            Vector3 up, forward, right;

            up = rotation * Vector3.up;
            forward = rotation * Vector3.forward;
            right = rotation * Vector3.right;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bone.position, bone.position + (up * 0.025f));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(bone.position, bone.position + (right * 0.025f));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(bone.position, bone.position + (forward * 0.025f));
        }

        private void DrawPalm(Transform bone, BoneBasis basis)
        {
            DrawBasis(bone, basis);

            Quaternion rotation = Quaternion.LookRotation(bone.transform.TransformDirection(basis.Forward),
                basis.Up);

            Vector3 up, forward, right;

            up = rotation * Vector3.up;
            forward = rotation * Vector3.forward;
            right = rotation * Vector3.right;

            Vector3 palmPosition = wrist.TransformPoint(palmOffset);

            Gizmos.color = Color.black;
            Gizmos.DrawLine(palmPosition, palmPosition + forward * 0.04f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled || !drawSkeleton) return;

            if (fingerMetacarpals == null || fingerMetacarpals.Length == 0) GetMetacarpals();

            for (int i = 0; i < fingerMetacarpals.Length; i++)
            {
                DrawBones(fingerMetacarpals[i], fingerBasis);
            }

            DrawPalm(wrist, wristBasis);
        }
    }
}