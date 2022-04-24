using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float movementSpeed = 0.001f;
    public float tickTime = 0.05f;
    public bool isActive = true;

    public float CamSpeedIncreaseInPercent = 5;
    public float[] EverySeconds = new float[5] { 1, 5, 5, 10, 20 };

    public Transform cameraPosition;

    public bool IncreaseSpeedOn = true;

    IEnumerator IncreaseSpeedOverTime()
    {
        int curSpeed = 0;

        while (IncreaseSpeedOn)
        {
            movementSpeed += movementSpeed*(CamSpeedIncreaseInPercent*0.01f);
            yield return new WaitForSeconds(EverySeconds[curSpeed]);
            if(curSpeed < EverySeconds.Length-1)
                curSpeed++;
        }
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraPosition = Camera.main.transform;
        StartCoroutine(MoveCamera());
    }

    public void SwitchIsActive()
    {
        isActive = !isActive;
        if (isActive)
            StartCoroutine(MoveCamera());
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
