using UnityEngine;

public class FABRIKBallSocketJoint : FABRIKJoint
{
    [Tooltip("Maximum angle of movement with regards to the projection axis")]
    [SerializeField] float swingConstraint;
    [Tooltip("Maximum twist rotation of the joint around the projection axis. (between -179 and 180)")]
    [SerializeField] float twistMax;
    [Tooltip("Minimum twist rotation of the joint around the projection axis. (between -179 and 180)")]
    [SerializeField] float twistMin;
    private Vector3 projectionAxis = Vector3.up;

    public override Quaternion ConstrainRotation(Quaternion startRotation)
    {
        Quaternion swing;
        Quaternion twist;
        startRotation.decompose(projectionAxis, out swing, out twist);

        //Constrain twist to +- twistConstraint
        //1. deconstruct our twist into an axis to rotate around
        // and an angle to rotate
        float angle;
        Vector3 twistAxis;
        twist.ToAngleAxis(out angle, out twistAxis);

        //2. wrap our angle between 180 and -180 and make sure our axis
        // faces the right direction cus Unity bungles sometimes
        if (angle > 180) angle -= 360;
        if (Vector3.Dot(twistAxis, projectionAxis) < 0) angle = -angle;

        //3. clamp our angle
        angle = Mathf.Clamp(angle, twistMin, twistMax);

        //4. reconstruct our twist Quaternion
        twist = Quaternion.AngleAxis(angle, twistAxis);

        //Time to recombine and check if we need to clamp swing
        Vector3 swungAxis = swing * twistAxis.normalized;
        float swingAngle = Vector3.Angle(twistAxis, swungAxis);
        if(swingAngle > swingConstraint)
        {
            //If were in here then we we need to slerp to find a rotation 
            // that moves towards our swung but only up to an acceptable angle
            //1. Take the cross product of twist and swung to find an axis to rotate around
            Vector3 swingAxis = Vector3.Cross(swungAxis, twistAxis);
            //2. check for the no swing or swung 180 degree case
            // if not the case then construct 
            if(swingAxis == Vector3.zero)
            {
                swing = Quaternion.identity;
            }
            else
            {
                //creates a quaternion that when multiplied with twist will swing it 
                // our maxSwing angle around the cross product axis, thus putting it into an acceptable place.
                swing = Quaternion.AngleAxis(swingConstraint, swingAxis);
            }
               
        }
        return swing * twist;



    }
}
