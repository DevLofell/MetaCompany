using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CommandData
{
    public string name;
    public string description;
    public string type;
    public string assetname;
    public object asset;
}



[System.Serializable]
public class BaseCommand
{
    public List<CommandData> basecommands;
}

public enum ConsoleMode
{
    Start,
    Help,
    Info,
    Proceed
}
public enum ErrorType
{
    Find,
    Simular,
    none
}

public class ConsoleManager : MonoBehaviour
{
    //싱글톤
    public static ConsoleManager instance;  


    //콘솔 시스템을 만들고 싶다.
    //1. json에서 데이터를 로드 한다.
    private static readonly string COMMAND_FILE_PATH = Path.Combine(Application.streamingAssetsPath, "Data", "Command.json");
    public BaseCommand baseCommand;
    //리스트 불러오기 위한 커멘드
    public List<SubCommand> subCommands;

    public List<CommandData> commands;

    public Coroutine loadDataCoroutine;

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



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    
        
        
    }
    public IEnumerator Start()
    {
        if (File.Exists(COMMAND_FILE_PATH))
        {
            string jsonContent = File.ReadAllText(COMMAND_FILE_PATH, System.Text.Encoding.UTF8);
            baseCommand = JsonUtility.FromJson<BaseCommand>(jsonContent);
            yield return new WaitUntil(() => (baseCommand != null && baseCommand.basecommands != null));
            commands.AddRange(baseCommand.basecommands); // baseCommand의 모든 항목을 commands 리스트에 추가

            foreach (SubCommand subCommand in subCommands)
            {
                commands.AddRange(subCommand.LoadDataList());
            }


        }
        else
        {
            Debug.LogError("Command.json file not found at path: " + COMMAND_FILE_PATH);
            baseCommand = new BaseCommand();
        }
        yield return new WaitUntil(() => (commands != null));
        LoadAndDisplayStartScreen();

    }

    
    /// <summary>
    /// 커멘드 화면에 보이기
    /// </summary>
    private void LoadAndDisplayStartScreen()
    {
     
        if (commands != null)
        {
            // 커멘드 중에 콘솔 시작 커멘드 찾기
            CommandData startCommand = commands.Find(cmd => cmd.name == "consolestart");
            if (startCommand != null)
            {
                DateTime today = DateTime.Now;
                string dayofWeek = today.DayOfWeek.ToString();
                string print = string.Format(startCommand.description, ConvertDayOfWeekToKorean(dayofWeek));
                PrintToConsole(print);
            }
        }
    }

    //2. 서브 시스템 등록된 만큼 대기한다.
    //3. 서브 시스템이 등록되면 command로 등록한다.
    //4. 개별 command의 에셋을 찾아 등록한다.

    public string GetCommandDescription(string commandName)
    {
        if (commands == null)
        {
            return "";
        }
        CommandData command = commands.Find(cmd => cmd.name == commandName);
        return command != null ? command.description : "";
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
        if (commands == null)
        {
            PrintToConsole("Json 로드 이상");
            return;
        }
        //첫번재 명령어와 나머지 명령어 분리
        string[] splitMessage = message.Split(new char[] { ' ' }, 2);
        //첫번째 명령어 지정
        string command = splitMessage[0].ToLower();
        //나머지 명령어
        string subCommand = splitMessage.Length > 1 ? splitMessage[1] : null;

        // 명령어와 유사도 비교
        foreach (var cmd in commands)
        {
            double similarity = CalculateSimilarity(command, cmd.name.ToLower());
            Debug.Log($"Command: {cmd.name}, Similarity: {similarity}");
        }



        // 유사도 검색 로직 
        var similarCommands = commands
      .Select(cmd => new { Command = cmd, Similarity = CalculateSimilarity(command, cmd.name.ToLower()) })
      .Where(x => x.Similarity <= 0.8f) // 50% 이상 유사한 명령어만 선택
      .OrderByDescending(x => x.Similarity)
      .ToList();


        if (commands.Any(cmd => CalculateSimilarity(command, cmd.name.ToLower()) < 0.99 && commands.Any(cmd => CalculateSimilarity(command, cmd.name.ToLower()) > 0.5)))
        {
            PrintToConsole("이 작업과 함께 제공된 개체가 없거나, 단어가 잘못 입력되었거나 존재하지 않습니다.");
        }
        else
        {
            PrintToConsole("이 단어에 제공되는 작업이 없습니다.");
        }


        //Command 사전 처리(1차)
        switch (command)
        {
            //moons 출력
            case "moons":
                PlanetManager planetManager = FindObjectOfType<PlanetManager>();
                if (planetManager != null)
                {
                    PrintToConsole(planetManager.GetFormattedList());
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
                    //PrintToConsole(storeManager.GetFormattedItemList());
                }
                else
                {
                    PrintToConsole("Store is currently unavailable.");
                }
                break;
            case "buy":
                Transaction(subCommand);
                break;
            case "info":
                GetInfo(subCommand);
                break;
            case "route":
                Transport(subCommand);
                break;
            default:
                // TODO: command중에 같은 이름 있는지 찾아보고 
                // 있으면 타입 찾고처리
                GetType(subCommand);
                break;

        }
    }

    public void Transaction(string args)
    {

    }
    public void Transport(string args)
    {

    }
    //완
    public void GetInfo(string args)
    {

        if (commands != null )
        {
            //TODO : 바꿔야됨
            // 커멘드 중에 콘솔 시작 커멘드 찾기
            CommandData startCommand = commands.Find(cmd => cmd.name == args);
            if (startCommand != null)
            {
                PrintToConsole(startCommand.description);
            }
        }
    }
    /// <summary>
    /// 모니터 초기화
    /// </summary>
    /// <param name="message">초기화 할 내용</param>
    public void PrintToConsole(string message, bool isClear = true)
    {
        if (isClear)
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
        else
        {
            DestroyImmediate(inputField.gameObject);
            output.text += "\n";
            output.text += message;
            output.text += "\n";
            GameObject inIns = Instantiate(inputPrefab, monitor);
            inputField = inIns.GetComponent<InputField>();
            inputField.Select();
            inputField.ActivateInputField();

        }

    }

    public void GetType(string commandName)
    {
        bool isFind = false;
        CommandData targetCommand = commands.Find(cmd => cmd.name == commandName);
        switch (targetCommand.type)
        {
            
            case "item":
                Transaction(commandName);
                break;
            case "planet":
                Transport(commandName);
                break;
        }
    }

    //유사도 검색 알고리즘
    //Levenshtein distance 알고리즘
    private static double CalculateSimilarity(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return 1.0 - ((double)d[n, m] / Mathf.Max(n, m));
    }

    static string ConvertDayOfWeekToKorean(string dayOfWeek)
    {
        switch (dayOfWeek.ToLower())
        {
            case "monday":
                return "월요일";
            case "tuesday":
                return "화요일";
            case "wednesday":
                return "수요일";
            case "thursday":
                return "목요일";
            case "friday":
                return "금요일";
            case "saturday":
                return "토요일";
            case "sunday":
                return "일요일";
            default:
                return "알 수 없는 요일";
        }
    }
}
