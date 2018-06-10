using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshGrabber : MonoBehaviour {
    
    [SerializeField] protected SkinnedMeshRenderer skinned;
    protected Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
    }

    // Update is called once per frame
    void Update()
    {
        skinned.BakeMesh(mesh);
        //Debug.Log(mesh.vertices[0]);
    }

    public Mesh getMesh()
    {
        return mesh;
    }
}
