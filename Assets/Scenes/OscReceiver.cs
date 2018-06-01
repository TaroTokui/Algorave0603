using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uOSC
{
    [RequireComponent(typeof(uOscServer))]
    public class OscReceiver : MonoBehaviour
    {
        [Range(0, 1)]
        public float _InputLow;
        [Range(0, 1)]
        public float _InputMid;
        [Range(0, 1)]
        public float _InputHigh;
        public float _InputKickdetection;
        public float _InputSnaredetection;
        public float _InputRythm;
        [Range(0, 1)]
        public float _InputSpectralcentroid;
        [Range(0, 1)]
        public float _InputFmp;
        [Range(0, 1)]
        public float _InputSmp;

        // Use this for initialization
        void Start()
        {
            var server = GetComponent<uOscServer>();
            server.onDataReceived.AddListener(OnDataReceived);
        }

        void OnDataReceived(Message message)
        {
            // address
            var msg = message.address;
            //Debug.Log(msg);
            switch (msg)
            {
                case "/Low":
                    //Debug.Log("low");
                    _InputLow = (float)message.values[0];
                    break;
                case "/Mid":
                    //Debug.Log("mid");
                    _InputMid = (float)message.values[0];
                    break;
                case "/High":
                    _InputHigh = (float)message.values[0];
                    break;
                case "/Kickdetection":
                    _InputKickdetection = (float)message.values[0];
                    break;
                case "/Snaredetection":
                    _InputSnaredetection = (float)message.values[0];
                    break;
                case "/Rythm":
                    _InputRythm = (float)message.values[0];
                    break;
                case "/Spectralcentroid":
                    _InputSpectralcentroid = (float)message.values[0];
                    break;
                case "/Fmp":
                    _InputFmp = (float)message.values[0];
                    break;
                case "/Smp":
                    _InputSmp = (float)message.values[0];
                    break;
                default:
                    //Debug.Log("Incorrect intelligence level.");
                    break;
            }
        }
    }
}
