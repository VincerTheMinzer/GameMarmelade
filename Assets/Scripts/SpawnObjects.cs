using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpawnObjects : MonoBehaviour
{
    public float noiseOffsetX = 0.0f;
    public float noiseStepSize = 0.1f;

    public List<GameObject> preBuiltObstacles;
    private List<GameObject> instantiatedObjects;
    public GameObject curRightMostObject;

    public GameObject myPrefab1;
    public CameraMovement cm;
    bool waitForCoroutine = true;

    // Start is called before the first frame update
    void Start()
    {
        instantiatedObjects = new List<GameObject>();
        StartCoroutine(startSpawn());
    }

    IEnumerator startSpawn()
    {
        bool stuff = true;
        int bla = Random.Range(0, preBuiltObstacles.Count);
        Vector3 myPlacableStartpos = preBuiltObstacles[bla].transform.GetChild(0).localPosition;
        Vector3 myPlacableEndpos = preBuiltObstacles[bla].transform.GetChild(1).localPosition;
        float mydistance = Vector3.Distance(myPlacableStartpos, myPlacableEndpos);
        Vector3 leftMost = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, -Camera.main.transform.position.z)) + new Vector3(mydistance / 2, 0, 0);
        Debug.Log("LEFTMOST: " + leftMost);
        cm.SwitchIsActive();

        while (stuff)
        {
            GameObject useMeLaterDaddy = SpawnPiece(bla, leftMost);
            bla = Random.Range(0, preBuiltObstacles.Count);
            myPlacableStartpos = preBuiltObstacles[bla].transform.GetChild(0).localPosition;
            myPlacableEndpos = preBuiltObstacles[bla].transform.GetChild(1).localPosition;
            mydistance = Vector3.Distance(myPlacableStartpos, myPlacableEndpos);
            leftMost.x += mydistance;
            yield return new WaitForSeconds(0.2f);
            
            if (!isInFrustum(useMeLaterDaddy))
                stuff = false;

        }

        cm.SwitchIsActive();
        waitForCoroutine = !waitForCoroutine;

        yield break;
    }

    bool isInFrustum(GameObject amIinFrustum)
    {
        Vector3 viewport2 = Camera.main.WorldToViewportPoint(amIinFrustum.transform.GetChild(1).position);
        return Is01(viewport2.x) && Is01(viewport2.y);
    }

    private GameObject SpawnPiece(int SpawnNr, Vector3 SpawnPos) 
    {
        float perlinNoiseOffset = Mathf.PerlinNoise(noiseOffsetX, 0);
        float mappedValue = (1.5f * perlinNoiseOffset) - 0.75f;
        Vector3 ScreenTop = new Vector3(0, Screen.height, -Camera.main.transform.position.z);
        Vector3 ScreenBot = new Vector3(0, 0, -Camera.main.transform.position.z);
        float distance = Vector3.Distance(Camera.main.ScreenToWorldPoint(ScreenTop), Camera.main.ScreenToWorldPoint(ScreenBot));
        float newScreenPos = (distance / 2) * mappedValue;
        Vector3 resVec = new Vector3(SpawnPos.x, newScreenPos, SpawnPos.z);
        curRightMostObject = Instantiate(preBuiltObstacles[SpawnNr], resVec, Quaternion.identity);
        instantiatedObjects.Add(curRightMostObject);
        noiseOffsetX += noiseStepSize;
        return curRightMostObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (waitForCoroutine)
            return;

        //spawn new if currently rightmost obstacle is just out of frame
        if(!curRightMostObject) { Debug.Log("Outside of screen boi");  return; }
        Vector3 viewport = Camera.main.WorldToViewportPoint(curRightMostObject.transform.GetChild(0).position);
        bool inCameraFrustum = Is01(viewport.x) && Is01(viewport.y);
        bool inFrontOfCamera = viewport.z > 0;

        if (inCameraFrustum && inFrontOfCamera)
        {
            int bla = Random.Range(0, preBuiltObstacles.Count);
            Vector3 myPlacableStartpos = preBuiltObstacles[bla].transform.GetChild(0).localPosition;
            Vector3 myPlacableEndpos = preBuiltObstacles[bla].transform.GetChild(1).localPosition;
            float mydistance = Vector3.Distance(myPlacableStartpos, myPlacableEndpos);
            Vector3 rightMost = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2, -Camera.main.transform.position.z)) + new Vector3(mydistance/2, 0, 0);
            SpawnPiece(bla, rightMost);
        }

        //remove leftmost object if endpos just left the frame
        Vector3 viewport2 = Camera.main.WorldToViewportPoint(instantiatedObjects[0].transform.GetChild(0).position);
        bool inCameraFrustum2 = Is01(viewport2.x) && Is01(viewport2.y);

        if (!inCameraFrustum2)
        {
            Destroy(instantiatedObjects[0].gameObject);
            instantiatedObjects.RemoveAt(0);
        }
    }

    private bool Is01(float a)
    {
        return a > 0 && a < 1;
    }
}


