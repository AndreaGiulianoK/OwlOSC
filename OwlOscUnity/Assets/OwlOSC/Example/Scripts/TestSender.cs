using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OwlOSC;

public class TestSender : MonoBehaviour
{
    public string remoteHost = "127.0.0.1";
    public int remotePort = 55555;
    public string prefix = "/test";
    [Range(0.01f,1f)]
    public float sendDelay = 0.5f;

    UDPSender sender;

    private void OnEnable() {
        sender = new UDPSender(remoteHost,remotePort);
        StartCoroutine(SendMessages());
    }

    IEnumerator SendMessages(){
        while(true){
            sender.Send(new OscMessage(prefix, Random.Range(0,float.MaxValue), "hello"));
            yield return new WaitForSeconds(sendDelay);
        }
    }

    private void OnDisable() {
        sender.Dispose();
    }
}
