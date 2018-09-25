using System;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Query;
using Leap.Unity.Space;
using UnityEngine.Serialization;

using Valve.VR;
using Leap.Unity;
using Leap.Unity.Interaction;

namespace CoordinateSpaceConversion
{
    public class SteamVRInteractionController : InteractionController
    {
        ContactBone[] _contactBones = new ContactBone[] { };
        ControllerType _controllerType = ControllerType.XRController;

        [SerializeField] bool _isLeft;
        SkeletalControllerHand skeletalControllerHand;
        SteamVRRemapper steamVRRemapper;

        [SerializeField]
        SteamVR_Action_Boolean grabAction;

        [SerializeField]
        SteamVR_Action_Boolean grabPinchAction;

        [SerializeField]
        public new List<Transform> primaryHoverPoints;

        [SerializeField]
        float disableContactAfterGraspTime = 0.25f;
        float disableContactTimer = 0;

        [SerializeField]
        InteractionHand handToOverride;

        ProviderSwitcher switcher;

        private List<Vector3> _graspManipulatorPoints = new List<Vector3>();

        public float maxGraspDistance = 0.06F;

        private bool _hasTrackedPositionLastFrame = false;
        private Vector3 _trackedPositionLastFrame = Vector3.zero;
        private Quaternion _trackedRotationLastFrame = Quaternion.identity;

        private bool _graspButtonLastFrame;
        /*private bool _graspButtonDown = false;
        private bool _graspButtonUp = false;
        private float _graspButtonDownSlopTimer = 0F;*/

        private void Awake()
        {
            SkeletalControllerHand[] controllerHands = FindObjectsOfType<SkeletalControllerHand>();

            foreach (SkeletalControllerHand controllerHand in controllerHands)
            {
                if (controllerHand.IsLeft == _isLeft)
                {
                    skeletalControllerHand = controllerHand;
                    break;
                }
            }

            steamVRRemapper = skeletalControllerHand.GetComponent<SteamVRRemapper>();
            _graspManipulatorPoints.Add(position);

            disableContactTimer = disableContactAfterGraspTime;
            _contactBones = new ContactBone[] { };

            switcher = FindObjectOfType<ProviderSwitcher>();
        }

        private void FixedUpdate()
        {
            if (isGraspingObject || handToOverride.isGraspingObject) disableContactTimer = 0;

            if(disableContactTimer <= disableContactAfterGraspTime)
            {
                disableContactTimer += Time.fixedDeltaTime;
            }

            if (handToOverride.contactBones != null)
            {
                bool setContactEnabled = disableContactTimer >= disableContactAfterGraspTime;
                if (handToOverride.contactEnabled != setContactEnabled) handToOverride.contactEnabled = setContactEnabled;
            }

            if (handToOverride.isGraspingObject)
            {
                // disable our grasp if we're grasping
                if (isGraspingObject) ReleaseGrasp();

                graspingEnabled = false;
            }
            else
            {
                if (!switcher.IsDefault)
                {
                    // only do this if our custom provider is enabled and working
                    graspingEnabled = true;
                    handToOverride.graspingEnabled = !isGraspingObject;
                }
                else
                {
                    graspingEnabled = false;
                    handToOverride.graspingEnabled = true;
                }               
            }
        }

        private void LateUpdate()
        {
            if(isTracked)
            {
                _hasTrackedPositionLastFrame = true;

                _trackedPositionLastFrame = position;
                _trackedRotationLastFrame = rotation;
            }
            else
            {
                _hasTrackedPositionLastFrame = false;
            }

            _graspButtonLastFrame = _graspButtonDown;
        }

        private IInteractionBehaviour _closestGraspableObject = null;
        private void refreshClosestGraspableObject()
        {
            _closestGraspableObject = null;

            float closestGraspableDistance = float.PositiveInfinity;
            foreach (var intObj in graspCandidates)
            {
                float testDist = intObj.GetHoverDistance(this.position);
                if (testDist < maxGraspDistance && testDist < closestGraspableDistance)
                {
                    _closestGraspableObject = intObj;
                    closestGraspableDistance = testDist;
                }
            }
        }

        public override ContactBone[] contactBones
        {
            get
            {
                return _contactBones;
            }
        }

        public override ControllerType controllerType
        {
            get
            {
                return _controllerType;
            }
        }

