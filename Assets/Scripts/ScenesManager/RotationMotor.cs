using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationMotor : MonoBehaviour
{
    private float logicalAngleZ;
    
    
    
    // Start is called before the first frame update
    private void Awake()
    {
        logicalAngleZ = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, logicalAngleZ);   // 绕 Z 轴
        
    }
}
