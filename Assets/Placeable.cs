using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placeable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Place() {
        Debug.Log("Placed");
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
    }
}
