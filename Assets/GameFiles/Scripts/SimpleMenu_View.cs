using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMenu_View : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height * 0.5f - 30, 100, 50), "Join"))
        {
            if (BoltNetwork.SessionList.Count < 1)
            {
                Debug.Log("No sessions. Create one.");
                foreach (var session in BoltNetwork.SessionList)
                {
                    Debug.Log(session.Value.HostName);
                }
            }
            else
            {
                BoltLauncher.StartClient();
            }
        }
        if (GUI.Button(new Rect(Screen.width * 0.5f, Screen.height * 0.5f + 30, 100, 50), "Host"))
        {
            BoltLauncher.StartServer();
        }
    }
}
