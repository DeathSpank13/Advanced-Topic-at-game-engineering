using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraPosition : MonoBehaviour
{


    public Transform cameraPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
