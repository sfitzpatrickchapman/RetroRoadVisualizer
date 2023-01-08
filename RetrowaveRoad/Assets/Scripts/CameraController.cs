using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public static CameraController m_instance;

    [Range(0f, 50f)] public float speed = 5f;


    void Start()
    {
        m_instance = this; 
    }

    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        camPos.z += Time.deltaTime * speed;
        Camera.main.transform.position = camPos;
    }
}
