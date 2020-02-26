using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstraintTest : MonoBehaviour
{
    [Header("Green")]
    [Range(0,360)]
    [UnityEngine.Serialization.FormerlySerializedAs("minAngle")]
    [SerializeField]
    float startAngle;

    [Header("Blue")]
    [Range(0,360)]
    [UnityEngine.Serialization.FormerlySerializedAs("maxAngle")]
    [SerializeField]
    float endAngle;

    [SerializeField]
    bool applyConstraints;

    [SerializeField]
    float currentAngle;

    [SerializeField]
    float distToMax;

    [SerializeField]
    float distToMin;

	// Update is called once per frame
	void Update ()
    {
        currentAngle = transform.localRotation.eulerAngles.z;

        Vector3 euler = transform.localRotation.eulerAngles;

        float maxAngle, minAngle;

        maxAngle = (IsConstrainedInside()) ? startAngle : endAngle;
        minAngle = (IsConstrainedInside()) ? endAngle : startAngle;

        distToMax = Mathf.DeltaAngle(euler.z, maxAngle);
        distToMin = Mathf.DeltaAngle(euler.z, minAngle);

        if (applyConstraints)
        {
            /*if (euler.z > maxAngle) euler.z = maxAngle;
            else if (euler.z < minAngle) euler.z = minAngle;*/

            if (euler.z > maxAngle || euler.z < minAngle)
            {
                // move euler.z to closest angle
                euler.z = (Mathf.Abs(distToMax) < Mathf.Abs(distToMin)) ? maxAngle : minAngle;
            }
        }

        if(applyConstraints) transform.localRotation = Quaternion.Euler(
            new Vector3(0, 0, euler.z));
    }

    public bool IsConstrainedInside()
    {
        return startAngle > endAngle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.parent.localToWorldMatrix;

        // draw line to minAngle
        Gizmos.color = Color.green;
        Vector3 minPointRotated = Quaternion.AngleAxis(startAngle, Vector3.forward) * Vector3.right;
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + minPointRotated * 0.01f);

        // draw line to maxAngle
        Gizmos.color = Color.blue;
        Vector3 maxPointRotated = Quaternion.AngleAxis(endAngle, Vector3.forward) * Vector3.right;
        Gizmos.DrawLine(transform.localPosition, transform.localPosition + maxPointRotated * 0.01f);

        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.015f);
    }
}
