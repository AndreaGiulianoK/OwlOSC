using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OwlOSC;

public class SimpleListener : MonoBehaviour
{
    public int localPort = 55555;
    UDPListener listener;
    
    private void OnEnable() {
        //Instantiate simple listener
        listener = new UDPListener(localPort);
        //register address path callback
        listener.AddAddress("/*",(packet) => {
            Debug.Log(packet.ToString());
        });
    }

    private void Update() {
        //read every frame all messages in queue
        listener.ReadAllQueuedMessages();
    }

    private void OnDisable() {
        listener.Dispose();
    }
}
