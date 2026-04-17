using UnityEngine;

public class PivotJoint : FABRIKJoint
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override Vector3 constrain(Vector3 L, Vector3 target)
    {
        //only really need X constraint for elbows and knees
        return transform.position + parentJoint.transform.position.normalized * segmentLen;

    }
}
