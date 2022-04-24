using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementCollision : MonoBehaviour
{

    public int collisions = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        collisions++;
        Debug.Log("Collisions: " + collisions);
    }

    private void OnTriggerExit(Collider other)
    {
        if (collisions > 0)
        {
            collisions--;
        }
    }

}
