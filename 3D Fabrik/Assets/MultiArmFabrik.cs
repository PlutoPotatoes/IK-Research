using UnityEngine;
using System.Collections.Generic;

public class MultiArmFabrik : MonoBehaviour
{
    [SerializeField] GameObject jointInstance;
    [SerializeField] GameObject endEffectorInstance;

    public int segmentCount;
    public float segmentLen;
    public float tolorance;
    public Vector3 EulerMax;
    public Vector3 EulerMin;
    public GameObject rootObject;
    public GameObject[] roots;
    public GameObject targetObject;
    public GameObject[] targets;

    public GameObject[] finger1;
    public GameObject[] finger2;
    public GameObject[] finger3;
    public GameObject[] hand;


    private GameObject[] segments;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
        Vector3 root = roots[0].transform.position;
        Vector3 target = targets[0].transform.position;
      
        if(Vector3.Distance(root, target) >= segmentLen * finger1.Length)
        {
            target = (target - root) * (segmentLen * finger1.Length-1);
        }
        finger1 = backwardSolve(finger1, target);
        finger1 = forwardSolve(finger1, root);

        root = roots[0].transform.position;
        target = targets[1].transform.position;

        if (Vector3.Distance(root, target) >= segmentLen * finger2.Length)
        {
            target = (target - root) * (segmentLen * finger2.Length-1);
        }
        finger2 = backwardSolve(finger2, target);
        finger2 = forwardSolve(finger2, root);

        root = roots[0].transform.position;
        target = targets[2].transform.position;

        if (Vector3.Distance(root, target) >= segmentLen * finger3.Length)
        {
            target = (target - root) * (segmentLen * finger3.Length-1);
        }
        finger3 = backwardSolve(finger3, target);
        finger3 = forwardSolve(finger3, root);

        root = roots[1].transform.position;
        target = targets[3].transform.position;

        if (Vector3.Distance(root, target) >= segmentLen * hand.Length)
        {
            target = (target - root) * (segmentLen * hand.Length);
        }
        hand = backwardSolve(hand, target);
        hand = forwardSolve(hand, root);




    }
    //Start with end effector move towards root
    GameObject[] backwardSolve(GameObject[] limb, Vector3 target)
    {
        int n = limb.Length;
        //move end effector to the target
        if(target != Vector3.zero) {
            limb[n - 1].transform.rotation = Quaternion.LookRotation(target);
        }

        limb[n - 1].transform.position = target;

        //FIXME rotate end effector and attached bone
        for(int i = n-2; i>=0; i--)
        {
            //get the joint position of the one that just moved
            Vector3 curr = limb[i + 1].transform.position;
            //get joint to move position
            Vector3 next = limb[i].transform.position;

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized * segmentLen;
            //shift the position of the joint ahead back by move dir to get the next segment's position
            moveDir = curr - moveDir;
            limb[i].transform.position = moveDir;

        }
        return limb;
    }

    //start with root move towards end effector
    //FIXME; constrain each joint on forward solve
    GameObject[] forwardSolve(GameObject[] limb, Vector3 root)
    {
        int n = limb.Length;
        limb[0].transform.position = root;
        //FIXME rotate end effector and attached bone
        for (int i = 0; i < n-1; i++)
        {
            //get the joint position of the one that just moved
            Vector3 curr = limb[i].transform.position;
            //get joint to move position (the joint 1 ahead in the chain)
            Vector3 next = limb[i+1].transform.position;

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized;
            //constrain the vector from curr to next using curr's rotational limits
            Vector3 constrainedDir = constrainJoint(limb[i], moveDir, EulerMax, EulerMin);

            limb[i+1].transform.position = curr - (constrainedDir*segmentLen);

            //rotate curr to face the repositioned next
            Vector3 faceDir = next - curr;
            limb[i].transform.rotation = Quaternion.LookRotation(faceDir.normalized);

        }
        return limb;
    }

    Vector3 constrainJoint(GameObject joint, Vector3 targetMoveDir, Vector3 EulerMax, Vector3 EulerMin)
    {
        //return targetMoveDir;
        // 1. convert targetMoveDir to joint's local space
        Vector3 localDir = Quaternion.Inverse(joint.transform.rotation) * targetMoveDir;

        // 2. find Quaternion rotation from forward to target
        Quaternion localRotationToTarget = Quaternion.FromToRotation(Vector3.forward, localDir);

        // 3. convert to euler angles
        Vector3 rotToTargetEulers = localRotationToTarget.eulerAngles;

        // 4. normalize the eulers (-180 <-> 180)
        rotToTargetEulers = NormalizeEulerAngles(rotToTargetEulers);

        // 5. Clamp each Euler
        Vector3 clampedEulers = new Vector3(
            Mathf.Clamp(rotToTargetEulers.x, EulerMin.x, EulerMax.x),
            Mathf.Clamp(rotToTargetEulers.y, EulerMin.y, EulerMax.y),
            Mathf.Clamp(rotToTargetEulers.z, EulerMin.z, EulerMax.z)
            );

        // 6. reconstruct the Quaternion
        Quaternion clampedRotation = Quaternion.Euler(clampedEulers);

        // 7. Convert back to Worldspace
        clampedRotation = clampedRotation * joint.transform.rotation;

        // 8. 
        Vector3 constrainedDirection = clampedRotation * Vector3.forward;


        return constrainedDirection;

    }

    private static Vector3 NormalizeEulerAngles(Vector3 euler)
    {
        return new Vector3(
            NormalizeAngle(euler.x),
            NormalizeAngle(euler.y),
            NormalizeAngle(euler.z)
        );
    }

    private static float NormalizeAngle(float angle)
    {
        angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
        return angle;
    }
}
