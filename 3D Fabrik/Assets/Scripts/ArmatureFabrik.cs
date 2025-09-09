using System.Numerics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmatureFabrik : MonoBehaviour
{
    [SerializeField] GameObject RightFootTarget;
    [SerializeField] GameObject LeftFootTarget;
    [SerializeField] GameObject RightHandTarget;
    [SerializeField] GameObject LeftHandTarget;
    [SerializeField] GameObject Root;

    [SerializeField] GameObject[] leftArm;
    [SerializeField] GameObject[] rightArm;
    [SerializeField] GameObject[] leftLeg;
    [SerializeField] GameObject[] rightLeg;
    [Tooltip("hips, left upper leg, right upper leg")]
    [SerializeField] GameObject[] lowerBodyTriangle;
    [Tooltip("Hip, spine, chest, upper chest")]
    [SerializeField] GameObject[] spine;
    [Tooltip("Upper Chest, left shoulder, right shoulder")]
    [SerializeField] GameObject[] upperBodyTriangle;
    [Tooltip("upper chest -> neck bone simplified for easy pivot joint manipulation")]
    [SerializeField] GameObject[] neck;


    /*
     * TODO:
     * 1. create static processor function that solves the body start to finish
     * 2. add limb types to joints and solve with constraints
     * 3. encorperate centroids for shared subroots
     * 4. create a queue that solves moved limbs first
     */


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void staticFABRIK()
    {
        //For each chain in sorted IKChains, call forward and then backward
    }
}
