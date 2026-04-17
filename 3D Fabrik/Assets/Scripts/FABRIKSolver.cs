using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.IO;

public class FABRIKSolver : MonoBehaviour
{
    private enum solvePattern
    {
        Constant,
        Delay_Frames,
        Reset_on_Delay

    }

    [SerializeField] bool useRotationConstraints;
    [SerializeField] bool usePositionalConstraints;
    [SerializeField] solvePattern solveType;
    [SerializeField] int delay;



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
    [SerializeField] Vector3[] ResetTargets;
    [SerializeField] float tolorance;
    [SerializeField] int maxIterations;
    [SerializeField] Vector3 legOffset;
    [SerializeField] string datafile;



    private Vector3[] ArmLeftResetPosition;
    private Vector3[] ArmRightResetPosition;
    private Vector3[] LegLeftResetPosition;
    private Vector3[] LegRightResetPosition;
    private Vector3[] SpineResetPosition;
    private Vector3[] NeckResetPosition;

    private Quaternion[] ArmLeftResetRotation;
    private Quaternion[] ArmRightResetRotation;
    private Quaternion[] LegLeftResetRotation;
    private Quaternion[] LegRightResetRotation;
    private Quaternion[] SpineResetRotation;
    private Quaternion[] NeckResetRotation;

    private string datapath;
    private int frame = 0;
    private int DelayFrame = 0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        datapath = "C:/Game Making/IK-Research/test_data/" + datafile + ".csv";
        createCSV(datapath);
        ArmLeftResetPosition = new Vector3[ArmLeft.Length];
        ArmRightResetPosition = new Vector3[ArmRight.Length];
        LegLeftResetPosition = new Vector3[LegLeft.Length];
        LegRightResetPosition = new Vector3[LegRight.Length];
        SpineResetPosition = new Vector3[Spine.Length];
        NeckResetPosition = new Vector3[Neck.Length];

