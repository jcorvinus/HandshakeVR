﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CatchCo;

using Leap;
using Leap.Unity;

namespace CoordinateSpaceConversion
{
    public class SkeletalControllerHand : MonoBehaviour
    {
        [SerializeField]
        LeapProvider provider;

        /*TYPE_THUMB = 0,
        TYPE_INDEX = 1,
        TYPE_MIDDLE = 2,
        TYPE_RING = 3,
        TYPE_PINKY = 4,*/

        [SerializeField] float palmWidth;
        [SerializeField] float palmForwardOfffset = 0;
        [SerializeField] float palmNormalOffset = 0;

        [SerializeField] float fingerWidth = 8;

        float forearmLength = 0.27f;

        [SerializeField]
        Vector3 modelPalmFacing;

        [SerializeField]
        Vector3 modelFingerPointing;

        [SerializeField]
        Transform thumbMetaCarpal;

        [SerializeField]
        Transform indexMetaCarpal;

        [SerializeField]
        Transform middleMetaCarpal;

        [SerializeField]
        Transform ringMetaCarpal;

        [SerializeField]
        Transform pinkyMetaCarpal;

        [SerializeField]
        Transform wrist;

        public Transform Wrist { get { return wrist; } }

        public Transform IndexMetacarpal { get { return indexMetaCarpal; } }
        public Transform MiddleMetacarpal { get { return middleMetaCarpal; } }
        public Transform RingMetacarpal { get { return ringMetaCarpal; } }
        public Transform PinkyMetacarpal { get { return pinkyMetaCarpal; } }

        public Transform ThumbMetacarpal { get { return thumbMetaCarpal; } }

        [SerializeField]
        bool isLeft;

        public bool IsLeft { get { return isLeft; } }

        [Header("Debug vars")]
        [SerializeField]
        bool drawBones = true;
        [SerializeField]
        bool drawBasis = true;

        protected Color[] colors = { Color.gray, Color.yellow, Color.cyan, Color.magenta };

        public static readonly float MM_TO_M = 1e-3f;

        float visibleTime = 0;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            visibleTime += Time.deltaTime;
        }

        Bone GenerateBone(Transform prev, Transform next, Bone.BoneType type, float fingerWidth)
        {
            Vector3 up, forward, right;
            GetBasis(prev, out right, out forward, out up);
            float metaDist = Vector3.Distance(prev.position, next.position);
            Vector3 metaCenter = (prev.position + next.position) * 0.5f;

            return new Bone(prev.position.ToVector(), next.position.ToVector(),
                metaCenter.ToVector(), forward.ToVector(), metaDist, fingerWidth,
                type, Quaternion.LookRotation(forward, up).ToLeapQuaternion());
        }

        Finger GenerateFinger(int frameID, int handID, Finger.FingerType fingerType,
            float timeVisible, Transform metaCarpalTransform)
        {
            float _fingerWidth = fingerWidth * MM_TO_M;

            // NOTE: if our type is thumb, our 'meta carpal transform' is actually our proximal,
            // and we'll need to generate a zero-length metacarpal for it
            Transform proximalTransform = (fingerType == Finger.FingerType.TYPE_THUMB) ? metaCarpalTransform : metaCarpalTransform.GetChild(0);
            Transform intermediateTransform = proximalTransform.GetChild(0);
            Transform distalTransform = intermediateTransform.GetChild(0);
            Transform tip = distalTransform.GetChild(0);

            Bone metaCarpal, proximal, intermediate, distal;
            metaCarpal = GenerateBone(metaCarpalTransform, proximalTransform, Bone.BoneType.TYPE_METACARPAL, _fingerWidth);
            proximal = GenerateBone(proximalTransform, intermediateTransform, Bone.BoneType.TYPE_PROXIMAL, _fingerWidth);
            intermediate = GenerateBone(intermediateTransform, distalTransform, Bone.BoneType.TYPE_INTERMEDIATE, _fingerWidth);
            distal = GenerateBone(distalTransform, tip, Bone.BoneType.TYPE_DISTAL, _fingerWidth);

            Vector tipPosition = tip.transform.position.ToVector();
            Vector direction = new Vector(0,0,0);

            float fingerLength = 
                Vector3.Distance(proximalTransform.position, intermediateTransform.position) +
                Vector3.Distance(intermediateTransform.position, distalTransform.position ) +
                Vector3.Distance(distalTransform.position, tip.position); // add up joint lengths for this

            return new Finger(frameID, handID, handID + (int)fingerType, timeVisible,
                tipPosition, direction, _fingerWidth, fingerLength, true, fingerType,
                metaCarpal, proximal, intermediate, distal);
        }

