using UnityEngine;

public class FABRIKPivotJoint : FABRIKJoint
{
    [Tooltip("Maximum twist rotation of the joint around the projection axis. (between -179 and 180)")]
    [SerializeField] float twistMax;
    [Tooltip("Minimum twist rotation of the joint around the projection axis. (between -179 and 180)")]
    [SerializeField] float twistMin;
    //This joint will only rotate around this vector without moving, using transform.up for now
    private Vector3 projectionAxis;

    private void Start()
    {
        projectionAxis = transform.up;
    }
    public override Quaternion ConstrainRotation(Quaternion startRotation)
    {
        Quaternion twist;
        Quaternion swing;

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

        return twist;

    }
}
