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
        finger1 = forwardReach(finger1, target);
        finger1 = backwardReach(finger1, root);

        root = roots[0].transform.position;
        target = targets[1].transform.position;

        if (Vector3.Distance(root, target) >= segmentLen * finger2.Length)
        {
            target = (target - root) * (segmentLen * finger2.Length-1);
        }
        finger2 = forwardReach(finger2, target);
        finger2 = backwardReach(finger2, root);

        root = roots[0].transform.position;
        target = targets[2].transform.position;

        if (Vector3.Distance(root, target) >= segmentLen * finger3.Length)
        {
            target = (target - root) * (segmentLen * finger3.Length-1);
        }
        finger3 = forwardReach(finger3, target);
        finger3 = backwardReach(finger3, root);

        root = roots[1].transform.position;
        target = targets[3].transform.position;

        if (Vector3.Distance(root, target) >= segmentLen * hand.Length)
        {
            target = (target - root) * (segmentLen * hand.Length);
        }
        hand = forwardReach(hand, target);
        hand = backwardReach(hand, root);




    }
    GameObject[] forwardReach(GameObject[] limb, Vector3 target)
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


    GameObject[] backwardReach(GameObject[] limb, Vector3 root)
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
            Vector3 moveDir = (curr - next).normalized * segmentLen;
            //shift the position of the joint ahead back by move dir to get the next segment's position
            moveDir = curr - moveDir;
            limb[i+1].transform.position = moveDir;

            //rotate curr to face the repositioned next
            Vector3 faceDir = next - curr;
            limb[i].transform.rotation = Quaternion.LookRotation(faceDir.normalized);

        }
        return limb;
    }
}
