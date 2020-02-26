using System;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Query;
using Leap.Unity.Space;
using UnityEngine.Serialization;

using Valve.VR;
using Leap.Unity;
using Leap.Unity.Interaction;

namespace HandshakeVR
{
    public class SteamVRInteractionController : InteractionController
    {
        ContactBone[] _contactBones = new ContactBone[] { };
        ControllerType _controllerType = ControllerType.XRController;

        [SerializeField] bool _isLeft;
		[SerializeField] bool isPinchGrip; // if true, keep our position in between index and thumb.
        SkeletalControllerHand skeletalControllerHand;
        SteamVRRemapper steamVRRemapper;
		PinchDetector pinchGrabDetector;

        [SerializeField]
        SteamVR_Action_Boolean grabAction;

		[SerializeField]
		public new List<Transform> primaryHoverPoints = new List<Transform>(1);

        [SerializeField]
        InteractionHand handToOverride;

        ProviderSwitcher switcher;

        private List<Vector3> _graspManipulatorPoints = new List<Vector3>(3);

        public float maxGraspDistance = 0.06F;
		public float minGraspDistance = 0.02f;

        private bool _hasTrackedPositionLastFrame = false;
        private Vector3 _trackedPositionLastFrame = Vector3.zero;
        private Quaternion _trackedRotationLastFrame = Quaternion.identity;

        private bool _graspButtonLastFrame;

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

            _contactBones = new ContactBone[] { };

			primaryHoverPoints.Add(skeletalControllerHand.IndexMetacarpal.GetChild(0).GetChild(0).GetChild(0));

			switcher = FindObjectOfType<ProviderSwitcher>();
			pinchGrabDetector = GetComponent<PinchDetector>();
		}

		private void Start()
		{
			base.Start();

			if (isPinchGrip)
			{
				HandModelBase handModelBase = (_isLeft) ? switcher.LeftAbstractHandModel : switcher.RightAbstractHandModel;

				PinchDetector pinchDetector = pinchGrabDetector;
				pinchDetector.HandModel = handModelBase;
				pinchDetector.enabled = true;
			}
		}

