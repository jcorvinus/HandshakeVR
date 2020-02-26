using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HandshakeVR
{
    [CustomEditor(typeof(SkeletalControllerHand))]
    public class SkeletalControllerHandEditor : Editor
    {
        SkeletalControllerHand controllerHand;

        private void OnEnable()
        {
            controllerHand = target as SkeletalControllerHand;
        }

        void DrawConstraint(Transform transform, float minAngle, float maxAngle)
        {
            if (transform == null) return;

            Handles.matrix = transform.parent.localToWorldMatrix;

            float wrappedMin = minAngle;

            if (wrappedMin < 0)
            {
                wrappedMin = 360 + wrappedMin;
            }

            bool constrainInside = true;
            float angleDelta = maxAngle - minAngle;

            angleDelta = (constrainInside) ? angleDelta : 360 - angleDelta;
            Vector3 center = transform.localPosition;

            Handles.DrawWireArc(center, Vector3.forward,
                Quaternion.AngleAxis((constrainInside) ? maxAngle : minAngle, Vector3.forward) *
                Vector3.right, -angleDelta, 0.01f);

            Handles.matrix = Matrix4x4.identity;
        }

        private void OnSceneGUI()
        {
            for(int i=0; i < controllerHand.ConstraintCount; i++)
            {
                SkeletalControllerHand.BoneConstraint constraint = controllerHand.GetConstraintAtIndex(i);

                Transform boneTransform = constraint.BoneToConstrain;
                float minAngle = constraint.StartAngle;
                float maxAngle = constraint.EndAngle;

                DrawConstraint(boneTransform, minAngle, maxAngle);
            }
        }
    }
}