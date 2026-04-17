using UnityEngine;

public class ConstrainedHingeJoint : FABRIKJoint
{
    private enum hingeType
    {
        Leg,
        Arm
    }
    [SerializeField] ProjectionAxis axis = ProjectionAxis.X;
    [SerializeField] float MinAngle;
    [SerializeField] float MaxAngle;
    [SerializeField] float offset = 0;
    [SerializeField] hingeType limbType;


    public override Vector3 constrain(Vector3 L, Vector3 target)
    {

        //only really need X constraint for elbows and knees
        if(limbType == hingeType.Arm)
        {
            return ArmHingeOnParentX(L, target);

        }
        else
        {
            return LegHinge(L, target);
        }

    }

    private Vector3 LegHinge(Vector3 L, Vector3 target)
    {
        //rotate target from local constrain vector (ex. transform.forward) to worldspace constraint vector
        //then offset so target vector originates from the Origin
        Quaternion LtoW = Quaternion.FromToRotation(parentJoint.transform.right, Vector3.right);
        target = LtoW * (target - L);

        //Project target onto our constraint plane, normalize, and find theta for constraint
        Vector3 OProj = Vector3.ProjectOnPlane(target, Vector3.right);
        OProj = OProj.normalized;
        float theta = Mathf.Atan2(OProj.y, OProj.z);

        //adjust theta from -PI to Pi range to a 0-2PI range
        if (theta < 0)
        {
            theta = theta + (2 * Mathf.PI);
        }
        //This part actually works quite well, keeps the knee limited by thigh angle
        float upperLegAngle = -Vector3.SignedAngle(Vector3.down, parentJoint.transform.forward, parentJoint.transform.right);
        MaxAngle = (270 + upperLegAngle);
        MinAngle = Mathf.Max(90, 90 + upperLegAngle);
        if (offset == 1)
        {
            //print(theta * Mathf.Rad2Deg);
        }
        //create our new position incase we don't need constraints
        Vector3 newPos = new Vector3(0, OProj.y, OProj.z);
        //if theta is large enough to be constrained, or small enough to ignore the mininum use max
        //otherwise use min
        if (theta > (MaxAngle * Mathf.Deg2Rad) || theta * Mathf.Rad2Deg < 90)
        {
            theta = MaxAngle * Mathf.Deg2Rad;
            newPos.z = Mathf.Cos(theta);
            newPos.y = Mathf.Sin(theta);
        }else if (theta < (MinAngle * Mathf.Deg2Rad))
        {
            theta = MinAngle * Mathf.Deg2Rad;
            newPos.z = Mathf.Cos(theta);
            newPos.y = Mathf.Sin(theta);
        }
        /*
        newPos.z = Mathf.Cos(MaxAngle * Mathf.Deg2Rad);
        newPos.y = Mathf.Sin(MaxAngle * Mathf.Deg2Rad);
        */
        //rotate back to parent's local space, normalize, and offset back to joint pos using L
        return (Quaternion.Inverse(LtoW) * (newPos)).normalized * segmentLen + L;
    }

    private Vector3 ArmHingeOnParentX(Vector3 L, Vector3 target)
    {
        //rotate target from local constrain vector (ex. transform.forward) to worldspace constraint vector
        //then offset so target vector originates from the Origin
        Quaternion LtoW = Quaternion.FromToRotation(parentJoint.transform.right, Vector3.right);
        target = LtoW * (target - L);

        //Project target onto our constraint plane, normalize, and find theta for constraint
        Vector3 OProj = Vector3.ProjectOnPlane(target, Vector3.right);
        OProj = OProj.normalized;
        float theta = Mathf.Atan2(OProj.y, OProj.z);

        //adjust theta from -PI to Pi range to a 0-2PI range
        if (theta < 0)
        {
            theta = theta + (2 * Mathf.PI);
        }
        //create our new position incase we don't need constraints
        Vector3 newPos = new Vector3(0, OProj.y, OProj.z);

        if( !(theta > (MinAngle * Mathf.Deg2Rad) || theta < (MaxAngle * Mathf.Deg2Rad)))
        {
            if(theta - MinAngle * Mathf.Deg2Rad >= (MaxAngle * Mathf.Deg2Rad) - theta)
            {
                theta = MaxAngle * Mathf.Deg2Rad;
            }
            else
            {
                theta = MinAngle * Mathf.Deg2Rad;
            }
            newPos.z = Mathf.Cos(theta);
            newPos.y = Mathf.Sin(theta);

        }
        //rotate back to parent's local space, normalize, and offset back to joint pos using L
        return (Quaternion.Inverse(LtoW) * (newPos)).normalized * segmentLen + L;
    }

    private Vector3 HingeWORLDZ(Vector3 L, Vector3 target)
    {
        Vector3 O = Vector3.Project(target, L);
        float dist = (O - L).magnitude;
        //should be transform.forward
        Vector3 OProj = Vector3.ProjectOnPlane(target, Vector3.forward);
        Vector3 LProj = Vector3.ProjectOnPlane(L, Vector3.forward);
        //both O and L are projected onto the XZ plane, O is shifted by L so t  at it is at the origin
        //shift OProj to the origin
        OProj -= LProj;
        OProj = OProj.normalized;

        float theta = Mathf.Atan2(OProj.y, OProj.x);
        //adjust theta from -PI to Pi range to a 0-2PI range
        if(theta < 0)
        {
            theta = Mathf.Abs(theta+Mathf.PI) + Mathf.PI;
        }

        Vector3 newPos = new Vector3(OProj.x, OProj.y, 0);

        float adjustedAngle = theta;
        
        if(theta < MinAngle * Mathf.Deg2Rad || theta > MaxAngle * Mathf.Deg2Rad){

            //FIXME find the min arc to both constraining angles and set adjusted angle to the closer one
            float maxArc = Mathf.Abs(theta - (MaxAngle * Mathf.Deg2Rad)) % 2*Mathf.PI;
            float minArc = (theta < MinAngle * Mathf.Deg2Rad) ? MinAngle * Mathf.Deg2Rad - theta : (2 * Mathf.PI) - Mathf.Abs(theta - MinAngle * Mathf.Deg2Rad);

            minArc = ((MinAngle * Mathf.Deg2Rad) + 2 * Mathf.PI - theta) % 2*Mathf.PI;



            //new idea
            Vector3 minPos;
            Vector3 maxPos;
            minPos.y = Mathf.Sin(MinAngle * Mathf.Deg2Rad);
            minPos.x = Mathf.Cos(Mathf.Deg2Rad * MinAngle);
            minPos.z = 0;
            maxPos.y = Mathf.Sin(MaxAngle * Mathf.Deg2Rad);
            maxPos.x = Mathf.Cos(Mathf.Deg2Rad * MaxAngle);
            maxPos.z = 0;

            if(Vector3.Distance(minPos, OProj) >= Vector3.Distance(maxPos, OProj))
            {
                newPos = minPos;
            }
            else
            {
                newPos = maxPos;
            }
        }
        newPos = newPos.normalized * segmentLen + L;
        return newPos;
    }


}
