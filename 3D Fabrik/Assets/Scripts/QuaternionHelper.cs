using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionHelper
{
    public static void decompose(this Quaternion Q, Vector3 Direction, out Quaternion swing, out Quaternion twist)
    {
        Vector3 P = Vector3.Project(new Vector3(Q.x, Q.y, Q.z), Direction);
        twist = new Quaternion(P.x, P.y, P.z, Q.w);
        swing = Q * Quaternion.Inverse(twist);
    }
}
