using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FABRIKHingeJoint : FABRIKJoint
{
    public enum HingeAxis{
        XHinge,
        YHinge,
        ZHinge
    };
    [SerializeField] HingeAxis hingeAxis;
    

    public override Quaternion ConstrainRotation(Quaternion startRotation)
    {
        Vector3 twistAxis;

        switch (hingeAxis)
        {
            case HingeAxis.XHinge:
                twistAxis = Vector3.right;
                break;
            case HingeAxis.YHinge:
                twistAxis = Vector3.up;
                break;
            case HingeAxis.ZHinge:
                twistAxis = Vector3.forward;
                break;
            default:
                twistAxis = Vector3.right;
                break;
        }
        // Rotate the twist axis by the quaternion to get the actual twisted direction
        Vector3 rotatedTwist = startRotation * twistAxis;

        // Project the rotated vector onto a plane orthogonal to Z (i.e., remove Z component)
        Vector3 flattened = Vector3.ProjectOnPlane(rotatedTwist, twistAxis).normalized;

        // if the rotation is already on the Z axis return identity
        if (flattened == Vector3.zero)
        {
            return startRotation;
        }

        // Create a new quaternion that aligns the twist axis with the projected vector
        return Quaternion.LookRotation(flattened, twistAxis);
    }
}
