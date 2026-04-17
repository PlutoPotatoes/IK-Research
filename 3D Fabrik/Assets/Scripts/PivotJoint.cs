using UnityEngine;

public class PivotJoint : FABRIKJoint
{

    public override Vector3 constrain(Vector3 L, Vector3 target)
    {
        //only really need X constraint for elbows and knees
        return transform.position + parentJoint.transform.position.normalized * segmentLen;

    }
}
