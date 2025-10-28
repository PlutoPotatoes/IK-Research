using UnityEngine;

public class ConstrainedHingeJoint : FABRIKJoint
{

    [SerializeField] ProjectionAxis axis = ProjectionAxis.X;
    [SerializeField] Sidedness side;
    [SerializeField] GameObject parentJoint;
    [SerializeField] float MinAngle;
    [SerializeField] float MaxAngle;
    private int sideMultiplier = 1;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(side == Sidedness.negative)
        {
            sideMultiplier = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override Vector3 constrain(Vector3 L, Vector3 target)
    {
        //only really need X constraint for elbows and knees
        return HingeOnParentX(L, target);

    }

    private Vector3 HingeOnParentX(Vector3 L, Vector3 target)
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
            theta = Mathf.Abs(theta + Mathf.PI) + Mathf.PI;
        }
        //create our new position incase we don't need constraints
        Vector3 newPos = new Vector3(0, OProj.y, OProj.z);

        //check if theta is outside our allowed range
        if (theta < MinAngle * Mathf.Deg2Rad || theta > MaxAngle * Mathf.Deg2Rad)
        {
            //create both constrained max and min positions
            Vector3 minPos;
            Vector3 maxPos;
            minPos.y = Mathf.Sin(MinAngle * Mathf.Deg2Rad);
            minPos.x = Mathf.Cos(Mathf.Deg2Rad * MinAngle);
            minPos.z = 0;
            maxPos.y = Mathf.Sin(MaxAngle * Mathf.Deg2Rad);
            maxPos.x = Mathf.Cos(Mathf.Deg2Rad * MaxAngle);
            maxPos.z = 0;
            //set newPos to the closer constraint position
            if (Vector3.Distance(minPos, OProj) >= Vector3.Distance(maxPos, OProj))
            {
                newPos = new Vector3(0, minPos.y, minPos.x);
            }
            else
            {
                newPos = new Vector3(0, maxPos.y, maxPos.x);
            }
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
