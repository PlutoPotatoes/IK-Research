using UnityEngine;

public class Fabrik : MonoBehaviour
{
    [SerializeField] GameObject jointInstance;
    [SerializeField] GameObject endEffectorInstance;
    [SerializeField] GameObject boneInstance;

    public int segmentCount;
    public float segmentLen;
    public float tolorance;
    public GameObject rootObject;
    public GameObject targetObject;
    public float rotMax_x;
    public float rotMax_y;



    private GameObject[] segments;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        segments = new GameObject[segmentCount];
        float pos = 0;
        for (int i = 0; i < segmentCount - 1; i++) 
            {
                GameObject seg = Instantiate(jointInstance, this.transform);
                seg.transform.position = new Vector3(0, pos, 0);
                pos += segmentLen;
                segments[i] = seg;
            }
            GameObject endEffector = Instantiate(endEffectorInstance, this.transform);
            endEffector.transform.position = new Vector3(0, pos, 0);
            segments[segmentCount - 1] = endEffector;
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
        Vector3 root = rootObject.transform.position;
        Vector3 target = targetObject.transform.position;
        if(Vector3.Distance(root, target) >= segmentLen * segmentCount)
        {
            target = (target - root).normalized * (segmentLen * segmentCount);
        }

        forwardReach(target);
        backwardReach(root);
    }

    void forwardReach(Vector3 target)
    {
        //move end effector to the target
        segments[segmentCount - 1].transform.rotation = Quaternion.LookRotation(target);
        segments[segmentCount - 1].transform.position = target;

        //FIXME rotate end effector and attached bone
        for(int i = segmentCount-2; i>=0; i--)
        {
            //get the joint position of the one that just moved
            Vector3 curr = segments[i + 1].transform.position;
            //get joint to move position
            Vector3 next = segments[i].transform.position;

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized * segmentLen;
            //shift the position of the joint ahead back by move dir to get the next segment's position
            moveDir = curr - moveDir;
            segments[i].transform.position = moveDir;

        }
    }
    void backwardReach(Vector3 root)
    {
        segments[0].transform.position = root;
        //FIXME rotate end effector and attached bone
        for (int i = 0; i < segmentCount-1; i++)
        {
            //get the joint position of the one that just moved
            Vector3 curr = segments[i].transform.position;
            //get joint to move position (the joint 1 ahead in the chain)
            Vector3 next = segments[i+1].transform.position;

            //get direction from vector facing from next to curr and scale it to segment length
            Vector3 moveDir = (curr - next).normalized * segmentLen;
            //shift the position of the joint ahead back by move dir to get the next segment's position
            moveDir = curr - moveDir;

            segments[i+1].transform.position = moveDir;

            //rotate curr to face the repositioned next
            Vector3 faceDir = next - curr;
            segments[i].transform.rotation = Quaternion.LookRotation(faceDir.normalized);
            /*
                curr.x, curr.y
                next.x, next.y
                
                
            */


            segments[i].transform.rotation = Quaternion.LookRotation(faceDir.normalized);



            /* 
             * extend curr's direction vector by segmentLen
             * cast a capsule collider centered at that point with the same rotation?
             * moveDir = 
             * 
             * 
             * */
        }
    }
}
