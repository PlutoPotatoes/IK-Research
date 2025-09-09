using UnityEngine;
using System.Collections.Generic;

public class MultiArmFabrik : MonoBehaviour
{
    [SerializeField] GameObject jointInstance;
    [SerializeField] GameObject endEffectorInstance;

    public int segmentCount;
    public float segmentLen;
    public float tolorance;
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
            Vector3 initialMoveDir = (curr - next).normalized * segmentLen;
            Quaternion moveDir = Quaternion.LookRotation(initialMoveDir);
            if (limb[i].CompareTag("hinge"))
            {
                hingeConstraint(moveDir, Vector3.forward, out moveDir);
            }
            

            Vector3 vectorMoveDir = (moveDir * Vector3.forward).normalized;
            limb[i+1].transform.position = curr - (vectorMoveDir*segmentLen);
            //rotate curr to face the repositioned next
            Vector3 faceDir = limb[i+1].transform.position - limb[i].transform.position;
            limb[i].transform.rotation = Quaternion.LookRotation(faceDir);

        }
        return limb;
    }

    //FIXME: make this flatten on the joint's local forward vector not just the Z Axis
    void hingeConstraint(Quaternion rotation, Vector3 twistAxis, out Quaternion ConstrainedRot)
    {
        ConstrainedRot = rotation;


        // Rotate the twist axis by the quaternion to get the actual twisted direction
        Vector3 rotatedTwist = rotation * twistAxis;

        // Project the rotated vector onto a plane orthogonal to Z (i.e., remove Z component)
        Vector3 flattened = Vector3.ProjectOnPlane(rotatedTwist, twistAxis).normalized;

        // if the rotation is already on the Z axis return identity
        if (flattened == Vector3.zero)
        {
            return;
        }

        // Create a new quaternion that aligns the twist axis with the projected vector
        ConstrainedRot = Quaternion.LookRotation(flattened, twistAxis);

    }


}
