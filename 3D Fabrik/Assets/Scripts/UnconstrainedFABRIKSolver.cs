using UnityEngine;
using System.Collections;

public class UnconstrainedFABRIKSolver : MonoBehaviour
{
    [SerializeField] GameObject[] ArmLeft;
    [SerializeField] GameObject[] ArmRight;
    [SerializeField] GameObject[] LegLeft;
    [SerializeField] GameObject[] LegRight;
    [SerializeField] GameObject[] Spine;
    [SerializeField] GameObject[] Neck;


    [SerializeField] GameObject[] Roots;
    [SerializeField] GameObject[] targets;
    [SerializeField] float tolorance;
    [SerializeField] int maxIterations;

    private ArrayList limbs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        solveBody(limbs);
    }

    void solveBody(ArrayList limbs)
    {
        FABRIK(ArmLeft, Roots[1].transform.position, targets[0].transform.position);
        FABRIK(ArmRight, Roots[1].transform.position, targets[1].transform.position);
        FABRIK(LegLeft, Roots[0].transform.position, targets[2].transform.position);
        FABRIK(LegRight, Roots[0].transform.position, targets[3].transform.position);
        FABRIK(Spine, Roots[0].transform.position, targets[4].transform.position);
        FABRIK(Neck, Roots[1].transform.position, targets[5].transform.position);




    }

    private void FABRIK(GameObject[] limb, Vector3 root, Vector3 target)
    {
        int i = 0;
        backwardSolve(limb, target);
        forwardSolve(limb, root);
        while (Vector3.Distance(limb[limb.Length - 1].transform.position, target) > tolorance)
        {
            backwardSolve(limb, target);
            forwardSolve(limb, root);
            if (i < maxIterations)
            {
                backwardSolve(limb, target);
                forwardSolve(limb, root);

            }
            else
            {
                foreach(GameObject joint in limb)
                {
                    joint.transform.position = Vector3.zero;   
                }
                
                backwardSolve(limb, target);
                forwardSolve(limb, root);
                break;
            }
            i++;
        }
    }

    GameObject[] backwardSolve(GameObject[] limb, Vector3 target)
    {
        int n = limb.Length;
        //move end effector to the target
        if (target != Vector3.zero)
        {
            limb[n - 1].transform.rotation = Quaternion.LookRotation(target);
        }

        limb[n - 1].transform.position = target;

        //FIXME rotate end effector and attached bone
        for (int i = n - 2; i >= 0; i--)
        {
            //get the joint position of the one that just moved
            Vector3 curr = limb[i + 1].transform.position;
            //get joint to move position
            Vector3 next = limb[i].transform.position;
            FABRIKJoint joint = limb[i].GetComponent<FABRIKJoint>();

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized * joint.segmentLen;
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
        for (int i = 0; i < n - 1; i++)
        {
            //get the joint position of the one that just moved
            Vector3 curr = limb[i].transform.position;
            //get joint to move position (the joint 1 ahead in the chain)
            Vector3 next = limb[i + 1].transform.position;
            var joint = limb[i].GetComponent<FABRIKJoint>();

            Vector3 moveDir = (curr - next).normalized * joint.segmentLen;
            moveDir = curr - moveDir;
            //get joint constraints
            //FIXME: RANDOMLY RETURNS NAN at 0,0
            limb[i + 1].transform.position = moveDir;
            //rotate curr to face the repositioned next
            Vector3 faceDir = limb[i + 1].transform.position - limb[i].transform.position;
            //Quaternion rot = joint.constrainTwist(Quaternion.LookRotation(faceDir));
            limb[i].transform.rotation = Quaternion.LookRotation(faceDir);


        }
        return limb;
    }
}
