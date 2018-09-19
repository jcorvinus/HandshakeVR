using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace CoordinateSpaceConversion
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

        [SerializeField]
        bool applyPositions = false;

        [SerializeField]
        bool applyPositionsToMetacarpalsOnly = true;

        [SerializeField]
        bool dontApplyRotationToMetacarpals = false;

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

        [SerializeField]
        Transform[] fingerMetacarpals; // index, middle, ring, pinky

        [SerializeField]
        BoneBasis fingerBasis;

        [SerializeField]
        Transform thumbMetacarpal;

        SteamVR_Behaviour_Pose steamVRPose;

        [Header("Debug Vars")]
        [SerializeField]
        bool drawSkeleton = false;  

        private void Awake()
        {
            if(!controllerHand) controllerHand = GetComponent<SkeletalControllerHand>();
            steamVRPose = wrist.GetComponentInParent<SteamVR_Behaviour_Pose>();
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
                else fingerRoot = controllerHand.PinkyMetacarpal;

                MatchBones(fingerMetacarpals[fingerIndx], fingerRoot, fingerBasis, wristBoneOrientation);
            }

            MatchBones(thumbMetacarpal, controllerHand.ThumbMetacarpal, fingerBasis, wristBoneOrientation);
        }

        public bool IsTracking { get { return steamVRPose.isValid; } }

        void MatchBones(Transform steamVRBone, Transform leapBone, BoneBasis basis, Quaternion leapOrientation, int depth =0)
        {
            if (applyPositions)
            {
                if(applyPositionsToMetacarpalsOnly && depth == 0 || !applyPositionsToMetacarpalsOnly) leapBone.transform.position = steamVRBone.transform.position;                
            }
            if(depth > 0 || !dontApplyRotationToMetacarpals) leapBone.transform.rotation = GlobalRotationFromBasis(steamVRBone, basis) * leapOrientation;

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

            for (int i = 0; i < fingerMetacarpals.Length; i++)
            {
                DrawBones(fingerMetacarpals[i], fingerBasis);
            }

            DrawBones(thumbMetacarpal, fingerBasis);

            DrawPalm(wrist, wristBasis);
        }
    }
}