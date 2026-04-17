using UnityEngine;

public class ConstrainedFabrikJoint : FABRIKJoint
{


    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint13;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint23;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint33;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint43;
    [SerializeField] ProjectionAxis projectAxis = ProjectionAxis.Z;
    [SerializeField] bool isPivot = false;
    [SerializeField] Sidedness side;
    




    private int sideMultiplier;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (side) { 
            case Sidedness.positive:
                sideMultiplier = 1;
                break;
            case Sidedness.negative:
                sideMultiplier = -1;
                break;
        }
                

    }

    private void Update()
    {

    }

    public override Vector3 constrain(Vector3 L, Vector3 target)
    {

        switch (projectAxis) {
            case ProjectionAxis.Z:
                return ConstrainParentXY(L, target);
            case ProjectionAxis.Y:
                return ConstrainParentXZ(L, target);
            case ProjectionAxis.X:
                return ConstrainParentYZ(L, target);
            default:
                return ConstrainXY(L, target);
        }

    }

    
    private Vector3 ConstrainParentXY(Vector3 L, Vector3 target)
    {
        Quaternion LtoW = Quaternion.FromToRotation(parentJoint.transform.forward, Vector3.forward);
        target = LtoW * (target - L);
        //Debug.DrawLine(transform.position, target+L, Color.yellow);
        Vector3 OProjZ = Vector3.ProjectOnPlane(target, Vector3.forward);

        Vector2 t = new Vector2(OProjZ.x, OProjZ.y);
        Vector2 line = new Vector3();

        if (t.x >= 0)
        {
            if (t.y >= 0)
            {
                //Quadrant 1
                line = new Vector2(quadConstraint13.x, quadConstraint13.y);

            }
            else
            {
                // Quadrant 4
                line = new Vector2(quadConstraint43.x, quadConstraint43.y);


            }
        }
        else
        {
            if (t.y > 0)
            {
                //Quadrant 2
                line = new Vector2(quadConstraint23.x, quadConstraint23.y);

            }
            else
            {
                // Quadrant 3
                line = new Vector2(quadConstraint33.x, quadConstraint33.y);


            }
        }
        Vector3 newDir = IntersectionPointTwoLines(
            new Vector3(line.x, 0, 0),
            new Vector3(0, line.y, 0),
            Vector3.zero,
            OProjZ,
            ProjectionAxis.Z);


        //int sideMult = (side == Sidedness.both) ? (int)Mathf.Sign(target.z - L.z) : sideMultiplier;
        float newZ = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(segmentLen, 2) - Mathf.Pow(newDir.x, 2) - Mathf.Pow(newDir.y, 2))) ;
        newDir = new Vector3(newDir.x, newDir.y, newZ);
        newDir = (Quaternion.Inverse(LtoW) * newDir).normalized * segmentLen + L;

        return newDir;
    }

    private Vector3 ConstrainXY(Vector3 L, Vector3 target)
    {
        Vector3 O = Vector3.Project(target, L);
        float dist = (O - L).magnitude;
        Vector3 OProjZ = Vector3.ProjectOnPlane(target, Vector3.forward);
        Vector3 LProjZ = Vector3.ProjectOnPlane(L, Vector3.forward);

        Vector3 origin = Vector3.zero;
        //both O and L are projected onto the XZ plane, O is shifted by L so that it is at the origin
        //shift OProj to the origin
        OProjZ -= LProjZ;
        //create 2d line vector in (slope, intercept) form using Oproj's x and z as x and y values
        Vector2 t = new Vector2(OProjZ.x, OProjZ.y);
        Vector2 line = new Vector3();

        if (t.x >= 0)
        {
            if (t.y >= 0)
            {
                //Quadrant 1
                line = new Vector2(quadConstraint13.x, quadConstraint13.y);

            }
            else
            {
                // Quadrant 4
                line = new Vector2(quadConstraint43.x, quadConstraint43.y);


            }
        }
        else
        {
            if (t.y > 0)
            {
                //Quadrant 2
                line = new Vector2(quadConstraint23.x, quadConstraint23.y);

            }
            else
            {
                // Quadrant 3
                line = new Vector2(quadConstraint33.x, quadConstraint33.y);


            }
        }
        //find intercept between boundary line and target line

        Vector3 newDir = IntersectionPointTwoLines(
            new Vector3(line.x, 0, 0),
            new Vector3(0, line.y, 0),
            Vector3.zero,
            OProjZ,
            ProjectionAxis.Z);

        int sideMult = (side == Sidedness.both) ? (int)Mathf.Sign(target.z - L.z) : sideMultiplier;
        float newZ = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(segmentLen, 2) - Mathf.Pow(newDir.x, 2) - Mathf.Pow(newDir.y, 2))) * sideMult;
        newDir = new Vector3(newDir.x, newDir.y, newZ);


        //draw lines for visual
        /*Debug.DrawLine(new Vector3(line.x, 0, 0), new Vector3(0, line.y, 0));
        Debug.DrawLine(Vector3.zero, OProjZ);
        Debug.DrawLine(Vector3.zero, newDir, Color.red);*/



        return newDir+L;

    }

    private Vector3 ConstrainParentXZ(Vector3 L, Vector3 target)
    {
        Quaternion LtoW = Quaternion.FromToRotation(parentJoint.transform.up, Vector3.up);
        target = LtoW * (target - L);

        Vector3 OProjY = Vector3.ProjectOnPlane(target, Vector3.up);


        Vector2 t = new Vector2(OProjY.x, OProjY.z);
        Vector2 line = new Vector3();


        if (t.x >= 0)
        {
            if (t.y >= 0)
            {
                //Quadrant 1
                line = new Vector2(quadConstraint13.x, quadConstraint13.z);

            }
            else
            {
                // Quadrant 4
                line = new Vector2(quadConstraint43.x, quadConstraint43.z);


            }
        }
        else
        {
            if (t.y > 0)
            {
                //Quadrant 2
                line = new Vector2(quadConstraint23.x, quadConstraint23.z);

            }
            else
            {
                // Quadrant 3
                line = new Vector2(quadConstraint33.x, quadConstraint33.z);


            }
        }

        Vector3 newDir = IntersectionPointTwoLines(
            new Vector3(line.x, 0, 0),
            new Vector3(0, line.y, 0),
            Vector3.zero,
            OProjY,
            projectAxis);
        int sideMult = (side == Sidedness.both) ? (int)Mathf.Sign(target.y - L.y) : sideMultiplier;
        float newY = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(segmentLen, 2) - Mathf.Pow(newDir.x, 2) - Mathf.Pow(newDir.y, 2))) * sideMult;

        newDir = new Vector3(newDir.x, newY, newDir.y);
        newDir = (Quaternion.Inverse(LtoW) * newDir).normalized * segmentLen + L;

        return newDir;
    }
    
    
    private Vector3 ConstrainParentYZ(Vector3 L, Vector3 target)
    {

        Quaternion LtoW = Quaternion.FromToRotation(parentJoint.transform.right, Vector3.right);
        target = LtoW * (target - L);

        Vector3 OProjX = Vector3.ProjectOnPlane(target, Vector3.right);


        Vector2 t = new Vector2(OProjX.z, OProjX.y);
        Vector2 line = new Vector3();


        if (t.x >= 0)
        {
            if (t.y >= 0)
            {
                //Quadrant 1
                line = new Vector2(quadConstraint13.z, quadConstraint13.y);

            }
            else
            {
                // Quadrant 4
                line = new Vector2(quadConstraint43.z, quadConstraint43.y);



            }
        }
        else
        {
            if (t.y > 0)
            {
                //Quadrant 2
                line = new Vector2(quadConstraint23.z, quadConstraint23.y);

            }
            else
            {
                // Quadrant 3
                line = new Vector2(quadConstraint33.z, quadConstraint33.y);


            }
        }

        Vector3 newDir = IntersectionPointTwoLines(
            new Vector3(line.x, 0, 0),
            new Vector3(0, line.y, 0),
            Vector3.zero,
            OProjX,
            projectAxis);
        int sideMult = (side == Sidedness.both) ? (int)Mathf.Sign(target.x - L.x) : sideMultiplier;
        float newX = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(segmentLen, 2) - Mathf.Pow(newDir.x, 2) - Mathf.Pow(newDir.y, 2))) * sideMult;

        newDir = new Vector3(newX, newDir.y, newDir.x);

        Debug.DrawLine(new Vector3(0, line.y, 0), new Vector3(0, 0, line.x));
        Debug.DrawLine(Vector3.zero, OProjX, Color.blue);
        Debug.DrawLine(Vector3.zero, newDir, Color.red);

        //convert t back to v3 using new Y, add LProj to shift back to the joint position

        return (Quaternion.Inverse(LtoW) * (newDir)).normalized * segmentLen + L;

    }

}
