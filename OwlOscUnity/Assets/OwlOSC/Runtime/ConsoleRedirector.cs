using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleRedirector : MonoBehaviour
{
    public bool redirectConsole = true;
    private bool _redirect = true;

    private void Awake() {
        _redirect = redirectConsole;
        if(redirectConsole)
            OWL.UnitySystemConsoleRedirector.Redirect(_redirect);
    }

    private void Update() {
        if(redirectConsole != _redirect){
            _redirect = redirectConsole;
            OWL.UnitySystemConsoleRedirector.Redirect(_redirect);
        }
    }
}
