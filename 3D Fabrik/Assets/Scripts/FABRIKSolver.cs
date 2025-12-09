using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;

public class FABRIKSolver : MonoBehaviour
{
    [SerializeField] GameObject[] ArmLeft;
    [SerializeField] GameObject[] ArmRight;
    [SerializeField] GameObject[] LegLeft;
    [SerializeField] GameObject[] LegRight;
    [SerializeField] GameObject[] Spine;
    [SerializeField] GameObject[] Neck;


    [SerializeField] GameObject[] Roots;
    [SerializeField] GameObject[] targets;
    [SerializeField] GameObject[] FABRIKTrackPoints;
    [SerializeField] GameObject[] ArmatureTrackPoints;
    [SerializeField] float tolorance;
    [SerializeField] int maxIterations;
    [SerializeField] Vector3 legOffset;
    [SerializeField] string datafile;


    private string datapath;
    private int frame = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        datapath = "C:/Game Making/IK-Research/test_data/" + datafile + ".csv";
        createCSV(datapath);

    }

    // Update is called once per frame
    void Update()
    {
        solveBody();
        gatherData(datapath);
    }

    void solveBody()
    {
        FABRIK(ArmLeft, Roots[1].transform.position , targets[0].transform.position);
        FABRIK(ArmRight, Roots[1].transform.position , targets[1].transform.position);
        FABRIK(LegLeft, Roots[0].transform.position-legOffset, targets[2].transform.position);
        FABRIK(LegRight, Roots[0].transform.position + legOffset, targets[3].transform.position);
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
                    //joint.transform.position = Vector3.zero;
                }
                //limb[0].transform.rotation = Quaternion.LookRotation(Vector3.forward + limb[0].transform.position, Vector3.up);
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
            moveDir = joint.constrain(curr, moveDir);
            //FIXME: RANDOMLY RETURNS NAN at 0,0
            limb[i + 1].transform.position = moveDir;
            //rotate curr to face the repositioned next
            Vector3 faceDir = limb[i + 1].transform.position - limb[i].transform.position;
            //Quaternion rot = joint.constrainTwist(Quaternion.LookRotation(faceDir));
            if (faceDir != Vector3.zero)
            {
                if (!joint.isSubBase)
                {

                    limb[i].transform.rotation = Quaternion.LookRotation(faceDir, joint.rotateAxis);
                }
                else
                {
                    limb[i].transform.rotation = Quaternion.LookRotation(this.transform.forward);
                }
            }

        }
        return limb;
    }

    private void createCSV(string filename)
    {
        string header = "Frame,LeftArmDistance,RightArmDistance,LeftLegDistance,RightLegDistance,ShouldersDistance,HeadDistance,LeftElbowDistance,RightElbowDistance,LeftKneeDistance,RightKneeDistance";
        File.WriteAllText(filename, header);
    }

    private void gatherData(string filename)
    {
        //vector3 distance(joint, target) - end effector joint length
        //track arms targets 0 and 1
        float LeftArmDistance = Vector3.Distance(ArmLeft[2].transform.position, targets[0].transform.position) - 1;
        float RightArmDistance = Vector3.Distance(ArmRight[2].transform.position, targets[1].transform.position) - 1;
        //track legs targets 2 and 3
        float LeftLegDistance = Vector3.Distance(LegLeft[2].transform.position, targets[2].transform.position) - 1;
        float RightLegDistance = Vector3.Distance(LegRight[2].transform.position, targets[3].transform.position) - 1;
        //track shoulders target 4
        float ShouldersDistance = Vector3.Distance(Spine[3].transform.position, targets[4].transform.position) - 1;
        //track head target 5
        float HeadDistance = Vector3.Distance(Neck[2].transform.position, targets[4].transform.position) - 1;
        float LeftElbowDistance = Vector3.Distance(ArmLeft[1].transform.position, ArmatureTrackPoints[2].transform.position);
        float RightElbowDistance = Vector3.Distance(ArmRight[1].transform.position, ArmatureTrackPoints[3].transform.position);

        float LeftKneeDistance = Vector3.Distance(LegLeft[1].transform.position, ArmatureTrackPoints[0].transform.position);
        float RightKneeDistance = Vector3.Distance(LegRight[1].transform.position, ArmatureTrackPoints[1].transform.position);

        //format and store in csv?

        string data = "\n" + frame + "," + LeftArmDistance + "," + RightArmDistance + "," + LeftLegDistance + "," + RightLegDistance + "," + ShouldersDistance + "," + HeadDistance + "," + LeftElbowDistance + "," + RightElbowDistance + "," + LeftKneeDistance + "," + RightKneeDistance;
        File.AppendAllText(filename, data);
        frame++;
    }

}