        public Hand GenerateHandData(int frameID)
        {
            //int frameID = 0;
            int handID = !isLeft ? 0 : 1;

            List<Finger> fingers = new List<Finger>();

            fingers.Add(GenerateFinger(0, handID, Finger.FingerType.TYPE_THUMB, visibleTime,
                thumbMetaCarpal));

            fingers.Add(GenerateFinger(0, handID, Finger.FingerType.TYPE_INDEX, visibleTime,
                indexMetaCarpal));

            fingers.Add(GenerateFinger(0, handID, Finger.FingerType.TYPE_MIDDLE, visibleTime,
                middleMetaCarpal));

            fingers.Add(GenerateFinger(0, handID, Finger.FingerType.TYPE_RING, visibleTime,
                ringMetaCarpal));

            fingers.Add(GenerateFinger(0, handID, Finger.FingerType.TYPE_PINKY, visibleTime,
                pinkyMetaCarpal));


            // forearm length is 0.27
            // forearm width is 0.09

            Vector forearmStart, forearmEnd;

            forearmStart = GetForearmStart().ToVector();
            forearmEnd = GetForearmEnd().ToVector();

            Quaternion forearmRotation = GetForearmRotation();

            Arm arm = new Arm(forearmStart,forearmEnd, (forearmStart + forearmEnd) * 0.5f,
                (forearmEnd - forearmStart).Normalized, forearmLength, 0.09f,
                forearmRotation.ToLeapQuaternion());
           
            Vector palmPosition = GetPalmPosition().ToVector();
            Vector palmNormal = GetPalmNormal().ToVector();
            Vector palmVelocity = new Vector(0,0,0);

            //palmWidth = 85f * MM_TO_M;

            LeapQuaternion rotation = GetHandRotation().ToLeapQuaternion();

            Hand newHand = new Hand(frameID, handID, 1, 0, 0, 0, 0, palmWidth, isLeft, visibleTime, arm,
                fingers, palmPosition, palmPosition, palmVelocity, palmNormal,
                rotation, wrist.TransformDirection(modelPalmFacing).ToVector(), wrist.position.ToVector()); // maybe 'direction' is related to palm direction?

            return newHand;
        }

        void GenerateBones()
        {
            if (thumbMetaCarpal != null) DestroyImmediate(thumbMetaCarpal.gameObject);
            if (indexMetaCarpal != null) DestroyImmediate(indexMetaCarpal.gameObject);
            if (middleMetaCarpal != null) DestroyImmediate(middleMetaCarpal.gameObject);
            if (ringMetaCarpal != null) DestroyImmediate(ringMetaCarpal.gameObject);
            if (pinkyMetaCarpal != null) DestroyImmediate(pinkyMetaCarpal.gameObject);

            #region Metacarpals
            // generate thumb metacarpal
            GameObject thumbObject = new GameObject("ThumbMeta");
            thumbObject.transform.SetParent(this.transform);
            thumbMetaCarpal = thumbObject.transform;

            // generate index metacarpal
            GameObject indexObject = new GameObject("IndexMeta");
            indexObject.transform.SetParent(this.transform);
            indexMetaCarpal = indexObject.transform;

            // generate middle metacarpal
            GameObject middleObject = new GameObject("MiddleMeta");
            middleObject.transform.SetParent(this.transform);
            middleMetaCarpal = middleObject.transform;

            // generate ring metacarpal
            GameObject ringObject = new GameObject("RingMeta");
            ringObject.transform.SetParent(this.transform);
            ringMetaCarpal = ringObject.transform;

            // generate pinky metacarpal
            GameObject pinkyObject = new GameObject("PinkyMeta");
            pinkyObject.transform.SetParent(this.transform);
            pinkyMetaCarpal = pinkyObject.transform;
            #endregion

            // generate child bones
            for(int i=0; i < 5; i++)
            {
                GenerateChildren(GetMetaCarpal((Leap.Finger.FingerType)i),
                    (i == 0)? 2 : 3);
            }
        }

        private void GetBasis(Transform reference, out Vector3 right, out Vector3 forward, out Vector3 up)
        {
            forward = modelFingerPointing;
            up = modelPalmFacing * -1;
            right = Vector3.Cross(forward, up);

            forward = reference.TransformDirection(forward);
            up = reference.TransformDirection(up);
            right = reference.TransformDirection(right);
        }

        public Quaternion GetLocalBasis()
        {
            Vector3 forward = modelFingerPointing;
            Vector3 up = modelPalmFacing * -1;
            return Quaternion.LookRotation(forward, up);
        }

