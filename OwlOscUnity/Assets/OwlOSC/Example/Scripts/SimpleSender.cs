using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OwlOSC;

public class SimpleSender : MonoBehaviour
{
    public string remoteHost = "127.0.0.1";
    public int remotePort = 55555;
    public string prefix = "/test";

    private void OnEnable() {
        Send();
    }

    [ContextMenu("Send Message")]
    public void Send(){
        using(var sender = new UDPSender(remoteHost,remotePort)){
            sender.Send(new OscMessage(prefix, Random.Range(0,float.MaxValue), "hello"));
        }
    }
}
