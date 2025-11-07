using UnityEngine;

public class TargetAnimator : MonoBehaviour
{
    [SerializeField] GameObject RightFoot;
    [SerializeField] GameObject LeftFoot;
    [SerializeField] GameObject RightHand;
    [SerializeField] GameObject LeftHand;
    [SerializeField] GameObject Hip;
    [SerializeField] GameObject Chest;
    [SerializeField] GameObject Head;

    [SerializeField] GameObject RightFootTarget;
    [SerializeField] GameObject LeftFootTarget;
    [SerializeField] GameObject RightHandTarget;
    [SerializeField] GameObject LeftHandTarget;
    [SerializeField] GameObject HipTarget;
    [SerializeField] GameObject ChestTarget;
    [SerializeField] GameObject HeadTarget;

    [SerializeField] Vector3 offset;

    void Update()
    {
        RightFootTarget.transform.position = RightFoot.transform.position + offset;
        LeftFootTarget.transform.position = LeftFoot.transform.position + offset;
        RightHandTarget.transform.position = RightHand.transform.position + offset;
        LeftHandTarget.transform.position = LeftHand.transform.position + offset;
        HipTarget.transform.position = Hip.transform.position + offset;
        ChestTarget.transform.position = Chest.transform.position + offset;
        HeadTarget.transform.position = Head.transform.position + offset;
    }
}
