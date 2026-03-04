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
    [SerializeField] bool twistConstrainedJoint;
    [SerializeField] float PositiveRotationConstraint;
    [SerializeField] float NegativeRotationConstraint;
    [SerializeField] public bool isSubBase;
    [SerializeField] public JointType jointType;
    [SerializeField] public GameObject parentJoint;
    public Vector3 rotateAxis = Vector3.up;


    public Quaternion constrainRotation(Vector3 faceDir, Transform targetNormal, Vector3 target)
    {
        //transform of the GameObject holding the whole model
        Transform modelTransform = GetComponentInParent<Transform>();

        //default rotation and position
        Vector3 up = this.transform.up;
        Quaternion rotation = Quaternion.LookRotation(faceDir, up);

        //rotational offset from world.forward to joint.forward
        Quaternion forwardShift = Quaternion.FromToRotation(Vector3.forward, faceDir);

        if (isSubBase)
        {
            return Quaternion.LookRotation(modelTransform.forward, Vector3.up);
        }


        switch (jointType)
        {
            case JointType.UpperLeg:
                //Shift world.up by rotational offset from world.forward -> joint.foward and use it as the up vector
                up = forwardShift * Vector3.up;
                rotation = Quaternion.LookRotation(faceDir, up);
                break;
            case JointType.UpperArm:
                //Shift world.up by rotational offset from world.forward -> joint.foward and use it as the up vector
                forwardShift = Quaternion.FromToRotation(Vector3.forward, faceDir);
                up = forwardShift * Vector3.up;
                rotation = Quaternion.LookRotation(faceDir, up);
                break;
            case JointType.Knee:
                //need to find a way to keep the knee facing forward

                Vector3 h2t = parentJoint.transform.position+target;
                Vector3 knee_proj = Vector3.Project(transform.position, h2t);
                rotation = Quaternion.LookRotation(faceDir, knee_proj);

                Debug.DrawLine(transform.position, knee_proj, Color.blue);
                Debug.DrawLine(parentJoint.transform.position, target, Color.red);

                break;
            case JointType.Elbow:
                break;
        }

        return rotation;

    }

    public Quaternion constrainTwist(Quaternion rot)
    {
        if (twistConstrainedJoint)
        {
            //get swing and twist
            Vector3 twistAxis = this.transform.forward;
            Quaternion swing;
            Quaternion twist;
            rot.decompose(twistAxis, out swing, out twist);
            //get angle
            float angle;
            Vector3 axis;
            twist.ToAngleAxis(out angle, out axis);
            //constrain angle
            if (angle > PositiveRotationConstraint && angle < NegativeRotationConstraint)
            {
                if (PositiveRotationConstraint - angle <= NegativeRotationConstraint - angle)
                {
                    angle = PositiveRotationConstraint;
                }
                else
                {
                    angle = NegativeRotationConstraint;
                }
            }
            //constrain twist
            twist = Quaternion.AngleAxis(angle, axis);
            return swing * twist;
        }
        return rot;
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
    public (float, float) GetZYPosition(Vector3 vector)
    {
        return (vector.z, vector.y);
    }

}
