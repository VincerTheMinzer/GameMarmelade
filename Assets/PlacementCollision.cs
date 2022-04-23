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

    void OnCollisionEnter(Collision collision)
    {
        collisions++;
        Debug.Log("Collisions: " + collisions);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collisions > 0)
        {
            collisions--;
        }
    }

}
