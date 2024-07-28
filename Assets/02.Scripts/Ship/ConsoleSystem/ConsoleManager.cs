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

/// <summary>
/// 실제 커멘드 리스트
/// </summary>
[System.Serializable]
public class CommandList
{
    public List<Command> commands;
}
/// <summary>a
/// 콘솔시스템 매니저
/// </summary>
public class ConsoleManager : MonoSingleton<ConsoleManager>
{
    //경로(READONLY)
    private static readonly string COMMAND_FILE_PATH = Path.Combine(Application.streamingAssetsPath, "Data", "Command.json");

    //Serialized 된 데이터
    public CommandList commandList;

    [Header("프리팹")]
    //출력(Text) 프리팹
    public GameObject outputPrefab;
    //입력(InputField) 프리팹
    public GameObject inputPrefab;

    [Header("타겟")]
    //보여질 대상
    public Transform monitor;
    //공간 띄울 대상
    public GameObject Spacer;
    //집중된 InputField
    public InputField inputField;
    //현재 Output
    private Text output;
    
    //Dictionary형 Command
/*
    private Dictionary<string, System.Action<string[]>> commands;*/

    private void Awake()
    {
        //Json Command 로드
        LoadCommandList();
        //화면 초기화
        LoadAndDisplayStartScreen();
    }

    /// <summary>
    /// Json Command 로드
    /// </summary>
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


    /// <summary>
    /// 커멘드 화면에 보이기
    /// </summary>
    private void LoadAndDisplayStartScreen()
    {
        if (commandList != null && commandList.commands != null)
        {
            // 커멘드 중에 콘솔 시작 커멘드 찾기
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
        //입력 대기
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Enter(inputField.text);
        }
    }
    /// <summary>
    /// 모니터 초기화
    /// </summary>
    /// <param name="message">초기화 할 내용</param>
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

        Debug.Log("모니터 출력 :" + message);
        //내보낼 인스턴스
        GameObject outIns = Instantiate(outputPrefab, monitor);
        outIns.transform.SetSiblingIndex(Spacer.transform.GetSiblingIndex() + 1);
        output = outIns.GetComponent<Text>();
        output.text = message;
        //입력할 인스턴스
        GameObject inIns = Instantiate(inputPrefab, monitor);
        inputField = inIns.GetComponent<InputField>();
        //선택
        inputField.Select();
        inputField.ActivateInputField();
    }

    /// <summary>
    /// 입력한 커멘드 분석
    /// </summary>
    /// <param name="message">전송할 InputField 입력값</param>
    public void Enter(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            ProcessCommand(message);
            inputField.text = "";
        }
    }


    /// <summary>
    /// 명령어 실제 처리
    /// </summary>
    /// <param name="message">전송할 InputField 입력값</param>
    private void ProcessCommand(string message)
    {
        //첫번재 명령어와 나머지 명령어 분리
        string[] splitMessage = message.Split(new char[] { ' ' }, 2);
        //첫번째 명령어 지정
        string command = splitMessage[0].ToLower();
        //나머지 명령어
        string[] args = splitMessage.Length > 1 ? new string[] { splitMessage[1] } : new string[0];

        //Command 사전 처리(1차)
        switch(command)
        {
            //moons 출력
            case "moons":
                PlanetManager planetManager = FindObjectOfType<PlanetManager>();
                if (planetManager != null)
                {
                    PrintToConsole(planetManager.GetFormattedItemList());
                }
                else
                {
                    PrintToConsole("Store is currently unavailable.");
                }
                break;
            case "store":
                StoreManager storeManager = FindObjectOfType<StoreManager>();
                if (storeManager != null)
                {
                    PrintToConsole(storeManager.GetFormattedItemList());
                }
                else
                {
                    PrintToConsole("Store is currently unavailable.");
                }
                break;
            case "buy":
                Transaction(args);
                break;
            case "info":
                GetInfo(args);
                break;
            case "route":
            default:
                // TODO: command중에 같은 이름 있는지 찾아보고 
                // 있으면 타입 찾고처리
                GetType(args);
                break;

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

    public void GetInfo(string[] args)
    {

        if (commandList != null && commandList.commands != null)
        {
            // 커멘드 중에 콘솔 시작 커멘드 찾기
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

    public void GetType(string[] args)
    {

        if (commandList != null && commandList.commands != null)
        {
            // 커멘드 중에 콘솔 시작 커멘드 찾기
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


    public void Transaction(string[] args)
    {

    }


    /*
    else if (commands.ContainsKey(command))
    {
        commands[command](args);
    }*
/*private void RegisterCommands()
{
    commands = new Dictionary<string, System.Action<string[]>>();

    foreach (var command in commandList.commands)
    {
        switch (command.actionname)
        {
            case "storelist":
                commands[command.name] = (args) => StoreList(args);
                break;
            case "none":
                // Do nothing for commands with no action
                break;
            // Add more cases for other action names as needed
            default:
                Debug.LogWarning($"Unknown action name: {command.actionname} for command: {command.name}");
                break;
        }
    }
}*/
}