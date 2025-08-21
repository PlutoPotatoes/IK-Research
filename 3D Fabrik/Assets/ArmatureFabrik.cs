using System.Numerics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmatureFabrik : MonoBehaviour
{
    [SerializeField] GameObject RightFootTarget;
    [SerializeField] GameObject LeftFootTarget;
    [SerializeField] GameObject Root;

    private GameObject GeometryRoot;
    private List<GameObject> IKChains; // replace type with Effector once completed

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FABRIK()
    {
        //For each chain in sorted IKChains, call forward and then backward
    }
}
