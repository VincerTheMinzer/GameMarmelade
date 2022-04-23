using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySpecialMovement : MonoBehaviour
{
    private Rigidbody myrigid;
    public GameObject MyCameraGO;
    public Transform MyCameraTrans;

    //MOVEMENT
    [Header("Movement Variables")]
    public float maxSpeed = 5;
    public float AddSpeed = 0.5f;
    public float breakSpeed = 0.2f;
    float curSpeed = 0;

    private float breakpercent = 0;

    Vector3 playerMoveDir = Vector3.zero;
    Vector3 moveDir = Vector3.zero;
    Vector3 curMoveDir = Vector3.zero;
    bool hadPlayerInput = false;
    bool setupcomplete = false;

    // Start is called before the first frame update
    void Start()
    {
        myrigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        hadPlayerInput = false;

        if (Input.GetKey(KeyCode.D))
        {
            playerMoveDir += transform.forward * (AddSpeed * Time.deltaTime);
            hadPlayerInput = true;
        }

        if (Input.GetKey(KeyCode.A))
        {
            playerMoveDir -= transform.forward * (AddSpeed * Time.deltaTime);
            hadPlayerInput = true;
        }

        ApplySpeed();
    }

    private void ApplySpeed()
    {
        if (!hadPlayerInput)
        {
            if(!setupcomplete)
                moveDir = playerMoveDir;

            curMoveDir = Vector3.Lerp(moveDir, Vector3.zero, Mathf.Clamp(breakpercent += (breakSpeed * Time.deltaTime), 0, 1));
            playerMoveDir = curMoveDir;
            setupcomplete = true;
            curSpeed -= breakSpeed;
        }
        else
        {
            setupcomplete = false;
            breakpercent = 0;
            curSpeed += AddSpeed;
            curMoveDir = playerMoveDir;
        }

        myrigid.velocity = curMoveDir;
    }
}
