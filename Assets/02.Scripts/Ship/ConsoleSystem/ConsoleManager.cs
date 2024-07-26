using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;

[System.Serializable]
public class Command
{
    public string name;
    public string description;
    public string actionname;
}

[System.Serializable]
public class CommandList
{
    public List<Command> commands;
}

public class ConsoleManager : MonoSingleton<ConsoleManager>
{
    private static readonly string COMMAND_FILE_PATH = Path.Combine(Application.streamingAssetsPath, "Data", "Command.json");

    public CommandList commandList;

    [Header("프리팹")]
    public GameObject outputPrefab;
    public GameObject inputPrefab;

    [Header("타겟")]
    public Transform monitor;
    public GameObject Spacer;
    public InputField inputField;
    private Text output;

    private Dictionary<string, System.Action<string[]>> commands;

    private void Awake()
    {
        LoadCommandList();
        SetupInitialConsole();
        RegisterCommands();
        LoadAndDisplayStartScreen();
    }

    private void LoadCommandList()
    {
        if (File.Exists(COMMAND_FILE_PATH))
        {
            string jsonContent = File.ReadAllText(COMMAND_FILE_PATH, System.Text.Encoding.UTF8);
            commandList = JsonUtility.FromJson<CommandList>(jsonContent);
        }
        else
        {
            Debug.LogError("Command.json file not found at path: " + COMMAND_FILE_PATH);
            commandList = new CommandList();
        }
    }

    private void LoadAndDisplayStartScreen()
    {
        if (commandList != null && commandList.commands != null)
        {
            Command startCommand = commandList.commands.Find(cmd => cmd.name == "consolestart");
            if (startCommand != null)
            {
                PrintToConsole(startCommand.description);
            }
            else
            {
                PrintToConsole("Welcome to the console.");
            }
        }
        else
        {
            PrintToConsole("Welcome to the console.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Enter(inputField.text);
        }
    }

    private void SetupInitialConsole()
    {
        if (Spacer == null)
        {
            Spacer = new GameObject("Spacer");
            Spacer.transform.SetParent(monitor, false);
            // Spacer에 필요한 컴포넌트 추가 (예: LayoutElement)
        }
        ClearMonitor("");
    }

    public void ClearMonitor(string message)
    {
        for (int i = monitor.childCount - 1; i >= 0; i--)
        {
            Transform child = monitor.GetChild(i);
            if (child.gameObject != Spacer)
            {
                Destroy(child.gameObject);
            }
        }

        Debug.Log(message);
        GameObject outIns = Instantiate(outputPrefab, monitor);
        outIns.transform.SetSiblingIndex(Spacer.transform.GetSiblingIndex() + 1);
        output = outIns.GetComponent<Text>();
        output.text = message;

        GameObject inIns = Instantiate(inputPrefab, monitor);
        inputField = inIns.GetComponent<InputField>();
        inputField.Select();
    }

    public void Enter(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            ProcessCommand(message);
            inputField.text = "";
        }
    }

    private void RegisterCommands()
    {
        commands = new Dictionary<string, System.Action<string[]>>();
    }

    private void ProcessCommand(string message)
    {
        string[] splitMessage = message.Split(' ');
        string command = splitMessage[0].ToLower();
        string[] args = new string[splitMessage.Length - 1];
        System.Array.Copy(splitMessage, 1, args, 0, args.Length);

        if (command == "store")
        {
            StoreManager storeManager = FindObjectOfType<StoreManager>();
            if (storeManager != null)
            {
                PrintToConsole(storeManager.GetFormattedItemList());
            }
            else
            {
                PrintToConsole("Store is currently unavailable.");
            }
        }
        else if (commands.ContainsKey(command))
        {
            commands[command](args);
        }
        else
        {
            PrintToConsole("Unknown command: " + command);
        }
    }

    public string GetCommandDescription(string commandName)
    {
        if (commandList == null || commandList.commands == null)
        {
            return "";
        }
        Command command = commandList.commands.Find(cmd => cmd.name == commandName);
        return command != null ? command.description : "";
    }

    public void PrintToConsole(string message)
    {
        ClearMonitor(message);
    }
}