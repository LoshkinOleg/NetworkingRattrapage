/*
using System;
using System.Collections.Generic;
using UnityEngine;
using UdpKit;
using Bolt;
using Bolt.Photon;
using Bolt.Matchmaking;

[BoltGlobalBehaviour]
public class GlobalNetworkCallbacks : GlobalEventListener
{
    // Public properties.
    public static GlobalNetworkCallbacks Instance => instance;
    static GlobalNetworkCallbacks instance = null;
    public UdpSession Session => session;
    UdpSession session = null;

    // Private fields.
    BoltEntity player = null;
    int passwordHash = 0;

    // Public methods.
    public bool CheckPassword(int passwordHash)
    {
        return this.passwordHash == passwordHash;
    }

    // Monobehaviour inherited.
    void Awake()
    {
        Debug.Assert(!instance);
        instance = this;
    }

    // Bolt inherited.
    public override void SceneLoadLocalDone(string scene)
    {
        if (scene == "Menu")
        {
            player = BoltNetwork.Instantiate(BoltPrefabs.Player);
            player.TakeControl();
        }
    }
}

[BoltGlobalBehaviour("Menu")]
public class MenuNetworkCallbacks : GlobalEventListener
{
    // Public properties.
    public static MenuNetworkCallbacks Instance => instance;
    static MenuNetworkCallbacks instance = null;
    public List<UdpSession> Sessions => sessions;
    List<UdpSession> sessions = new List<UdpSession>();
    public KeyValuePair<bool, string> ServerAnswer => serverAnswer;
    KeyValuePair<bool, string> serverAnswer = new KeyValuePair<bool, string>();
    public int ConnectionRequestId { get; set; }

    // Monobehaviour inherited.
    private void Awake()
    {
        Debug.Assert(!instance);
        instance = this;
    }

    // Role independent.
    public override void BoltStartBegin()
    {
        BoltNetwork.RegisterTokenClass<ConnectionToken>();
        // BoltNetwork.RegisterTokenClass<SessionDefinitionToken>();
    }
    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        sessions.Clear();
        foreach (var session in sessionList)
        {
            sessions.Add(session.Value);
        }
    }

    // Server code.
    public override void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token)
    {
        if (BoltNetwork.IsServer)
        {
            if (token is ConnectionToken)
            {
                var connectionToken = token as ConnectionToken;
                if (GlobalNetworkCallbacks.Instance.Session.ConnectionsCurrent + 1 <= GlobalNetworkCallbacks.Instance.Session.ConnectionsMax)
                {
                    BoltNetwork.Accept(endpoint);
                }
                else
                {
                    connectionToken.answer = "Too many players.";
                    BoltNetwork.Refuse(endpoint);
                }
                if (GlobalNetworkCallbacks.Instance.CheckPassword(connectionToken.passwordHash))
                {
                    BoltNetwork.Accept(endpoint);
                }
                else
                {
                    connectionToken.answer = "Wrong password.";
                    BoltNetwork.Refuse(endpoint);
                }
            }
        }
    }

    // Client code.
    public override void ConnectRefused(UdpEndPoint endpoint, IProtocolToken token)
    {
        if (BoltNetwork.IsClient)
        {
            if (token is ConnectionToken)
            {
                var connectionToken = token as ConnectionToken;
                if (connectionToken.id == ConnectionRequestId)
                {
                    serverAnswer = new KeyValuePair<bool, string>(false, connectionToken.answer);
                }
            }
        }
    }
}
*/