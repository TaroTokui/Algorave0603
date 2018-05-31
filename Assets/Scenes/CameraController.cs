using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{


    public Transform targetObject;

    [SerializeField]
    public bool StopCamera = false;

    [SerializeField]
    [Range(-50, 50)]
    public float RotationSpeed = 10;

    [SerializeField]
    [Range(0, 300)]
    public float Distance = 100;

    [SerializeField]
    [Range(0, 90)]
    public float HeightAngle = 45;

    [SerializeField]
    [Range(0, 1)]
    public float PitchLevel = 0;

    // private variables
    private Vector3 mPosition;
    private Vector3 mVelocity;
    private float mAngle;


    // Use this for initialization
    void Start()
    {
        mPosition = new Vector3(0, 0, 0);
        mAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (StopCamera) return;

        mAngle = Mathf.Deg2Rad * (Time.time % 360) * RotationSpeed;

        float h = Mathf.Sin(HeightAngle * Mathf.Deg2Rad) * Distance;
        float w = Mathf.Cos(HeightAngle * Mathf.Deg2Rad) * Distance;

        float x = Mathf.Cos(mAngle) * w;
        float z = Mathf.Sin(mAngle) * w;

        mPosition.x = x;
        mPosition.y = h;
        mPosition.z = z;
        //this.transform.Rotate(0, (Input.GetAxis("Horizontal") * 1), 0);
        //this.transform.Translate(mPosition);
        this.transform.position = mPosition;
        //this.transform.LookAt(targetObject);
        this.transform.LookAt(targetObject.position + new Vector3(mPosition.x, 0, mPosition.z) * PitchLevel);
    }
}
