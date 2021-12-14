using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity;

namespace HandshakeVR
{
    public static class HandProperties
    {
        /// <summary>
        /// Converts a Leap.Finger.FingerType enum type to a FingerFilter enum.
        /// 
        /// There are two possible failure states: being fed an enum as integer that is out of range (you have to try hard to do this), and 
        /// trying to convert a FingerFilter.none to Leap.Finger.FingerType, as FingerType has no equivalent.
        /// 
        /// Upon failure, the method will throw a System.NotSupported exception with details about the failure.
        /// </summary>
        /// <param name="finger">Finger filter to convert.</param>
        /// <returns></returns>
        public static Finger.FingerType FingerTypeFromFingerFilter(FingerFilter finger)
        {
            switch (finger)
            {
                case FingerFilter.none:
                    throw new System.NotSupportedException("Error in HandProperties.cs: FingerFilter.none cannot map to Leap.Finger.FingerType. No equivalent exists.");
                    return Finger.FingerType.TYPE_THUMB; // all thumbs, lol

                case FingerFilter.thumb:
                    return Finger.FingerType.TYPE_THUMB;

                case FingerFilter.index:
                    return Finger.FingerType.TYPE_INDEX;

                case FingerFilter.middle:
                    return Finger.FingerType.TYPE_MIDDLE;

                case FingerFilter.ring:
                    return Finger.FingerType.TYPE_RING;

                case FingerFilter.pinky:
                    return Finger.FingerType.TYPE_PINKY;

                default:
                    throw new System.NotSupportedException("Error in HandProperties.cs: An out of bounds enum value " + ((int)finger).ToString() + " was supplied.");
                    return Finger.FingerType.TYPE_THUMB; // all thumbs, lol
            }
        }

        /// <summary>
        /// Converts a FingerFilter enum type to a Leap.Finger.FingerType enum.
        /// 
        /// There is only one possible failure state: being fed an enum as an integer that is out of range.
        /// 
        /// Upon failure, the method will throw a System.NotSupported exception with details about the failure.
        /// </summary>
        /// <param name="fingerType">FingerType to convert.</param>
        /// <returns></returns>
        public static FingerFilter FingerFilterFromFingerType(Leap.Finger.FingerType fingerType)
        {
            switch (fingerType)
            {
                case Finger.FingerType.TYPE_INDEX:
                    return FingerFilter.index;

                case Finger.FingerType.TYPE_MIDDLE:
                    return FingerFilter.middle;

                case Finger.FingerType.TYPE_PINKY:
                    return FingerFilter.pinky;

                case Finger.FingerType.TYPE_RING:
                    return FingerFilter.ring;

                case Finger.FingerType.TYPE_THUMB:
                    return FingerFilter.thumb;

                default:
                    throw new System.NotSupportedException("Error in HandProperties.cs: An out of bounds enum value " + ((int)fingerType).ToString() + " was supplied.");
                    return FingerFilter.thumb;
            }
        }

        /// <summary>
        /// Returns a 
        /// </summary>
        /// <param name="isLeft"></param>
        /// <returns>Either UserHand.Handedness.Left or UserHand.Handedness.Right depending on the input value.</returns>
        public static UserHand HandFilterFromSide(bool isLeft)
        {
            if (isLeft) return UserRig.Instance.LeftHand;
            else return UserRig.Instance.RightHand;
        }
    }
}