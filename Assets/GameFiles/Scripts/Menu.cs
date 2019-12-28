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

    int screenWidth = 0;
    int screenHeight = 0;
    Vector2 screenCenter = Vector2.zero;
    const int WIDGET_WIDTH = 400;
    const int WIDGET_HEIGHT = 100;
    const int BUTTON_WIDTH = 200;
    const int BUTTON_HEIGHT = 50;

    // Monobehaviour inherited.
    private void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        screenCenter = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
    }
    void OnGUI()
    {
        switch (currentTab)
        {
            case Tab.MAIN:
                {
                    GUILayout.BeginHorizontal("Box");
                    {
                        if (GUI.Button(new Rect(screenCenter.x - BUTTON_WIDTH * 0.5f, screenCenter.y * 0.33f, BUTTON_WIDTH, BUTTON_HEIGHT), "Create New Session"))
                        {
                            currentTab = Tab.SESSION_CREATION;
                        }
                        if (GUI.Button(new Rect(screenCenter.x - BUTTON_WIDTH * 0.5f, screenCenter.y * 0.66f, BUTTON_WIDTH, BUTTON_HEIGHT), "Join Random"))
                        {
                            BoltLauncher.StartClient();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                break;
            case Tab.SESSION_CREATION:
                {
                    GUILayout.BeginVertical("Box");
                    {
                        GUI.Label(new Rect(screenCenter.x - WIDGET_WIDTH * 0.5f, screenCenter.y * 0.33f - WIDGET_HEIGHT * 0.5f, WIDGET_WIDTH, WIDGET_HEIGHT), "Session Name:");
                        hostName = GUI.TextField(new Rect(screenCenter.x - WIDGET_WIDTH * 0.5f, screenCenter.y * 0.33f + WIDGET_HEIGHT * 0.5f, WIDGET_WIDTH, WIDGET_HEIGHT), hostName);
                    }
                    GUILayout.EndVertical();

                    if (GUI.Button(new Rect(screenCenter.x - BUTTON_WIDTH * 0.5f, screenCenter.y * 1.33f, BUTTON_WIDTH, BUTTON_HEIGHT), "Create"))
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

    public override void BoltStartFailed()
    {
        Debug.LogError("Bolt start failed!");
    }
}
