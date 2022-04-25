using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(ParticleSystem))]
public class Placeable : MonoBehaviour
{
    private Mesh mesh;
    private ParticleSystem ps;
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).localScale = transform.localScale;
        ps = transform.GetChild(0).GetComponent<ParticleSystem>();
        var sh = ps.shape;
        sh.enabled = true;
        sh.shapeType = ParticleSystemShapeType.Mesh;
        sh.mesh = transform.GetComponent<MeshFilter>().mesh;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Place() {
        Debug.Log("Placed");
        ps.Play();
    }
}