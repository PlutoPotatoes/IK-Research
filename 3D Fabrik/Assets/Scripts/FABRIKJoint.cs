using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FABRIKJoint : MonoBehaviour
{
    [SerializeField] GameObject[] connectedJoints;
    [SerializeField] bool isSubBase;
    [HideInInspector]
    public float boneLength = 1;

    //experimental section with centroids
    [HideInInspector]
    public List<Vector3> storedPositions;

    private void Start()
    {
        if (connectedJoints.Length > 0)
        {
            foreach (GameObject joint in connectedJoints)
            {
                boneLength += (transform.position - joint.transform.position).magnitude;
            }
            boneLength /= connectedJoints.Length;
        }
    }
    public virtual Quaternion ConstrainRotation(Quaternion startRotation)
    {
        return startRotation;
    }

    //add array to store positions and a function to return centroid
    public Vector3 getCentroid()
    {
        Vector3 centroid = Vector3.zero;
        foreach(Vector3 pos in storedPositions)
        {
            centroid += pos;
        }
        centroid /= storedPositions.Count;
        storedPositions.Clear();
        return centroid;
    }
    
    

}
