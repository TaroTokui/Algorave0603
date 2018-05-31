using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uOSC
{
    [RequireComponent(typeof(uOscServer))]
    public class OscReceiver : MonoBehaviour
    {
        //[SerializeField]
        public float _InputLow;

        //[SerializeField]
        public float _InputMid;

        //[SerializeField]
        public float _InputHi;

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
                case "/Hi":
                    _InputHi = (float)message.values[0];
                    break;
                default:
                    //Debug.Log("Incorrect intelligence level.");
                    break;
            }
        }
    }
}
