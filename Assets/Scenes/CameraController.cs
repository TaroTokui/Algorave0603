using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using uOSC;

public class CameraController : MonoBehaviour
{

    public enum CameraType
    {
        SimpleRotation,
        RondomMove
    }


    public Transform targetObject;

    [SerializeField]
    public bool StopCamera = false;

    [SerializeField]
    private CameraType currentCameraType = CameraType.SimpleRotation;

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

    [SerializeField]
    [Range(0, 1)]
    public float MoveThreshold = 0.5f;

    [SerializeField]
    bool useOsc = true;

    [SerializeField]
    OscReceiver _OscReceiver;

    /// 音声入力
    float _InputLow = 0.0f;
    float _InputMid = 0.0f;
    float _InputHigh = 0.0f;
    float _InputKickdetection = 0.0f;
    float _InputSnaredetection = 0.0f;
    float _InputRythm = 0.0f;
    float _InputSpectralcentroid = 0.0f;
    float _InputFmp = 0.0f;
    float _InputSmp = 0.0f;

    // private variables
    private Vector3 mPosition;
    private Vector3 mVelocity;
    private float mAngle;
    private Vector3 mTargetPosition;


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

        // grab osc params
        if (useOsc)
        {
            _InputLow = _OscReceiver._InputLow;
            _InputMid = _OscReceiver._InputMid;
            _InputHigh = _OscReceiver._InputHigh;
            _InputKickdetection = _OscReceiver._InputKickdetection;
            _InputSnaredetection = _OscReceiver._InputSnaredetection;
            _InputRythm = _OscReceiver._InputRythm;
            _InputSpectralcentroid = _OscReceiver._InputSpectralcentroid;
            _InputFmp = _OscReceiver._InputFmp;
            _InputSmp = _OscReceiver._InputSmp;
        }

        switch(currentCameraType)
        {
            case CameraType.SimpleRotation:
                SimpleRotation();
                break;
            case CameraType.RondomMove:
                RondomMove();
                break;
            default:
                break;
        }

    }

    void SimpleRotation()
    {
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

    void RondomMove()
    {
        if(_InputSnaredetection > MoveThreshold)
        {
            mTargetPosition.x = Random.value;
            mTargetPosition.y = Random.value + 0.2f;
            mTargetPosition.z = Random.value;
        }

        mPosition = Vector3.Lerp(mPosition, mTargetPosition * 100, 0.1f);

        this.transform.position = mPosition;
        //this.transform.LookAt(targetObject);
        this.transform.LookAt(targetObject.position + new Vector3(mPosition.x, 0, mPosition.z) * PitchLevel);
    }
}
