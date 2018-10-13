using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class Console : MonoBehaviour {

    public static readonly string COMMAND_SAVE = "save";
    public static readonly string COMMAND_SAVE_MECH = "savemech";

    public static readonly string COMMAND_ADD_CORE = "addcore";
    public static readonly string COMMAND_ADD_LEGS = "addlegs";
    public static readonly string COMMAND_ADD_SOCKETABLE = "addsocketable";

    public bool Showing {
        get { return consoleWindow.gameObject.activeSelf; }
        set {

            consoleWindow.gameObject.SetActive(value);
            
        }
    }
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        WriteToConsole(message, stackTrace, type);
    }

    public void WriteToConsole(string message)
    {
        WriteToConsole(message, null, LogType.Log);
    }
    public void WriteToConsole(string message, string stackTrace, LogType type)
    {
        if (console != null)
        {
            console.text += (message == null ? "" : (message + "\r\n"))
                            + (stackTrace == null || type == LogType.Log ? "" : (stackTrace + "\r\n"));
        }
    }

    public Transform consoleWindow;
    public Transform inputText;
    public Transform consoleText;

    private TMP_InputField input;
    private TextMeshProUGUI console;

    public void Start()
    {
        input = inputText.GetComponent<TMP_InputField>();
        console = consoleText.GetComponent<TextMeshProUGUI>();
    }

    public Stack<string> usedCommands = new Stack<string>();

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Showing = !Showing;
        }else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            input.text =  usedCommands.Count > 0 ?  usedCommands.Pop() : input.text;
        }
        else if (Showing && Input.GetKeyDown(KeyCode.Return))
        {
            WriteToConsole("> " + input.text, "", LogType.Log);
            ProcessCommand();
        }
    }

    public bool ProcessCommand()
    {
        string[] parts = input.text.Split(null);
        usedCommands.Push(input.text);
        input.text = "";

        if (parts.Length > 1)
        {
            string command = parts[0].ToLower();

            try
            {
                if (command.Equals(COMMAND_ADD_CORE))
                {
                    return AddMechItemToInventory(Global.Resources[(CoreNames)Enum.Parse(typeof(CoreNames), parts[1].Trim())], parts[1].Trim());
                }
                else if (command.Equals(COMMAND_ADD_SOCKETABLE))
                {
                    return AddMechItemToInventory((MechItem)Global.Resources[(SocketableNames)Enum.Parse(typeof(SocketableNames), parts[1].Trim())], parts[1].Trim());
                }
                else if (command.Equals(COMMAND_ADD_LEGS))
                {
                    return AddMechItemToInventory(Global.Resources[(LegNames)Enum.Parse(typeof(LegNames), parts[1].Trim())], parts[1].Trim());
                }

                else if (command.Equals(COMMAND_SAVE_MECH))
                {
                    return SaveMech(parts[1].Trim());
                }

            }
            catch(ArgumentException ae)
            {
                WriteToConsole("Invalid parameter: " + parts[1].Trim()+" for command: "+ command);
                return false;
            }

        }
        WriteToConsole("Unknown command");
        return false;
    }

    public bool SaveMech(string name)
    {
        foreach(GameUnit gu in GameUnit.unitsByFaction[Global.FACTION_PLAYER])
        {
            //Debug.Log("Gameunit: " + gu.uniqueName);
            if (gu.isPlayer)
            {
                MechData data = ScriptableObject.CreateInstance<MechData>();

                if (data.Save(gu.mech, name))
                {
                    string json = JsonUtility.ToJson(data);
                    string filePath = Path.Combine(Application.streamingAssetsPath, name + ".json");
                    File.WriteAllText(filePath, json);
                    WriteToConsole("Saved mech as: " + name);
                    return true;
                }
                else
                {
                    WriteToConsole("No playermech found?");
                }

            }
        }
        WriteToConsole("Unable to save player-mech to: " + name);
        return false;
    }
    public bool AddMechItemToInventory(MechItem mi, string name)
    {
        if (mi != null)
        {
            if (Global.Inventory.PutIntoInventory(mi))
            {
                WriteToConsole("Added: " + name + " to inventory");
                return true;
            }
            else
            {
                WriteToConsole("Unable to add: " + name + " into inventory, not enough space");
            }
        }
        return false;
    }
}