		private float GetGrabDistance()
		{
			if (skeletalControllerHand == null) return maxGraspDistance;

			float grabDist = maxGraspDistance;
			if(isPinchGrip)
			{
				Vector3 indexTip = GetIndexFingertipPosition();
				Vector3 thumbTip = GetThumbtipPosition();

				grabDist = Vector3.Distance(indexTip, thumbTip) * 0.65f;
			}
			else
			{
				Vector3 middleTip = GetMiddleFingertipPosition();
				Vector3 indexTip = GetIndexFingertipPosition();

				float middleDist = Vector3.Distance(middleTip, skeletalControllerHand.GetPalmPosition()) * 0.5f;
				float indexDist = Vector3.Distance(indexTip, skeletalControllerHand.GetPalmPosition()) * 0.5f;

				grabDist = (middleDist + indexDist) * 0.5f;
			}

			return Mathf.Clamp(grabDist, minGraspDistance, maxGraspDistance);
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

            //if(!isPinchGrip) _graspButtonLastFrame = _graspButtonDown;
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
                _graspManipulatorPoints.Add(position + rotation * Vector3.forward * 0.05F);
                _graspManipulatorPoints.Add(position + rotation * Vector3.right * 0.05F);

                return _graspManipulatorPoints;
            }
        }

        public override Vector3 hoverPoint
        {
            get
            {
                return (skeletalControllerHand != null) ? skeletalControllerHand.LeapHand.Finger((int)Leap.Finger.FingerType.TYPE_INDEX).TipPosition.ToVector3() :
					position;
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
                return false; // since this is used to hide the InteractionHand but we want both of them to show up at once,
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
				//return (skeletalControllerHand != null) ? skeletalControllerHand.GetPalmPosition() + (skeletalControllerHand.GetPalmNormal() * (maxGraspDistance)) : transform.position;
				if (skeletalControllerHand == null || skeletalControllerHand.LeapHand == null) return transform.position;
				
				if(isPinchGrip)
				{
					Vector3 indexFingertip = GetIndexFingertipPosition();
					Vector3 thumbFingertip = GetThumbtipPosition();

					float grabDist = GetGrabDistance();

					return ((indexFingertip + thumbFingertip) * 0.5f) + (skeletalControllerHand.GetPalmNormal() * grabDist) * -0.5f;
				}
				else
				{
					return skeletalControllerHand.GetPalmPosition() + (skeletalControllerHand.GetPalmNormal() * GetGrabDistance() * 0.5f);
				}
            }
        }

		private Vector3 GetIndexFingertipPosition()
		{
			return skeletalControllerHand.LeapHand.Finger((int)Leap.Finger.FingerType.TYPE_INDEX).TipPosition.ToVector3();			
		}
		private Vector3 GetThumbtipPosition()
		{
			return skeletalControllerHand.LeapHand.Finger((int)Leap.Finger.FingerType.TYPE_THUMB).TipPosition.ToVector3();
		}

		private Vector3 GetMiddleFingertipPosition()
		{
			return skeletalControllerHand.LeapHand.Finger((int)Leap.Finger.FingerType.TYPE_MIDDLE).TipPosition.ToVector3();
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

		private bool _graspButtonDown;
		/*{
            get
            {
				if (isPinchGrip)
				{
					return pinchGrabDetector.ActivatedThisFrame;
				}
				else
				{
					return grabAction.GetStateDown((isLeft) ? SteamVR_Input_Sources.LeftHand :
						SteamVR_Input_Sources.RightHand);
				}
            }
        }*/

		private bool _graspButtonUp;
        /*{
            get
            {
				if (isPinchGrip)
				{
					return pinchGrabDetector.DeactivatedThisFrame;
				}
				else
				{
					return grabAction.GetStateUp((isLeft) ? SteamVR_Input_Sources.LeftHand : 
						SteamVR_Input_Sources.RightHand);
				}
            }
        }*/

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

			fixedUpdateGraspButtonState();
		}

		private void fixedUpdateGraspButtonState(bool ignoreTemporal = false)
		{
			_graspButtonDown = false;
			_graspButtonUp = false;

			/*bool graspButton = (isPinchGrip) ? pinchGrabDetector.IsActive : grabAction.state;

			if(graspButton != _graspButtonLastFrame)
			{
				if(graspButton)
				{
					_graspButtonDown = true;
				}
				else
				{
					_graspButtonUp = true;
				}
			}*/

			bool graspButton = _graspButtonLastFrame;

			if (!_graspButtonLastFrame)
			{
				graspButton = (isPinchGrip) ? pinchGrabDetector.IsActive : grabAction.state;

				if (graspButton)
				{
					// Grasp button was _just_ depressed this frame.
					_graspButtonDown = true;
				}
			}
			else
			{
				graspButton = (isPinchGrip) ? pinchGrabDetector.IsActive : grabAction.state;

				if (!graspButton)
				{
					// Grasp button was _just_ released this frame.
					_graspButtonUp = true;
				}
			}

			_graspButtonLastFrame = graspButton;
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
            /*Vector3 unwarpedPosition;
            Quaternion unwarpedRotation;
            warpedSpaceElement.anchor.transformer.WorldSpaceUnwarp(primaryHoverPoint.position,
                                                                   primaryHoverPoint.rotation,
                                                                   out unwarpedPosition,
                                                                   out unwarpedRotation);*/

            // no colliders to operate on so we won't do anything here.
        }

        private void OnDrawGizmos()
		{
			Gizmos.color = (isPinchGrip) ? Color.green : Color.white;
            Gizmos.DrawWireSphere(position, GetGrabDistance());
        }
    }
}