using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float movementSpeed = 0.001f;
    public float tickTime = 0.05f;
    public bool isActive = true;

    public Transform cameraPosition; 

    // Start is called before the first frame update
    void Start()
    {
        cameraPosition = Camera.main.transform;
        StartCoroutine(MoveCamera());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator MoveCamera() {
        while(isActive)
        {
            cameraPosition.position = cameraPosition.position + cameraPosition.right * movementSpeed;
            yield return new WaitForSeconds(tickTime);
        }
        yield break;
    }
}
