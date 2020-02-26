using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConstraintTest))]
public class ConstraintTestEditor : Editor
{
    SerializedProperty startAngleProperty;
    SerializedProperty endAngleProperty;
    //SerializedProperty constrainInsideProperty;
    ConstraintTest m_instance;

    private void OnEnable()
    {
        m_instance = target as ConstraintTest;
        startAngleProperty = serializedObject.FindProperty("startAngle");
        endAngleProperty = serializedObject.FindProperty("endAngle");
        //constrainInsideProperty = serializedObject.FindProperty("constrainInside");
    }

    private void OnSceneGUI()
    {
        Handles.matrix = m_instance.transform.parent.localToWorldMatrix;

        float wrappedMin = startAngleProperty.floatValue;

        if(wrappedMin < 0)
        {
            wrappedMin = 360 + wrappedMin;
        }

        bool constrainInside = m_instance.IsConstrainedInside();

        float angleDelta = endAngleProperty.floatValue - startAngleProperty.floatValue;
        float startAngle = startAngleProperty.floatValue;
        float endAngle = endAngleProperty.floatValue;

        angleDelta = (constrainInside) ? angleDelta : 360 - angleDelta;
        Vector3 center = m_instance.transform.localPosition;

        Handles.DrawWireArc(center, Vector3.forward, 
            Quaternion.AngleAxis((constrainInside) ? endAngle : startAngle, Vector3.forward) *
            Vector3.right, -angleDelta, 0.01f);

        Handles.matrix = Matrix4x4.identity;
    }
}
