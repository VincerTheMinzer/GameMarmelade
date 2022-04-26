using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpawnObjects : MonoBehaviour
{
    public float noiseOffsetX = 0.0f;
    public float noiseStepSize = 0.1f;

    public List<GameObject> preBuiltObstacles;
    [Range(0, 1)]
    public float chanceThatTilesAreMissing = 0.3f;
    [Range(0, 1)]
    public float maxPercentOfMissingTiles = 0.5f;

    [Range(0, 1)]
    public float chanceThatCosmeticsAreMissing = 0.3f;

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

        var newObj = curRightMostObject;
        int numberOfElements = newObj.transform.Find("Elements").transform.childCount;
        
        List<int> alreadyRemoved = new List<int>();

        if (Random.Range(0f, 1f) <= chanceThatTilesAreMissing) 
        {
            Debug.Log("Yees");
            Debug.Log("Number of Elements: " + numberOfElements);

            int maxItemsToRemove = Mathf.RoundToInt(numberOfElements * maxPercentOfMissingTiles);
            Debug.Log("Max Items to remove: " + maxItemsToRemove);
            int itemsToRemove = Random.Range(0, maxItemsToRemove + 1);
            Debug.Log("Items to remove: " + itemsToRemove);

            while(itemsToRemove > 0)
            {
                int childNumber = RandomRangeExcept(0, numberOfElements, alreadyRemoved);
                Debug.Log("Child number: " + childNumber + " , Already Removed: " + alreadyRemoved.Count);

                alreadyRemoved.Add(childNumber);

                newObj.transform.Find("Elements").transform.GetChild(childNumber).gameObject.SetActive(false);

                itemsToRemove--;
            }
        } else
        {
            Debug.Log("nope");
        }
        
        noiseOffsetX += noiseStepSize;
        return curRightMostObject;
    }

    private int RandomRangeExcept(int min, int max, List<int> except){
        var number = 0;
        do {
                number = Random.Range(min, max);
        } while (except.Contains(number)) ;
        return number;
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


