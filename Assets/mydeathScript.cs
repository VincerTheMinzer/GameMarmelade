using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mydeathScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(deathCor());
    }

    IEnumerator deathCor() {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
        yield break;
    }
}
