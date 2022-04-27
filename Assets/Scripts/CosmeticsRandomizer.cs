using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticsRandomizer : MonoBehaviour
{

    [Range(0, 1)]
    public float chanceThatCosmeticsAreMissing = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        int numberOfCosemtics = transform.childCount;

        for (int i = 0; i < numberOfCosemtics; i++)
        {
            if (Random.Range(0f, 1f) <= chanceThatCosmeticsAreMissing)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
