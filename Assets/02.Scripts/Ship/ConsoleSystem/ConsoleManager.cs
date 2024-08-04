using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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
public enum ConsoleState
{
    Normal,
    AwaitingConfirmation
}


public class ConsoleManager : MonoBehaviour
{
    //싱글톤
    public static ConsoleManager instance;

    private PlanetData currentTargetPlanet;
    private ItemData currentTargetItem;

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
    //스크롤
    
    //현재 Output
    private Text output;



    private ConsoleState currentState = ConsoleState.Normal;
    private Action<string> confirmationCallback;

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
    public void OnEnable()
    {
        currentState = ConsoleState.Normal;
        LoadAndDisplayStartScreen();
    }
    private void OnDisable()
    {
        RemoveInputFieldFocus();
        StopAllCoroutines();
    }
    private void SetupInputFieldFocus()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            inputField.ActivateInputField();
        }
    }

    private void RemoveInputFieldFocus()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
        }
    }
    private void OnInputFieldEndEdit(string value)
    {
        
        // Enter 키를 눌렀을 때의 동작
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Enter(value);
            inputField.text = "";
        }

        // 포커스를 다시 설정
        StartCoroutine(RefocusInputField());
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
    private void FixedUpdate()
    {
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollValue) > 0.01f)
        {
            ScrollRect scrollRect = monitor.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollValue);
            }
        }
    }
    private IEnumerator RefocusInputField()
    {
        // 한 프레임 대기 (즉시 실행하면 포커스가 제대로 잡히지 않을 수 있음)
        yield return null;
        inputField.ActivateInputField();
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
    private void ResetState()
    {
        currentState = ConsoleState.Normal;
        confirmationCallback = null;
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

        if (currentState == ConsoleState.AwaitingConfirmation)
        {
            confirmationCallback?.Invoke(message);
            currentState = ConsoleState.Normal;
            confirmationCallback = null;
            return;
        }
        //첫번재 명령어와 나머지 명령어 분리
        string[] splitMessage = message.Split(new char[] { ' ' }, 2);
        //첫번째 명령어 지정
        string command = splitMessage[0].ToLower();
        //나머지 명령어
        string subCommand = splitMessage.Length > 1 ? splitMessage[1] : null;
/*
        // 명령어와 유사도 비교
        foreach (var cmd in commands)
        {
            double similarity = CalculateSimilarity(command, cmd.name.ToLower());
            Debug.Log($"Command: {cmd.name}, Similarity: {similarity}");
        }
*/


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
        if(subCommand != null)
        {
            if (commands.Any(cmd => CalculateSimilarity(subCommand, cmd.name.ToLower()) < 0.99 && commands.Any(cmd => CalculateSimilarity(subCommand, cmd.name.ToLower()) > 0.5)))
            {
                PrintToConsole("이 작업과 함께 제공된 개체가 없거나, 단어가 잘못 입력되었거나 존재하지 않습니다.");
            }

        }


        //Command 사전 처리(1차)
        switch (command)
        {
            case "help":
                PrintToConsole(">MOONS\n항로를 지정할 위성 목록을 봅니다.\n\n>STORE\n회사 상점의 유용한 아이템 목록을 봅니다.");

                break;
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
                    PrintToConsole(storeManager.GetFormattedList());
                }
                else
                {
                    PrintToConsole("Store is currently unavailable.");
                }
                break;
            case "info":
                GetInfo(subCommand);
                break;
            case "buy":
                Transaction(subCommand);
                break;
            case "route":
                Transport(subCommand);
                break;
            default:
                // TODO: command중에 같은 이름 있는지 찾아보고 
                // 있으면 타입 찾고처리
                GetType(message);
                break;

        }

    }
    public void GetType(string commandName)
    {

        CommandData targetCommand = commands.Find(cmd => cmd.name == commandName);
        Debug.Log(targetCommand == null);
        switch (targetCommand?.type)
        {
            case "item":
                Transaction(commandName);
                break;
            case "planet":
                Transport(commandName);
                break;
            default:
                PrintToConsole("이 단어에 제공되는 작업이 없습니다.");
                break;
        }
    }

    public void Transaction(string itemName)
    {

        StoreManager storeManager = FindObjectOfType<StoreManager>();
        if (storeManager == null)
        {
            PrintToConsole("Store Manager not found.");
            return;
        }

        ItemData targetItem = storeManager.FindItemByName(itemName);
        if (targetItem == null)
        {
            PrintToConsole($"Item '{itemName}' not found.");
            return;
        }

        currentTargetItem = targetItem;

        currentState = ConsoleState.AwaitingConfirmation;
        confirmationCallback = TransactionConfirmation;

        PrintToConsole($"{targetItem.name}을 주문하려고 합니다.", true);
        PrintToConsole($"아이템의 총 가격: ${targetItem.price}.", false);
        PrintToConsole("CONFIRM 또는 DENY을(를) 입력하세요.", false);

    }

    private void TransactionConfirmation(string input)
    {
        if (IsInputSimilar(input, "confirm", 0.2f))
        {
            // 여기서 실제 구매 로직을 구현합니다.
            // 예를 들어, 플레이어의 크레딧을 확인하고 차감하는 등의 로직을 추가할 수 있습니다.
            StoreManager storeManager = FindObjectOfType<StoreManager>();
            if (storeManager != null)
            {
                storeManager.AddItemToRocketDelivery(currentTargetItem);
                PrintToConsole($"1개의 {currentTargetItem.name}을 주문했습니다. 당신의 현재 소지금은 알수 없음입니다.\n");
                PrintToConsole($"우리의 계약자는 작업 중에도 빠른 무료 배송 해택을 누릴 수 있습니다! 구매한 모든 상품은 1시간마다 대략적인 위치에 도착합니다.", false);
            }
            else
            {
                PrintToConsole("Error: Store Manager not found.");
            }
        }
        else if (IsInputSimilar(input, "deny", 0.2))
        {
            PrintToConsole(input, false);
            PrintToConsole("주문을 취소했습니다.", false);
        }
        else
        {
            PrintToConsole("Invalid input. Purchase cancelled.");
        }
        ResetState();

    }

    public void Transport(string planetName)
    {
        if (string.IsNullOrEmpty(planetName))
        {
            PrintToConsole("Please specify a planet name.");
            return;
        }

        PlanetManager planetManager = FindObjectOfType<PlanetManager>();
        if (planetManager == null)
        {
            PrintToConsole("Planet Manager not found.");
            return;
        }

        PlanetData targetPlanet = planetManager.FindPlanetByName(planetName);
        if (targetPlanet == null)
        {
            PrintToConsole($"Planet '{planetName}' not found.");
            return;
        }
/*
        if (targetPlanet.asset == null)
        {
            PrintToConsole($"Scene asset for planet '{planetName}' is not assigned.");
            return;
        }*/

        currentTargetPlanet = targetPlanet;

        currentState = ConsoleState.AwaitingConfirmation;
        confirmationCallback = TransportConfirmation;

        PrintToConsole($"{targetPlanet.name}의 이동 비용은 ${targetPlanet.price}입니다.", true);
        PrintToConsole($"이 위성의 현재 날씨는 알수 없음 입니다.", false);
        PrintToConsole("CONFIRM 또는 DENY을(를) 입력하세요.", false);

        // 입력 필드의 onEndEdit 이벤트에 TransportConfirmation 함수를 연결
    }
    private void TransportConfirmation(string input)
    {
        if (IsInputSimilar(input, "confirm", 0.15))
        {
            // 여기서 실제 이동 로직을 구현합니다.
            // 예를 들어, 플레이어의 크레딧을 확인하고 차감하는 등의 로직을 추가할 수 있습니다.
            PrintToConsole($"Routing autopilot to the {currentTargetPlanet.name}.");
            PrintToConsole($"Please enjoy your flight", false);
            // SceneManagement.LoadScene(currentTargetPlanet.asset.name);
            string name = currentTargetPlanet.assetname;
            SceneLoadManager.instance.LoadSceneByName(name);
        }
        else if (IsInputSimilar(input, "deny", 0.15))
        {
            PrintToConsole(input, false);
            PrintToConsole("주문을 취소했습니다.", false);
        }
        ResetState();
        // 입력 필드의 이벤트 리스너를 원래대로 되돌립니다.
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

            //Debug.Log("모니터 출력 :" + message);
            //내보낼 인스턴스
            GameObject outIns = Instantiate(outputPrefab, monitor);
            outIns.transform.SetSiblingIndex(Spacer.transform.GetSiblingIndex() + 1);
            output = outIns.GetComponent<Text>();
            output.text = message;
        }
        else
        {
            DestroyImmediate(inputField.gameObject);
            output.text += "\n";
            output.text += message;
            output.text += "\n";

        }
        // 새 InputField 생성 및 위치 지정
        GameObject inIns = Instantiate(inputPrefab, monitor);
        inputField = inIns.GetComponent<InputField>();

        // InputField를 항상 마지막 자식으로 설정
        inputField.transform.SetAsLastSibling();

        // Canvas 업데이트를 강제로 실행하여 레이아웃 즉시 조정
        Canvas.ForceUpdateCanvases();

        // InputField 활성화 및 포커스
        inputField.Select();
        inputField.ActivateInputField();
        SetupInputFieldFocus();

        ScrollRect scrollRect = monitor.GetComponentInParent<ScrollRect>();

        RectTransform rect = scrollRect.GetComponent<Transform>() as RectTransform;
        if (scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollRect.transform);
            rect.ForceUpdateRectTransforms();
            scrollRect.verticalNormalizedPosition = 0f; // 스크롤뷰를 가장 아래로 설정합니다.
            Canvas.ForceUpdateCanvases(); // 이 줄을 추가하여 레이아웃을 강제로 업데이트합니다.
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
    private bool IsInputSimilar(string input, string target, double threshold)
    {
        return CalculateSimilarity(input.ToLower(), target.ToLower()) >= threshold;
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
