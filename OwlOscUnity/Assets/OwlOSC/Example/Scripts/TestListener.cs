using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OwlOSC;
using UnityEngine.UI;

public class TestListener : MonoBehaviour
{
    public int localPort = 55555;
    public string addressPath = "/test";
    [Range(0.001f,10f)]
    public float readDelay = 1f;
    public Text uiText;

    UDPListener listener;

    private void OnEnable() {
        //Instantiate Listener
        //In Unity only enqueue callbacks because objects can be instantiated only in main thread
        listener = new UDPListener(localPort);
        //Register Log to all address with fast wildcard
        listener.AddAddress("/*",(packet) => {
            Debug.Log("CallBack: " + packet.ToString());
            if(uiText != null)
                uiText.text += "\n" + packet.ToString();
        });
        //Register a path with Regex comparation
        bool callback = listener.AddAddress(addressPath,(packet) => {
            Debug.Log("Regex Address Path: " + packet.ToString());
        });
        //Alert if address patch cannot be registered
        if(!callback)
            Debug.LogError($"Malformed address path: '{addressPath}'");
    }

    private void Update() {
        //Every Frame read all messages in queue
        //Remove every message from threaded queue and evaluate addresses callbacks
        //Note: older message will be discarded if queue count exeed queue limit (1000)
        listener.ReadAllQueuedMessages();
    }

    //On Disable Object must DISPOSE READER!
    private void OnDisable() {
        StopAllCoroutines();
        listener.Dispose();
    }
}
