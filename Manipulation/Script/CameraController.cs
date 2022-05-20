using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 10f;
    CharacterController controller;

    void Start()
    {
        controller = this.GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = 0;
        y += (Input.GetKey(KeyCode.Q)) ? 1 : 0;
        y += (Input.GetKey(KeyCode.E)) ? -1 : 0;

        controller.Move(new Vector3(x, y, z) * speed * Time.deltaTime);
    }
}
