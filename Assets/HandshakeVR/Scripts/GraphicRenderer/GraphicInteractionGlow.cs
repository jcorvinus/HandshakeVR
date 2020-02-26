using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if LeapGraphicRenderer
using Leap.Unity.GraphicalRenderer;
#endif

using Leap.Unity;
using Leap.Unity.Interaction;

namespace HandshakeVR
{
    public class GraphicInteractionGlow : MonoBehaviour
    {

        [Tooltip("If enabled, the object will lerp to its hoverColor when a hand is nearby.")]
        public bool useHover = true;

        [Tooltip("If enabled, the object will use its primaryHoverColor when the primary hover of an InteractionHand.")]
        public bool usePrimaryHover = false;

        [Header("InteractionBehaviour Colors")]
        public Color defaultColor = Color.Lerp(Color.black, Color.white, 0.1F);
        public Color suspendedColor = Color.red;
        public Color hoverColor = Color.Lerp(Color.black, Color.white, 0.7F);
        public Color primaryHoverColor = Color.Lerp(Color.black, Color.white, 0.8F);

        [Header("InteractionButton Colors")]
        [Tooltip("This color only applies if the object is an InteractionButton or InteractionSlider.")]
        public Color pressedColor = Color.white;

        private InteractionBehaviour _intObj;

#if LeapGraphicRenderer
        private LeapGraphic _graphic;
#endif

        void Start()
        {
            _intObj = GetComponent<InteractionBehaviour>();

#if LeapGraphicRenderer
            _graphic = GetComponent<LeapGraphic>();

            if(!_graphic)
            {
                _graphic = GetComponentInChildren<LeapGraphic>();
            }
#endif
        }

        void Update()
        {
#if LeapGraphicRenderer
            if (_graphic != null)
            {

                // The target color for the Interaction object will be determined by various simple state checks.
                Color targetColor = defaultColor;

                // "Primary hover" is a special kind of hover state that an InteractionBehaviour can
                // only have if an InteractionHand's thumb, index, or middle finger is closer to it
                // than any other interaction object.
                if (_intObj.isPrimaryHovered && usePrimaryHover)
                {
                    targetColor = primaryHoverColor;
                }
                else
                {
                    // Of course, any number of objects can be hovered by any number of InteractionHands.
                    // InteractionBehaviour provides an API for accessing various interaction-related
                    // state information such as the closest hand that is hovering nearby, if the object
                    // is hovered at all.
                    if (_intObj.isHovered && useHover)
                    {
                        float glow = _intObj.closestHoveringControllerDistance.Map(0F, 0.2F, 1F, 0.0F);
                        targetColor = Color.Lerp(defaultColor, hoverColor, glow);
                    }
                }

                if (_intObj.isSuspended)
                {
                    // If the object is held by only one hand and that holding hand stops tracking, the
                    // object is "suspended." InteractionBehaviour provides suspension callbacks if you'd
                    // like the object to, for example, disappear, when the object is suspended.
                    // Alternatively you can check "isSuspended" at any time.
                    targetColor = suspendedColor;
                }

                // We can also check the depressed-or-not-depressed state of InteractionButton objects
                // and assign them a unique color in that case.
                if (_intObj is InteractionButton && (_intObj as InteractionButton).isPressed)
                {
                    targetColor = pressedColor;
                }

                // Lerp actual material color to the target color.
                _graphic.SetRuntimeTint(Color.Lerp(_graphic.GetRuntimeTint(), targetColor, 30F * Time.deltaTime));
            }
#endif
        }
    }
}