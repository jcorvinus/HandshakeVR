using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace HandshakeVR.Demo
{
    public class AlertWindow : MonoBehaviour
    {
        enum Corner { UpperLeft, UpperRight, LowerRight, LowerLeft }

        [SerializeField]
        Alert alert;

        [SerializeField]
        Transform upperLeft, upperRight, lowerRight, lowerLeft;
        float boneLocalZValue;

        [SerializeField]
        Text[] text;
        Color textDefaultColor;

        [SerializeField]
        Image image;
        Color imageDefaultColor;

        [SerializeField]
        float windowSizeTime = 0.34f;

        [SerializeField]
        float contentFadeTime = 0.125f;

        [SerializeField]
        Vector2 dimensions;

        [SerializeField]
        SkinnedMeshRenderer skinnedMeshRenderer;

        private void Awake()
        {
            textDefaultColor = text[0].color;
            foreach (Text textItem in text)
            {
                textItem.color = Color.clear;
            }

            imageDefaultColor = (image != null) ? image.color : Color.clear;
            if(image != null) image.color = Color.clear;

            boneLocalZValue = transform.InverseTransformPoint(upperLeft.position).z;

            skinnedMeshRenderer.enabled = false;
        }

        private void Start()
        {
            alert.OnAlert.AddListener(ExpandWindow);
        }

        void CalcBounds()
        {
            Vector3 upperLeftGlobal = transform.TransformPoint(GetMaxPosition(Corner.UpperLeft));
            Vector3 upperRightGlobal = transform.TransformPoint(GetMaxPosition(Corner.UpperRight));
            Vector3 lowerLeftGlobal = transform.TransformPoint(GetMaxPosition(Corner.LowerLeft));
            Vector3 lowerRightGlobal = transform.TransformPoint(GetMaxPosition(Corner.LowerRight));

            Vector3 centerGlobal = Vector3.zero;
            centerGlobal += upperLeftGlobal;
            centerGlobal += upperRightGlobal;
            centerGlobal += lowerLeftGlobal;
            centerGlobal += lowerRightGlobal;
            centerGlobal /= 4;

            Vector3 centerLocal = skinnedMeshRenderer.transform.InverseTransformPoint(centerGlobal);
            Vector3 upperLeftLocal = skinnedMeshRenderer.transform.InverseTransformPoint(upperLeftGlobal);
            Vector3 upperRightLocal = skinnedMeshRenderer.transform.InverseTransformPoint(upperRightGlobal);
            Vector3 lowerLeftLocal = skinnedMeshRenderer.transform.InverseTransformPoint(lowerLeftGlobal);
            Vector3 lowerRightLocal = skinnedMeshRenderer.transform.InverseTransformPoint(lowerRightGlobal);

            Vector3 sizeLocal = new Vector3(
                Vector3.Distance(upperLeftLocal, lowerLeftLocal),
                Vector3.Distance(upperLeftLocal, upperRightLocal),
                skinnedMeshRenderer.localBounds.size.z);
            skinnedMeshRenderer.localBounds = new Bounds(centerLocal, sizeLocal);
        }

        void ExpandWindow()
        {
            CalcBounds();

            StartCoroutine(ScaleWindow(windowSizeTime));
            StartCoroutine(FadeContents(windowSizeTime * 0.85f, contentFadeTime));
        }
            
        /// <summary>
        /// Gets the expanded position of a corner, in local space
        /// </summary>
        /// <param name="corner"></param>
        /// <returns></returns>
        Vector3 GetMaxPosition(Corner corner)
        {
            Vector2 startPoint = Vector2.zero;

            switch (corner)
            {
                case Corner.UpperLeft:
                    Vector2 upperLeft = startPoint + Vector2.Scale(dimensions, new Vector2(0, 1));
                    return new Vector3(upperLeft.x, upperLeft.y, boneLocalZValue);

                case Corner.UpperRight:
                    Vector2 upperRight = startPoint + dimensions;
                    return new Vector3(upperRight.x * -1, upperRight.y, boneLocalZValue);

                case Corner.LowerRight:
                    Vector2 lowerRight = startPoint + Vector2.Scale(dimensions, new Vector2(1, 0));
                    return new Vector3(lowerRight.x * -1, lowerRight.y, boneLocalZValue);
                        
                case Corner.LowerLeft:
                    return new Vector3(startPoint.x, startPoint.y, boneLocalZValue);

                default:
                    return new Vector3(startPoint.x, startPoint.y, boneLocalZValue);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// lerp with an exponential tvalue
        /// </summary>
        public static float Exerp(float from, float to, float t)
        {
            return Mathf.Lerp(from, to, t * t);
        }

        IEnumerator ScaleWindow(float duration)
        {
            float time = 0;
            float tValue = 0;

            Vector3 startPos = transform.InverseTransformPoint(new Vector3(0, 0, boneLocalZValue));
            Vector3 upperLeftGoalPos = transform.TransformPoint(GetMaxPosition(Corner.UpperLeft));
            Vector3 upperRightGoalPos = transform.TransformPoint(GetMaxPosition(Corner.UpperRight));
            Vector3 lowerLeftGoalPos = transform.TransformPoint(GetMaxPosition(Corner.LowerLeft));
            Vector3 lowerRightGoalPos = transform.TransformPoint(GetMaxPosition(Corner.LowerRight));

            while (time < duration)
            {
                time += Time.deltaTime;
                tValue = Mathf.InverseLerp(0, duration, time);
                tValue = Exerp(0, 1, tValue);

                // tween our corners here
                upperLeft.position = Vector3.Lerp(startPos, upperLeftGoalPos, tValue);
                upperRight.position = Vector3.Lerp(startPos, upperRightGoalPos, tValue);
                lowerLeft.position = Vector3.Lerp(startPos, lowerLeftGoalPos, tValue);
                lowerRight.position = Vector3.Lerp(startPos, lowerRightGoalPos, tValue);

                skinnedMeshRenderer.enabled = true;
                yield return null;
            }
        }

        IEnumerator FadeContents(float delay, float duration)
        {
            float time = 0;
            float tValue = 0;

            yield return new WaitForSeconds(delay);

            while(time < duration)
            {
                time += Time.deltaTime;
                tValue = Mathf.InverseLerp(0, duration, time);

                foreach (Text textItem in text)
                {
                    textItem.color = Color.Lerp(Color.clear, textDefaultColor, tValue);
                }

                if (image != null) image.color = Color.Lerp(Color.clear, imageDefaultColor, tValue);

                yield return null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 offset = dimensions * 0.5f;
            offset = Vector2.Scale(offset, new Vector2(-1, 1));

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(offset, (Vector3)dimensions + Vector3.forward * 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere((GetMaxPosition(Corner.LowerLeft)), 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere((GetMaxPosition(Corner.UpperLeft)), 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere((GetMaxPosition(Corner.LowerRight)), 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere((GetMaxPosition(Corner.UpperRight)), 0.1f);

            Gizmos.matrix = Matrix4x4.identity;  
        }
    }
}