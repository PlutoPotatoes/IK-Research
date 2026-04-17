using UnityEngine;

public class ConstrainedLegJoint : FABRIKJoint
{


    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint13;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint23;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint33;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint43;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 positiveQuadConstraint1;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 positiveQuadConstraint2;
    [SerializeField] ProjectionAxis projectAxis = ProjectionAxis.Y;
    

    public override Vector3 constrain(Vector3 L, Vector3 target)
    {
         return ConstrainUpperLeg(L, target);

    }
    //TODO create is inside constraints check and create/fine tune the positive boundaries
    private Vector3 ConstrainUpperLeg(Vector3 L, Vector3 target)
    {
        Debug.DrawLine(transform.position, target, Color.black);


        Quaternion LtoW = Quaternion.FromToRotation(parentJoint.transform.up, Vector3.up);
        target = LtoW * (target - L);

        bool negativeY = true;
        if (target.y >= 0)
        {
            negativeY = false;
        }



        Vector3 OProjY = Vector3.ProjectOnPlane(target, Vector3.up);

        Vector2 t = new Vector2(OProjY.x, OProjY.z);
        Vector2 line = new Vector3();


        if (t.x >= 0)
        {
            if (t.y >= 0)
            {
                //Quadrant 1
                //allow positive y
                if (negativeY)
                {
                    line = new Vector2(quadConstraint13.x, quadConstraint13.z);
                }
                else
                {
                    line = new Vector2(positiveQuadConstraint1.x, positiveQuadConstraint1.z);
                }

            }
            else
            {
                // Quadrant 4
                //doesn't allow positive so keep negative
                line = new Vector2(quadConstraint43.x, quadConstraint43.z);
                negativeY = true;
            }
        }
        else
        {
            if (t.y > 0)
            {
                //Quadrant 2
                //allow positive y
                if (negativeY)
                {
                    line = new Vector2(quadConstraint23.x, quadConstraint23.z);
                }
                else
                {
                    line = new Vector2(positiveQuadConstraint2.x, positiveQuadConstraint2.z);
                }

            }
            else
            {
                // Quadrant 3
                //doesn't allow positive so keep negative
                line = new Vector2(quadConstraint33.x, quadConstraint33.z);
                negativeY = true;


            }
        }
        Vector3 newDir;
        if (negativeY)
        {
             newDir= IntersectionPointTwoLines(
                new Vector3(line.x, 0, 0),
                new Vector3(0, line.y, 0),
                Vector3.zero,
                OProjY,
                projectAxis);
        }
        else
        {
            newDir = ConstraintCheckUpper(
                new Vector3(line.x, 0, 0),
                new Vector3(0, line.y, 0),
                Vector3.zero,
                OProjY,
                projectAxis);
        }
        float newY = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(segmentLen, 2) - Mathf.Pow(newDir.x, 2) - Mathf.Pow(newDir.y, 2)));
        newY *= negativeY? -1 : 1;

        newDir = new Vector3(newDir.x, newY, newDir.y);
        newDir = (Quaternion.Inverse(LtoW) * newDir).normalized * segmentLen + L;

        return newDir;
    }


    public Vector3 ConstraintCheckUpper(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End, ProjectionAxis axis)
    {
        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;

        (x1, y1) = GetXYPosition(line1Start);
        (x2, y2) = GetXYPosition(line1End);
        (x3, y3) = GetXYPosition(line2Start);
        (x4, y4) = GetXZPosition(line2End);

        


        float topX = (x1 * y2 - x2 * y1) * (x3 - x4) - (x3 * y4 - x4 * y3) * (x1 - x2);
        float topY = (x1 * y2 - x2 * y1) * (y3 - y4) - (x3 * y4 - x4 * y3) * (y1 - y2);
        float bottom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        float pX = topX / bottom;
        float pY = topY / bottom;

        //pVector=intersection
        Vector3 pVector = new Vector3(pX, pY, 0f);
        //This is confusing, true actually means out of bounds here
        bool isInBoundsLine2 = IsIntersectionInBounds(line2Start, line2End, pVector);

        if (isInBoundsLine2)
        {
            return new Vector3(x4, y4, 0);
        }
        print("being corrected");
        return pVector;
    }

}
