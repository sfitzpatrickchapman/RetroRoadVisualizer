using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Range(0f, 25f)] public float speed = 5f;

    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        camPos.z += Time.deltaTime * speed;
        Camera.main.transform.position = camPos;
    }
}
