using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [Header("Kill Trigger Objects")]
    public GameObject leftTrigger;
    public GameObject bottomTrigger;
    public int killTriggerOffset = 20;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 stageDimensions = Camera.main.ScreenToWorldPoint(new Vector3(-killTriggerOffset, -killTriggerOffset, -Camera.main.transform.position.z));

        Debug.Log("stageDimensions: " + stageDimensions);
        Vector3 oldPosLeftTrigger = leftTrigger.transform.position;
        leftTrigger.transform.position = new Vector3(stageDimensions.x, oldPosLeftTrigger.y, oldPosLeftTrigger.z);

        Vector3 oldPosBottomTrigger = bottomTrigger.transform.position;
        bottomTrigger.transform.position = new Vector3(oldPosBottomTrigger.x, stageDimensions.y, oldPosBottomTrigger.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
