using UnityEngine;

public class ConstrainedHingeJoint : FABRIKJoint
{

    [SerializeField] ProjectionAxis axis = ProjectionAxis.X;
    [SerializeField] float PositiveIntercept;
    [SerializeField] float Negativeintercept;
    [SerializeField] Sidedness side;
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
        switch (axis)
        {
            case ProjectionAxis.X:
                return hingeX(L, target);
            case ProjectionAxis.Y:
                return hingeX(L, target);
            case ProjectionAxis.Z:
                return hingeX(L, target);
            default:
                return hingeX(L, target);
        }

    }

    public Vector3 hingeX(Vector3 L, Vector3 target)
    {
        Vector3 O = Vector3.Project(target, L);
        float dist = (O - L).magnitude;
        Vector3 OProj = Vector3.ProjectOnPlane(target, Vector3.right);

        Vector3 LProj = Vector3.ProjectOnPlane(L, Vector3.right);

        Vector3 origin = Vector3.zero;
        //both O and L are projected onto the XZ plane, O is shifted by L so that it is at the origin
        //shift OProj to the origin
        OProj -= LProj;
        float intercept;

        if (OProj.z> 0)
        {
            intercept = PositiveIntercept;
        }
        else
        {
            intercept = Negativeintercept;
        }

        Vector3 newDir = IntersectionPointTwoLines(
            new Vector3(intercept, segmentLen, 0),
            new Vector3(intercept, -segmentLen, 0),
            Vector3.zero,
            OProj,
            axis);
        int sideMult = (side == Sidedness.both) ? (int)Mathf.Sign(target.y - L.y) : sideMultiplier;

        newDir = new Vector3(0, newDir.x, newDir.y);

        return newDir + L;



    }

}
