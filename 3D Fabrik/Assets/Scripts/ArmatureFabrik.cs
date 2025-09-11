using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmatureFabrik : MonoBehaviour
{
    [SerializeField] GameObject RightFootTarget;
    [SerializeField] GameObject LeftFootTarget;
    [SerializeField] GameObject RightHandTarget;
    [SerializeField] GameObject LeftHandTarget;
    [SerializeField] GameObject HeadTarget;
    [SerializeField] GameObject Root;


    [SerializeField] GameObject[] leftArm;
    [SerializeField] GameObject[] rightArm;
    [SerializeField] GameObject[] leftLeg;
    [SerializeField] GameObject[] rightLeg;
    [Tooltip("Hip, left upper leg, right upper leg")]
    [SerializeField] GameObject[] lowerBodyTriangle;
    [Tooltip("Hip, spine, chest, upper chest")]
    [SerializeField] GameObject[] spine;
    [Tooltip("Hip, left shoulder, right shoulder")]
    [SerializeField] GameObject[] upperBodyTriangle;
    [Tooltip("upper chest -> neck bone simplified for easy pivot joint manipulation")]
    [SerializeField] GameObject[] neck;


    /*
     * TODO:
     * 1. create static processor function that solves the body start to finish
     * 2. add limb types to joints and solve with constraints
     * 3. encorperate centroids for shared subroots
     * 4. create a queue that solves moved limbs first
     */

    /*
     * NEXT STEP:
     * create a separate function that solves the internal triangles as a cyclic cycle with the Hip as the target
     */

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void staticFABRIK()
    {
        //For each chain in sorted IKChains, call forward and then backward
        /*
         * solve order
         * 1. right arm
         * 2. left arm
         * 3. upper body triangle
         * 4. right leg
         * 5. left leg
         * 6. lower body triangle
         * 7. spine 
         * 8. head
         */
        //solve arms
        IterateFABRIK(rightArm, RightHandTarget);
        IterateFABRIK(leftArm, LeftHandTarget);
        //solve legs
        IterateFABRIK(leftLeg, LeftFootTarget);
        IterateFABRIK(rightLeg, RightFootTarget);

        //These two may need a different way to solve since they're cyclical
        //upper body
        IterateFABRIK(upperBodyTriangle, Root);
        //lower body
        IterateFABRIK(lowerBodyTriangle, Root);

        //spine and neck
        IterateFABRIK(spine, Root);
        IterateFABRIK(neck, HeadTarget);



    }
    private void IterateFABRIK(GameObject[] limb, GameObject target)
    {
        Vector3 rootPos = limb[0].transform.position;
        //add iteration in when needed
        chainBackwardSolve(limb, RightHandTarget.transform.position);
        chainForwardSolve(limb, rootPos);
    }
    //first step in FABRIK, solving from end effector to root
    private void chainBackwardSolve(GameObject[] limb, Vector3 target)
    {
        int n = limb.Length;
        //move end effector to the target
        // change to {Vector3.Distance(limb[n-1].transform.position, target) <= tolorance} once this actually works
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

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized * limb[i].GetComponent<FABRIKJoint>().boneLength;
            //shift the position of the joint ahead back by move dir to get the next segment's position
            moveDir = curr - moveDir;
            limb[i].transform.position = moveDir;

        }
    }


    //second step in FABRIK, solving from root to end effector
    private void chainForwardSolve(GameObject[] limb, Vector3 root)
    {
        int n = limb.Length;
        limb[0].transform.position = root;
        //FIXME rotate end effector and attached bone
        for (int i = 0; i < n - 1; i++)
        {
            var joint = limb[i].GetComponent<FABRIKJoint>();
            //get the joint position of the one that just moved
            Vector3 curr = limb[i].transform.position;
            //get joint to move position (the joint 1 ahead in the chain)
            Vector3 next = limb[i + 1].transform.position;
            Vector3 initialMoveDir = (curr - next).normalized * joint.boneLength;
            Quaternion moveDir = Quaternion.LookRotation(initialMoveDir);

            //use the joints inherited constrainRotation function to constrain moveDir
            moveDir = joint.ConstrainRotation(moveDir);


            Vector3 vectorMoveDir = (moveDir * Vector3.forward).normalized;
            limb[i + 1].transform.position = curr - (vectorMoveDir * joint.boneLength);
            //rotate curr to face the repositioned next
            Vector3 faceDir = limb[i + 1].transform.position - limb[i].transform.position;
            limb[i].transform.rotation = Quaternion.LookRotation(faceDir);

        }

    }


    void wackShitIdea()
    {
        //solve everything with a sub-base
        //FIXME each of theses steps still needs to store a backwardValue in the root of each limb
        chainBackwardSolve(rightArm, RightHandTarget.transform.position);
        chainBackwardSolve(leftArm, LeftHandTarget.transform.position);
        chainBackwardSolve(rightLeg, RightFootTarget.transform.position);
        chainBackwardSolve(leftLeg, LeftFootTarget.transform.position);
        chainBackwardSolve(neck, HeadTarget.transform.position);

        //solve everything connected to the hip
        chainBackwardSolve(lowerBodyTriangle, Root.transform.position);
        chainBackwardSolve(upperBodyTriangle, Root.transform.position);
        chainBackwardSolve(spine, Root.transform.position);

        //solve forward for external limbs with centroid position at root
        chainForwardSolve(rightArm, rightArm[0].GetComponent<FABRIKJoint>().getCentroid());
        chainForwardSolve(leftArm, leftArm[0].GetComponent<FABRIKJoint>().getCentroid());
        chainForwardSolve(rightLeg, rightLeg[0].GetComponent<FABRIKJoint>().getCentroid());
        chainForwardSolve(leftLeg, leftLeg[0].GetComponent<FABRIKJoint>().getCentroid());
        chainForwardSolve(neck, neck[0].GetComponent<FABRIKJoint>().getCentroid());

        //solve forwards for internal structures
        chainForwardSolve(lowerBodyTriangle, Root.transform.position);
        chainForwardSolve(upperBodyTriangle, Root.transform.position);
        chainForwardSolve(spine, Root.transform.position);

    }
}
