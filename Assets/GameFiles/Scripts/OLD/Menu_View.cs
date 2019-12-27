/*
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

class Menu_View : MonoBehaviour
{
    enum Tab
    {
        MAIN,
        SESSION_CREATION,
        PASSWORD,
        WAITING,
        CONNECT_REFUSED
    }

    // Controller.
    Menu_Controller controller = new Menu_Controller();

    // GUI fields.
    Tab currentTab = Tab.MAIN;
    Vector2 scrollPosition = Vector2.zero;
    int selectedSession = 0;
    string hostName = "";
    string serverAnswer = "";
    string password = "";
    int numberOfPlayers = 0;

    // Constants.
    const int TEXT_LABEL_WIDTH = 100;
    const int NUMBER_LABEL_WIDTH = 20;
    const int BUTTON_HEIGHT = 50;

    // Monobehaviour inherited.
    void Start()
    {
        controller.OnServerAnswer.AddListener(UpdateServerAnswer);
    }
    void OnGUI()
    {
        switch (currentTab)
        {
            case Tab.MAIN:
                {
                    DrawSessionListWindow();
                }
                break;
            case Tab.SESSION_CREATION:
                {
                    DrawSessionCreationWindow();
                }
                break;
            case Tab.PASSWORD:
                {
                    DrawPasswordFieldWindow();
                }
                break;
            case Tab.WAITING:
                {
                    DrawWaitingForResponse();
                }
                break;
            case Tab.CONNECT_REFUSED:
                {
                    DrawConnectRefused();
                }
                break;
        }
    }

    // Private methods.
    void DrawSessionListWindow()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, "Box");
        {
            GUILayout.BeginHorizontal("Box");
            {
                GUILayout.Label("Session name", GUILayout.Width(TEXT_LABEL_WIDTH));
                GUILayout.Label("Players", GUILayout.Width(NUMBER_LABEL_WIDTH));
                GUILayout.Label("Password protected", GUILayout.Width(TEXT_LABEL_WIDTH));
            }
            GUILayout.EndHorizontal();

            foreach (var session in controller.Sessions)
            {
                GUILayout.BeginHorizontal("Box");
                {
                    selectedSession = GUILayout.SelectionGrid(selectedSession, new string[0], 1, GUILayout.Width(2 * TEXT_LABEL_WIDTH + NUMBER_LABEL_WIDTH));
                    GUILayout.Label(session.HostName, GUILayout.Width(TEXT_LABEL_WIDTH));
                    GUILayout.Label(session.ConnectionsCurrent.ToString() + "/" + session.ConnectionsMax, GUILayout.Width(NUMBER_LABEL_WIDTH));
                    GUILayout.Label((session.HostName.Contains("pw_") ? "YES" : "NO"), GUILayout.Width(TEXT_LABEL_WIDTH));
                }
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal("Box");
        {
            if (GUILayout.Button("Join selected", GUILayout.Height(BUTTON_HEIGHT)))
            {
                if (// not pw protected)
                {
                    currentTab = Tab.PASSWORD;
                }
                else
                {
                    controller.RequestConnect(selectedSession);
                }
            }
            if (GUILayout.Button("Create new session", GUILayout.Height(BUTTON_HEIGHT)))
            {
                currentTab = Tab.SESSION_CREATION;
            }
        }
        GUILayout.EndHorizontal();
    }
    void DrawSessionCreationWindow()
    {
        GUILayout.BeginHorizontal("Box");
        {
            GUILayout.Label("Session's name: ", GUILayout.Width(TEXT_LABEL_WIDTH));
            hostName = GUILayout.TextField(hostName, GUILayout.Width(TEXT_LABEL_WIDTH));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("Box");
        {
            GUILayout.Label("Password: ", GUILayout.Width(TEXT_LABEL_WIDTH));
            password = GUILayout.PasswordField(password, '*', GUILayout.Width(TEXT_LABEL_WIDTH));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("Box");
        {
            GUILayout.Label("Max players: ", GUILayout.Width(TEXT_LABEL_WIDTH));
            numberOfPlayers = Mathf.Clamp(EditorGUILayout.IntField(numberOfPlayers, GUILayout.Width(NUMBER_LABEL_WIDTH)), 1, 20);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Create session") || Input.GetKeyDown(KeyCode.Return))
        {
            currentTab = Tab.WAITING;
            controller.CreateSession(hostName, numberOfPlayers, password);
        }
    }
    void DrawPasswordFieldWindow()
    {
        GUILayout.BeginHorizontal("Box");
        {
            GUILayout.Label("Password: ", GUILayout.Width(TEXT_LABEL_WIDTH));
            password = GUILayout.PasswordField(password, '*', GUILayout.Width(TEXT_LABEL_WIDTH));
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Submit", GUILayout.Height(BUTTON_HEIGHT)) || Input.GetKeyDown(KeyCode.Return))
        {
            controller.RequestConnect(selectedSession, password.GetHashCode());
        }
    }
    void DrawWaitingForResponse()
    {
        GUILayout.Label("Waiting for server's answer...");
    }
    void DrawConnectRefused()
    {
        GUILayout.Label("Connection attempt refused. Reason:");
        GUILayout.Label(serverAnswer);

        if (GUILayout.Button("OK", GUILayout.Height(BUTTON_HEIGHT)) || Input.GetKeyDown(KeyCode.Return))
        {
            currentTab = Tab.MAIN;
        }
    }
    void UpdateServerAnswer(KeyValuePair<bool, string> answer)
    {
        if (!answer.Key)
        {
            serverAnswer = answer.Value;
            currentTab = Tab.CONNECT_REFUSED;
        }
        else
        {
            serverAnswer = "";
        }
    }
}
*/