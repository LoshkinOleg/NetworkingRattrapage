using UnityEngine;
using Bolt;
using Bolt.Matchmaking;

class Menu : GlobalEventListener
{
    enum Tab
    {
        MAIN,
        SESSION_CREATION
    }

    // GUI fields.
    Tab currentTab = Tab.MAIN;
    string hostName = "";

    readonly Rect UPPER_MIDDLE_RECT = new Rect(Screen.width * 0.5f - 100, Screen.height * 0.33f, 200, 50);
    readonly Rect LOWER_MIDDLE_RECT = new Rect(Screen.width * 0.5f - 100, Screen.height * 0.66f, 200, 50);
    readonly Rect LOWER_RECT = new Rect(Screen.width * 0.5f - 100, Screen.height * 0.85f, 200, 50);

    // Monobehaviour inherited.
    void OnGUI()
    {
        switch (currentTab)
        {
            case Tab.MAIN:
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUI.Button(UPPER_MIDDLE_RECT, "Create New Session"))
                        {
                            currentTab = Tab.SESSION_CREATION;
                        }
                        if (GUI.Button(LOWER_MIDDLE_RECT, "Join Random"))
                        {
                            BoltLauncher.StartClient();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                break;
            case Tab.SESSION_CREATION:
                {
                    GUILayout.BeginVertical();
                    {
                        hostName = GUI.TextField(LOWER_MIDDLE_RECT, hostName);
                    }
                    GUILayout.EndVertical();

                    if (GUI.Button(LOWER_RECT, "Create"))
                    {
                        BoltLauncher.StartServer();
                    }
                }
                break;
        }
    }

    // Bolt inherited.
    public override void BoltStartDone()
    {
        if (BoltNetwork.IsServer)
        {
            BoltMatchmaking.CreateSession(hostName, null, "Main");
        }
        else
        {
            BoltMatchmaking.JoinRandomSession();
        }
    }
}
