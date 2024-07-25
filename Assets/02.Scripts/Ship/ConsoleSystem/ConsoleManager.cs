using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleManager : MonoSingleton<ConsoleManager>
{
    [Header("프리팹")]
    public GameObject outputPrefab;
    public GameObject inputPrefab;

    [Header("타겟")]
    public Transform monitor;
    public InputField inputField;
    private Text output;

    private Dictionary<string, System.Action<string[]>> commands;

    private void Awake()
    {
        SetupInitialConsole();
        RegisterCommands();
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
        ClearMonitor("");
    }

    public void ClearMonitor(string message)
    {
        for (int i = monitor.childCount - 1; i >= 0; i--)
        {
            Destroy(monitor.GetChild(i).gameObject);
        }

        GameObject outIns = Instantiate(outputPrefab, monitor);
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
        commands.Add("shop", OpenShop);
        commands.Add("goto", GoToDestination);
    }

    private void ProcessCommand(string message)
    {
        string[] splitMessage = message.Split(' ');
        string command = splitMessage[0];
        string[] args = new string[splitMessage.Length - 1];
        System.Array.Copy(splitMessage, 1, args, 0, args.Length);

        if (commands.ContainsKey(command))
        {
            commands[command](args);
        }
        else
        {
            PrintToConsole("Unknown command: " + command);
        }
    }

    private void OpenShop(string[] args)
    {
        // 상점 기능 구현
        PrintToConsole("Welcome to the shop!");
        // 추가 상점 로직
    }

    private void GoToDestination(string[] args)
    {
        if (args.Length > 0)
        {
            string destination = args[0];
            PrintToConsole("Going to " + destination);
            // 목적지 이동 로직
        }
        else
        {
            PrintToConsole("Please specify a destination.");
        }
    }

    public void PrintToConsole(string message)
    {
        ClearMonitor(message);
    }
}
