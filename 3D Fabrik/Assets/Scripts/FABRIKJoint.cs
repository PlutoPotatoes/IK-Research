using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FABRIKJoint : MonoBehaviour
{
    public enum ProjectionAxis
    {
        X,
        Y,
        Z
    };
    public enum Sidedness
    {
        positive,
        negative,
        both
    };
    public enum JointType
    {
        UpperLeg,
        Knee,
        UpperArm,
        Elbow,
        Misc
    }
    [SerializeField] public float segmentLen;
    [SerializeField] public bool isSubBase;
    [SerializeField] public JointType jointType;
    [SerializeField] public GameObject parentJoint;
    public Vector3 rotateAxis = Vector3.up;

    private Vector3 limb_normal = Vector3.zero;



    public Quaternion constrainRotation(Vector3 faceDir, Vector3 target)
    {
        //transform of the GameObject holding the whole model
        Transform modelTransform = transform.root;

        //default rotation and position
        Vector3 up = this.transform.up;
        Quaternion rotation = Quaternion.LookRotation(faceDir, up);


        //cross product solution variables
        Vector3 Target_Vector = transform.position - target;
        Vector3 Limb_Normal = Vector3.Cross(faceDir, Target_Vector);

        if(limb_normal != Vector3.zero)
        {
            Limb_Normal = Vector3.Lerp(limb_normal, Limb_Normal, 0.1f);
        }

        if (isSubBase)
        {
            return Quaternion.LookRotation(modelTransform.forward, modelTransform.up);
        }


        switch (jointType)
        {
            case JointType.UpperLeg:
                //fix this to account for non-origin actors
                //If target is y-positive relative to joint then z must be negative, else z is positive
                //accounts for positive side movement in legs
                up = Vector3.Cross(Limb_Normal, faceDir).normalized;
                if (gameObject.tag == "print")
                {
                    print(transform.up);
                }
                if (faceDir.y <= 0){
                    up = new Vector3(up.x, up.y, Mathf.Abs(up.z));
                }
                else
                {
                    up = new Vector3(up.x, up.y, -Mathf.Abs(up.z));

                }


                //up = Vector3.RotateTowards(forwardShift * modelTransform.up, ideal_up, 45 * Mathf.Deg2Rad, 0);

                up = Vector3.Lerp(transform.up, up, 0.2f);
                rotation = Quaternion.LookRotation(faceDir, up);

                break;
            case JointType.UpperArm:
                //Shift world.up by rotational offset from world.forward -> joint.foward and use it as the up vector
                up = Vector3.Cross(Limb_Normal, transform.forward);
                up = Vector3.Lerp(transform.up, up, 0.1f);
                //up = new Vector3(up.x, up.y, Mathf.Abs(up.z));
                rotation = Quaternion.LookRotation(faceDir, up);
                break;
            case JointType.Knee:
                rotation = Quaternion.LookRotation(faceDir, parentJoint.transform.up);

                break;
            case JointType.Elbow:
                rotation = Quaternion.LookRotation(faceDir, parentJoint.transform.up);

                break;
        }

        return rotation;

    }

    public virtual Vector3 constrain(Vector3 L, Vector3 target)
    {
        return target;
    }
    public Vector3 IntersectionPointTwoLines(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End, ProjectionAxis axis)
    {
        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;
        switch (axis)
        {
            case ProjectionAxis.Z:
                (x1, y1) = GetXYPosition(line1Start);
                (x2, y2) = GetXYPosition(line1End);
                (x3, y3) = GetXYPosition(line2Start);
                (x4, y4) = GetXYPosition(line2End);
                break;
            case ProjectionAxis.Y:
                (x1, y1) = GetXYPosition(line1Start);
                (x2, y2) = GetXYPosition(line1End);
                (x3, y3) = GetXYPosition(line2Start);
                (x4, y4) = GetXZPosition(line2End);
                break;
            case ProjectionAxis.X:
                (x1, y1) = GetXYPosition(line1Start);
                (x2, y2) = GetXYPosition(line1End);
                (x3, y3) = GetXYPosition(line2Start);
                (x4, y4) = GetZYPosition(line2End);
                break;
            default:
                (x1, y1) = GetXYPosition(line1Start);
                (x2, y2) = GetXYPosition(line1End);
                (x3, y3) = GetXYPosition(line2Start);
                (x4, y4) = GetXYPosition(line2End);
                break;

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
            return new Vector3(x4, y4, 0);
        }

        return pVector;
    }

    public bool IsIntersectionInBounds(Vector3 lineStart, Vector3 lineEnd, Vector3 intersection)
    {
        float distAC = Vector3.Distance(lineStart, intersection);
        float distAB = Vector3.Distance(lineStart, lineEnd);
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
    public (float, float) GetZYPosition(Vector3 vector)
    {
        return (vector.z, vector.y);
    }

}
