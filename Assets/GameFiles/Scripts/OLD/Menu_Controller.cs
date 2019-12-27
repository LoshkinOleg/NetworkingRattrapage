/*
using System;
using System.Collections.Generic;
using UnityEngine;
using UdpKit;
using Bolt;
using Bolt.Photon;
using Bolt.Matchmaking;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Menu_Controller
{
    // Nested types.
    public class UnityEvent_ServerAnswer : UnityEvent<KeyValuePair<bool, string>> { };

    public UnityEvent_ServerAnswer OnServerAnswer => OnServerAnswer;
    UnityEvent_ServerAnswer onServerAnswer = new UnityEvent_ServerAnswer();

    public List<UdpSession> Sessions
    {
        get
        {
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                return MenuNetworkCallbacks.Instance.Sessions;
            }
            else
            {
                Debug.LogError("Trying to access network callbacks specific to the Menu scene for another scene!");
                return null;
            }
        }
    }

    // Public methods.
    public void RequestConnect(int selectedSession, int passwordHash = 0)
    {
        // Create connection token.
        var connectionToken = new ConnectionToken();
        connectionToken.passwordHash = passwordHash;
        connectionToken.id = UnityEngine.Random.Range(0, Int16.MaxValue);

        // Update MenuNetworkCallbacks for it to expect an answer for this request id.
        MenuNetworkCallbacks.Instance.ConnectionRequestId = connectionToken.id;

        // Attempt to connect.
        BoltMatchmaking.JoinSession(Sessions[selectedSession], connectionToken);
    }
    public void CreateSession(string hostName, string password = "")
    {
        if (BoltNetwork.IsServer)
        {
            // Create session definition token.
            var sessionDefinitionToken = new SessionDefinitionToken();
            sessionDefinitionToken.passwordHash = password == "" ? 0 : password.GetHashCode();

            // Attempt to create session.
            BoltMatchmaking.CreateSession(hostName, sessionDefinitionToken, "Main");
        }
    }
}
*/