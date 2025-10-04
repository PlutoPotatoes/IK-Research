using UnityEngine;

public class Fabrik : MonoBehaviour
{
    [SerializeField] GameObject jointInstance;
    [SerializeField] GameObject endEffectorInstance;

    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint13;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint23;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint33;
    [Tooltip("Line equation with [x intercept, y intercept, z intercept] format")]
    [SerializeField] Vector3 quadConstraint43;
    public int segmentCount;
    public float segmentLen;
    public float tolorance;
    public GameObject rootObject;
    public GameObject targetObject;
    public int maxIterations;
    public float maxMoveDist;


    private GameObject[] segments;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        segments = new GameObject[segmentCount];
        float pos = 0;
        for (int i = 0; i < segmentCount - 1; i++) 
            {
                GameObject seg = Instantiate(jointInstance, this.transform);
                seg.transform.position = new Vector3(0, pos, 0);
                pos += segmentLen;
                segments[i] = seg;
            }
            GameObject endEffector = Instantiate(endEffectorInstance, this.transform);
            endEffector.transform.position = new Vector3(0, pos, 0);
            segments[segmentCount - 1] = endEffector;
    }

    // Update is called once per frame
    void Update()
    {
        fabrik();
    }

    void rot()
    {
        Vector3 diff = segments[1].transform.position - segments[0].transform.position;

        float thetaXY = Mathf.Atan2(diff.x, diff.y);
        float thetaXZ = Mathf.Atan2(diff.x, diff.z);



    }

    void fabrik()
    {
        Vector3 root = rootObject.transform.position;
        Vector3 target = targetObject.transform.position;
        if(Vector3.Distance(root, target) >= segmentLen * segmentCount)
        {
            target = (target - root).normalized * (segmentLen * segmentCount);
        }
        int iterations = 1;
        backwardReach(target);
        forwardReach(root);
        while ((segments[segmentCount - 1].transform.position - target).magnitude > tolorance && iterations < maxIterations)
        {
            backwardReach(target);
            forwardReach(root);
            iterations++;
        }

        print(iterations);
    }

    void backwardReach(Vector3 target)
    {
        //move end effector to the target
        segments[segmentCount - 1].transform.rotation = Quaternion.LookRotation(target);
        segments[segmentCount - 1].transform.position = target;

        //FIXME rotate end effector and attached bone
        for(int i = segmentCount-2; i>=0; i--)
        {
            //get the joint position of the one that just moved
            Vector3 curr = segments[i + 1].transform.position;
            //get joint to move position
            Vector3 next = segments[i].transform.position;

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized * segmentLen;
            //shift the position of the joint ahead back by move dir to get the next segment's position
            moveDir = curr - moveDir;
            segments[i].transform.position = moveDir;

        }
    }
    void forwardReach(Vector3 root)
    {
        segments[0].transform.position = root;
        //FIXME rotate end effector and attached bone
        for (int i = 0; i < segmentCount - 1; i++)
        {
            //get the joint position of the one that just moved
            Vector3 curr = segments[i].transform.position;
            //get joint to move position (the joint 1 ahead in the chain)
            Vector3 next = segments[i + 1].transform.position;

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized * segmentLen;
            //shift the position of the joint ahead back by move dir to get the next segment's position
            moveDir = curr - moveDir;

            moveDir = ConstrainRotation(curr, moveDir).normalized*segmentLen + curr;


            segments[i + 1].transform.position = moveDir;
            next = segments[i + 1].transform.position;

            //rotate curr to face the repositioned next
            Vector3 faceDir = next - curr;
            segments[i].transform.rotation = Quaternion.LookRotation(faceDir.normalized);

            segments[i].transform.rotation = Quaternion.LookRotation(faceDir.normalized);

        }
    }

    private Vector3 ConstrainRotation(Vector3 L, Vector3 target)
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
            OProjZ);

        float newZ = Mathf.Sqrt(Mathf.Pow(segmentLen, 2) - Mathf.Pow(newDir.x, 2) - Mathf.Pow(newDir.y, 2)) * Mathf.Sign(target.z-L.z);
        newDir = new Vector3(newDir.x, newDir.y, newZ);


        //draw lines for visual
        Debug.DrawLine(new Vector3(line.x, 0, 0), new Vector3(0, line.y, 0));
        Debug.DrawLine(Vector3.zero, OProjZ);
        Debug.DrawLine(Vector3.zero, newDir, Color.red);



        return newDir;


        //FIXME do I want to constrain on the z?
        //can i do all 3 or is that just fake?

        Vector3 OProjY = Vector3.ProjectOnPlane(newDir, Vector3.up);
        Vector2 t2 = new Vector2(newDir.x, OProjY.z);
        Vector2 line2 = new Vector3();


        if (t2.x >= 0)
        {
            if (t2.y >= 0)
            {
                //Quadrant 1
                line2 = new Vector2(quadConstraint13.x, quadConstraint13.z);

            }
            else
            {
                // Quadrant 4
                line2 = new Vector2(quadConstraint43.x, quadConstraint43.z);


            }
        }
        else
        {
            if (t2.y > 0)
            {
                //Quadrant 2
                line2 = new Vector2(quadConstraint23.x, quadConstraint23.z);

            }
            else
            {
                // Quadrant 3
                line2 = new Vector2(quadConstraint33.x, quadConstraint33.z);


            }
        }

        Vector3 dir2 = IntersectionPointTwoLines(
            new Vector3(line2.x, 0, 0),
            new Vector3(0, line2.y, 0),
            Vector3.zero,
            OProjY,
            false);
        newDir = new Vector3(newDir.x, newDir.y, dir2.y);

        Debug.DrawLine(new Vector3(line2.x, 0, 0), new Vector3(0, 0, line2.y));
        Debug.DrawLine(Vector3.zero, OProjY, Color.blue);
        Debug.DrawLine(Vector3.zero, newDir, Color.red);

        //convert t back to v3 using new Y, add LProj to shift back to the joint position

        return newDir + L;

    }



    public Vector3 IntersectionPointTwoLines(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End, bool xy = true)
    {
        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;
        if (xy)
        {
            ( x1,  y1) = GetXYPosition(line1Start);
            ( x2,  y2) = GetXYPosition(line1End);
            ( x3,  y3) = GetXYPosition(line2Start);
            ( x4,  y4) = GetXYPosition(line2End);
        }
        else
        {
            ( x1,  y1) = GetXYPosition(line1Start);
            ( x2,  y2) = GetXYPosition(line1End);
            ( x3,  y3) = GetXYPosition(line2Start);
            ( x4,  y4) = GetXZPosition(line2End);
        }

        float topX = (x1 * y2 - x2 * y1) * (x3 - x4) - (x3 * y4 - x4 * y3) * (x1 - x2);
        float topY = (x1 * y2 - x2 * y1) * (y3 - y4) - (x3 * y4 - x4 * y3) * (y1 - y2);
        float bottom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        float pX = topX / bottom;
        float pY = topY / bottom;

        Vector3 pVector = new Vector3(pX, pY, 0f);
        bool isInBoundsLine1 = IsIntersectionInBounds(line1Start, line1End, pVector);
        bool isInBoundsLine2 = IsIntersectionInBounds(line2Start, line2End, pVector);

        if (!isInBoundsLine1 || !isInBoundsLine2)
        {
            return line2End;
        }

        return pVector;
    }

    public bool IsIntersectionInBounds(Vector3 lineStart, Vector3 lineEnd, Vector3 intersection)
    {
        float distAC = Vector3.Distance(lineStart, intersection);
        float distBC = Vector3.Distance(lineEnd, intersection);
        float distAB = Vector3.Distance(lineStart, lineEnd);
        // Mathf.Abs(distAC + distBC - distAB) > 0.001f
        if (distAB < distAC)
        {
            return false;
        }

        return true;
    }

    public (float, float) GetXYPosition(Vector3 vector)
    {
        return (vector.x, vector.y);
    }
    public (float, float) GetXZPosition(Vector3 vector)
    {
        return (vector.x, vector.z);
    }


}
