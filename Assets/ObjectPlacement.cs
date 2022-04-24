using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacement : MonoBehaviour
{
    [Header("List of placeable Objects")]
    public List<GameObject> placeables;
    public List<GameObject> items;
    [Range(0.0f, 1.0f)]
    public float ItemRate;

    [Header("Preview")]
    [Range(0.0f, 0.1f)]
    public float PreviewScale;
    public Transform PreviewTransform;

    public float PlacedItemScale = 1f;

    [Header("Audio")]
    public List<AudioClip> insults;
    [Range(0f, 1f)]
    public float InsultRate;
    public AudioClip warning;

    public GameObject PreviewBackground;



    // placeable following the Cursor
    private GameObject currentPlaceable;
    // placeable displayed in the Preview
    private GameObject nextPlaceable;



    // Start is called before the first frame update
    void Start()
    {
        // initiate currentPlaceable
        int id = Random.Range(0, placeables.Count);
        currentPlaceable = Instantiate(placeables[id], transform);
        currentPlaceable.transform.localScale = new Vector3(PlacedItemScale, PlacedItemScale, PlacedItemScale);
        currentPlaceable.layer = 5;
        
        // initiate nextPlaceable
        id = Random.Range(0, placeables.Count);
        nextPlaceable = GenerateNextPlaceable();
    }

    // Update is called once per frame
    void Update()
    {
        // make the GameObject this script is attached to follow the Cursor
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0-Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        transform.position = worldPos;

        // user input
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Pressed left click.");

            if (currentPlaceable.GetComponent<PlacementCollision>().collisions == 0)
            {
                // detach currentPlaceable from Cursor and trigger effects
                currentPlaceable.transform.parent = null;
                currentPlaceable.layer = 0;
                currentPlaceable.GetComponent<Placeable>().Place();

                // attach nextPlaceable to Cursor
                currentPlaceable = nextPlaceable;
                currentPlaceable.transform.parent = null;
                currentPlaceable.transform.localScale = new Vector3(PlacedItemScale, PlacedItemScale, PlacedItemScale);
                currentPlaceable.transform.position = transform.position;
                currentPlaceable.transform.parent = transform;

                nextPlaceable = GenerateNextPlaceable();
            }
            else {
                Debug.Log("Ihr könnt das nicht dort platzieren, Mylord");

                AudioSource audioData = GetComponent<AudioSource>();
                float selectPlaylist = Random.Range(0f, 1f);
                if (selectPlaylist <= InsultRate)
                {
                    audioData.clip = insults[Random.Range(0, insults.Count)];
                }
                else {
                    audioData.clip = warning;
                }
                audioData.Play(0);
            }
        }
    }



    private GameObject GenerateNextPlaceable() {
        List<GameObject> collection;
        
        float spawnItem = Random.Range(0f, 1f);
        if (spawnItem <= ItemRate)
        {
            collection = items;
        }
        else {
            collection = placeables;
        }
        
        int id = Random.Range(0, collection.Count);
        GameObject placeable = Instantiate(collection[id]);
        
        Color color = new Color(Random.Range(0F, 1F), Random.Range(0, 1F), Random.Range(0, 1F));
        placeable.GetComponent<Renderer>().material.color = color;
        
        placeable.layer = 5;
        placeable.transform.localScale *= PreviewScale;
        placeable.transform.position = PreviewTransform.position;
        placeable.transform.parent = PreviewBackground.transform;
        
        return placeable;
    }

    private bool CollisionCheck() {
        
        
        return false;
    }
}