        ArmLeftResetRotation = new Quaternion[ArmLeft.Length];
        ArmRightResetRotation = new Quaternion[ArmRight.Length];
        LegLeftResetRotation = new Quaternion[LegLeft.Length];
        LegRightResetRotation = new Quaternion[LegRight.Length];
        SpineResetRotation = new Quaternion[Spine.Length];
        NeckResetRotation = new Quaternion[Neck.Length];
        get_reset_transforms();
        reset_model();

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Roots[0].transform.position;
        solve();
        //gatherData(datapath);
    }

    private void solve()
    {
        switch (solveType)
        {
            case solvePattern.Constant:
                solveBody();
                break;
            case solvePattern.Delay_Frames:
                if(DelayFrame%delay == 0)
                {
                    solveBody();
                    DelayFrame = 0;
                }
                DelayFrame++;
                break;
            case solvePattern.Reset_on_Delay:
                if (DelayFrame % delay == 0)
                {
                    get_reset_transforms();
                    reset_model();
                    DelayFrame = 0;
                }
                solveBody();
                DelayFrame++;
                break;

        }
    }

    void solveBody()
    {
        FABRIK(ArmLeft, Roots[1].transform.position , targets[0].transform.position, ResetTargets[4], ResetTargets[0]);
        FABRIK(ArmRight, Roots[1].transform.position , targets[1].transform.position, ResetTargets[4], ResetTargets[1]);
        FABRIK(LegLeft, Roots[0].transform.position-legOffset, targets[2].transform.position, ResetTargets[6], ResetTargets[2]);
        FABRIK(LegRight, Roots[0].transform.position + legOffset, targets[3].transform.position, ResetTargets[6], ResetTargets[3]);
        FABRIK(Spine, Roots[0].transform.position, targets[4].transform.position, ResetTargets[6], ResetTargets[4]);
        FABRIK(Neck, Roots[1].transform.position, targets[5].transform.position, ResetTargets[4], ResetTargets[6]);

    }

    void get_reset_transforms()
    {
        Reset_FABRIK(Spine, ResetTargets[6], ResetTargets[4]);
        Reset_FABRIK(Neck, ResetTargets[4], ResetTargets[5]);
        Reset_FABRIK(ArmLeft, ResetTargets[4] - legOffset, ResetTargets[0]);
        Reset_FABRIK(ArmRight, ResetTargets[4] + legOffset, ResetTargets[1]);
        Reset_FABRIK(LegLeft, (ResetTargets[7] - legOffset), ResetTargets[2]);
        Reset_FABRIK(LegRight, (ResetTargets[7] + legOffset), ResetTargets[3]);

        GameObject[][] limbs = { Spine, Neck, ArmLeft, ArmRight, LegLeft, LegRight };
        Vector3[][] reset_positions = { SpineResetPosition, NeckResetPosition, ArmLeftResetPosition, ArmRightResetPosition, LegLeftResetPosition, LegRightResetPosition };
        Quaternion[][] reset_rotations = { SpineResetRotation, NeckResetRotation, ArmLeftResetRotation, ArmRightResetRotation, LegLeftResetRotation, LegRightResetRotation};

        for (int i = 0; i<limbs.Length; i++)
        {
            for(int j=0; j<limbs[i].Length; j++)
            {
                reset_positions[i][j] = limbs[i][j].transform.position;
                reset_rotations[i][j] = limbs[i][j].transform.rotation;

            }

        }
    }

    private void reset_model()
    {
        GameObject[][] limbs = {  ArmLeft, ArmRight, LegLeft, LegRight, Spine, Neck };
        Vector3[][] reset_positions = { ArmLeftResetPosition, ArmRightResetPosition, LegLeftResetPosition, LegRightResetPosition, SpineResetPosition, NeckResetPosition };
        Quaternion[][] reset_rotations = {ArmLeftResetRotation, ArmRightResetRotation, LegLeftResetRotation, LegRightResetRotation, SpineResetRotation, NeckResetRotation };

        for (int i = 0; i < limbs.Length; i++)
        {
            for (int j = 0; j < limbs[i].Length; j++)   
            {
                limbs[i][j].transform.position = reset_positions[i][j] + Roots[0].transform.position;
                limbs[i][j].transform.rotation = reset_rotations[i][j];

            }

        }
    }

    private void Reset_FABRIK(GameObject[] limb, Vector3 resetRoot, Vector3 resetTarget)
    {
        for (int i = 0; i <= 10; i++)
        {
            backwardSolve(limb, resetTarget);
            forwardSolve(limb, resetRoot, resetTarget);
        }
    }

    private void FABRIK(GameObject[] limb, Vector3 root, Vector3 target, Vector3 resetRoot, Vector3 resetTarget)
    {
        //Adjust target to max distance for limb
        float limb_len = 0;
        foreach(GameObject joint in limb)
        {
            FABRIKJoint jointObject;
            if (joint.TryGetComponent<FABRIKJoint>(out jointObject))
            {
                limb_len += jointObject.segmentLen;
            }
        }
        if(Vector3.Distance(root, target) > limb_len)
        {
            target = ((target - root).normalized * limb_len) + root;
           
        }

        int i = 0;
        backwardSolve(limb, target);
        forwardSolve(limb, root, target);
        while (Vector3.Distance(limb[limb.Length - 1].transform.position, target) > tolorance)
        {

            if (i < maxIterations)
            {
                backwardSolve(limb, target);
                forwardSolve(limb, root, target);

            }
            else { 
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
            Vector3 faceDir = limb[i + 1].transform.position - limb[i].transform.position;

            Debug.DrawLine(limb[i].transform.position, (limb[i].transform.position + limb[i].transform.up), Color.gray);

        }

        return limb;
    }

    //start with root move towards end effector
    GameObject[] forwardSolve(GameObject[] limb, Vector3 root, Vector3 target)
    {
        int n = limb.Length;
        limb[0].transform.position = root;
        //limb[n - 1].transform.rotation = Quaternion.LookRotation(limb[n-1].transform.position - limb[0].transform.position, Vector3.up);

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
            if (usePositionalConstraints)
            {
                moveDir = joint.constrain(curr, moveDir);
            }

            limb[i + 1].transform.position = moveDir;

            //rotate curr to face the repositioned next
            Vector3 faceDir = limb[i + 1].transform.position - limb[i].transform.position;

            if (faceDir != Vector3.zero && useRotationConstraints)
            {
                limb[i].transform.rotation = joint.constrainRotation(faceDir, target);
            }
            else
            {
                limb[i].transform.rotation = Quaternion.LookRotation(faceDir);
            }



            Debug.DrawLine(limb[i].transform.position, (limb[i].transform.position + limb[i].transform.up), Color.white);
            Debug.DrawLine(limb[i].transform.up + limb[i].transform.position, limb[i + 1].transform.position);


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
