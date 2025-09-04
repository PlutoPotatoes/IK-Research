using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FABRIKJoint : MonoBehaviour
{
    public virtual Quaternion ConstrainRotation(Quaternion startRotation)
    {
        return startRotation;
    }


}