        public override List<Vector3> graspManipulatorPoints
        {
            get
            {
                _graspManipulatorPoints.Clear();
                _graspManipulatorPoints.Add(position);
                _graspManipulatorPoints.Add(hoverPoint + rotation * Vector3.forward * 0.05F);
                _graspManipulatorPoints.Add(hoverPoint + rotation * Vector3.right * 0.05F);

                return _graspManipulatorPoints;
            }
        }

        public override Vector3 hoverPoint
        {
            get
            {
                return position;
            }
        }

        public override InteractionHand intHand
        {
            get
            {
                return null;
            }
        }

        public override bool isBeingMoved
        {
            get
            {
                return false; // since this is used to hide the InteractionHand but we wane both of them to show up at once,
            }
        }

        public override bool isLeft
        {
            get
            {
                return _isLeft;
            }
        }

        public override bool isTracked
        {
            get
            {
                return steamVRRemapper.IsTracking;
            }
        }

        public override Vector3 position
        {
            get
            {
                return (skeletalControllerHand != null) ? skeletalControllerHand.GetPalmPosition() + (skeletalControllerHand.GetPalmNormal() * (maxGraspDistance)) : transform.position;
            }
        }

        public override Quaternion rotation
        {
            get
            {
                return (skeletalControllerHand != null) ? skeletalControllerHand.GetHandRotation() : transform.rotation;
            }
        }

        public override Vector3 velocity
        {
            get
            {
                if (_hasTrackedPositionLastFrame)
                {
                    return (this.transform.position - _trackedPositionLastFrame) / Time.fixedDeltaTime;
                }
                else
                {
                    return Vector3.zero;
                }
            }
        }

        protected override GameObject contactBoneParent
        {
            get
            {
                return gameObject;
            }
        }

        protected override List<Transform> _primaryHoverPoints
        {
            get
            {
                return primaryHoverPoints;
            }
        }

        public override Vector3 GetGraspPoint()
        {
            return skeletalControllerHand.GetPalmPosition();
        }

        private bool _graspButtonDown
        {
            get
            {
                return grabAction.GetStateDown((isLeft) ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
            }
        }

        private bool _graspButtonUp
        {
            get
            {
                return grabAction.GetStateUp((isLeft) ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
            }
        }

        protected override bool checkShouldGrasp(out IInteractionBehaviour objectToGrasp)
        {
            bool shouldGrasp = !isGraspingObject
                               && (_graspButtonDown)
                               && _closestGraspableObject != null;

            objectToGrasp = null;
            if (shouldGrasp) { objectToGrasp = _closestGraspableObject; }

            return shouldGrasp;
        }

        protected override bool checkShouldGraspAtemporal(IInteractionBehaviour intObj)
        {
            bool shouldGrasp = !isGraspingObject
                               && _graspButtonLastFrame
                               && intObj.GetHoverDistance(position) < maxGraspDistance;
            if (shouldGrasp)
            {
                var tempControllers = Pool<List<InteractionController>>.Spawn();
                try
                {
                    intObj.BeginGrasp(tempControllers);
                }
                finally
                {
                    tempControllers.Clear();
                    Pool<List<InteractionController>>.Recycle(tempControllers);
                }
            }

            return shouldGrasp;
        }

        protected override bool checkShouldRelease(out IInteractionBehaviour objectToRelease)
        {
            bool shouldRelease = _graspButtonUp && isGraspingObject;

            objectToRelease = null;
            if (shouldRelease) { objectToRelease = graspedObject; }

            return shouldRelease;
        }

        protected override void fixedUpdateGraspingState()
        {
            refreshClosestGraspableObject();
        }

        protected override void getColliderBoneTargetPositionRotation(int contactBoneIndex, out Vector3 targetPosition, out Quaternion targetRotation)
        {
            throw new NotImplementedException();
        }

        protected override bool initContact()
        {
            return true;
        }

        protected override void onObjectUnregistered(IInteractionBehaviour intObj)
        {

        }

        protected override void unwarpColliders(Transform primaryHoverPoint, ISpaceComponent warpedSpaceElement)
        {
            // Extension method calculates "unwarped" pose in world space.
            Vector3 unwarpedPosition;
            Quaternion unwarpedRotation;
            warpedSpaceElement.anchor.transformer.WorldSpaceUnwarp(primaryHoverPoint.position,
                                                                   primaryHoverPoint.rotation,
                                                                   out unwarpedPosition,
                                                                   out unwarpedRotation);

            // no colliders to operate on so we won't do anything here.
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(position, maxGraspDistance);
        }
    }
}