        private void GenerateChildren(Transform parent, int childCount)
        {
            Transform prevTransform = parent;

            for(int i=0; i < parent.childCount;i++)
            {
                // clear any existing children
                DestroyImmediate(parent.GetChild(i).gameObject);
            }

            for(int i=0; i < childCount; i++)
            {
                GameObject newBone = new GameObject("Bone");
                newBone.transform.SetParent(prevTransform);
                prevTransform = newBone.transform;
            }
        }

        public Transform GetMetaCarpal(Leap.Finger.FingerType type)
        {
            switch (type)
            {
                case Finger.FingerType.TYPE_THUMB:
                    return thumbMetaCarpal;

                case Finger.FingerType.TYPE_INDEX:
                    return indexMetaCarpal;

                case Finger.FingerType.TYPE_MIDDLE:
                    return middleMetaCarpal;

                case Finger.FingerType.TYPE_RING:
                    return ringMetaCarpal;

                case Finger.FingerType.TYPE_PINKY:
                    return pinkyMetaCarpal;
                default:
                    return null;
            }
        }

        public Vector3 GetPalmPosition()
        {
            return wrist.position + (wrist.TransformDirection(modelPalmFacing) * palmNormalOffset) +
                (wrist.TransformDirection(modelFingerPointing) * palmForwardOfffset);
        }

        private Vector3 GetPalmNormal()
        {
            return wrist.transform.TransformDirection(modelPalmFacing);
        }

        private Vector3 GetForearmStart()
        {
            return (wrist.transform.position + wrist.TransformDirection(-modelFingerPointing) * forearmLength * 0.5f);
        }

        private Vector3 GetForearmEnd()
        {
            return wrist.transform.position;
        }

        public Quaternion GetHandRotation()
        {
            Vector3 upForearm, forwardForearm;

            upForearm = wrist.TransformDirection(-modelPalmFacing);
            forwardForearm = wrist.TransformDirection(modelFingerPointing);

            return Quaternion.LookRotation(forwardForearm, upForearm);
        }

        private Quaternion GetForearmRotation()
        {
            Vector3 upForearm, forwardForearm;

            upForearm = wrist.TransformDirection(-modelPalmFacing);
            forwardForearm = wrist.TransformDirection(modelFingerPointing);

            return Quaternion.LookRotation(forwardForearm, upForearm);
        }

        Quaternion InverseTransformQuaternion(Transform reference, Quaternion worldRoation)
        {
            return Quaternion.Inverse(reference.transform.rotation) * worldRoation;
        }

        void DrawBones(Transform parent, Transform child, int boneIndx)
        {
            Gizmos.DrawLine(parent.transform.position, child.transform.position);

            Gizmos.color = colors[boneIndx];

            if (drawBasis) DrawBasis(parent);

            if(child.childCount > 0)
            {
                DrawBones(child, child.GetChild(0), boneIndx + 1);
            }
        }

        void DrawBasis(Transform bone)
        {
            Vector3 forward = modelFingerPointing;
            Vector3 up = modelPalmFacing * -1;
            Vector3 right = Vector3.Cross(forward, up);

            Color storedColor = Gizmos.color;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(right) * 0.01f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(up) * 0.01f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(forward) * 0.01f);

            Gizmos.color = storedColor;
        }

        void DrawHand()
        {
            for (int fingerIndx = 0; fingerIndx < 5; fingerIndx++)
            {
                Transform metaCarpal = GetMetaCarpal((Finger.FingerType)fingerIndx);

                DrawBones(metaCarpal, metaCarpal.GetChild(0), 0);
            }
        }

        private void OnDrawGizmos()
        {
            // draw hand
            if (drawBones)
            {
                DrawHand();

                // draw palm width
                Gizmos.DrawWireCube(GetPalmPosition(), Vector3.right * palmWidth);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(wrist.transform.position, GetPalmPosition());
                Gizmos.color = Color.black;
                Gizmos.DrawLine(GetPalmPosition(), GetPalmPosition() + wrist.TransformDirection(modelPalmFacing) * 0.03f);

                // draw forearm
                Gizmos.color = Color.red;
                Gizmos.DrawLine(GetForearmStart(), GetForearmEnd());

                if(drawBasis)
                {
                    Quaternion forearmRotation = GetForearmRotation();
                    Vector3 forearmUp = forearmRotation * Vector3.up;
                    Vector3 forearmForward = forearmRotation * Vector3.forward;
                    Vector3 forearmRight = forearmRotation * Vector3.right;

                    Vector3 forearmCenter = (GetForearmStart() + GetForearmEnd()) * 0.5f;

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(forearmCenter, forearmCenter + forearmForward * 0.01f);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(forearmCenter, forearmCenter + forearmUp * 0.01f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(forearmCenter, forearmCenter + forearmRight * 0.01f);
                }
            }
        }
    }
}