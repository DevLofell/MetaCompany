using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class Planet
{
    public string name;
    public string description;
    public int price;
    public string type;
    public string asset;
}
[System.Serializable]
public class PlanetList
{
    public List<Planet> companyplanets;
    public List<Planet> easeplanets;
    public List<Planet> normalplanets;
    public List<Planet> hardplanets;
}
public class PlanetManager : MonoBehaviour
{
    public PlanetList planetList;

    // Start is called before the first frame update
    void Start()
    {
        LoadPlanetList();
    }

    private void LoadPlanetList()
    {
    
    }
    
    public string GetFormattedList()
    {
        
        string companyplanetsStr = FormatItems(planetList.companyplanets);
        string reaseplanetsStr = FormatItems(planetList.easeplanets);
        string normalplanetsStr = FormatItems(planetList.normalplanets);
        string hardplanetsStr = FormatItems(planetList.hardplanets);

        //이거 {0},{1}등등 사이에 바로 넣는듯?
        return string.Format(ConsoleManager.instance.GetCommandDescription("moons"),
                             companyplanetsStr, reaseplanetsStr, normalplanetsStr, hardplanetsStr);
    }
    private string FormatItems(List<Planet> items)
    {
        string result = "";
        foreach (var item in items)
        {
            result += $"* {item.name}  //  가격: ${item.price}\n";
        }
        return result;
    }

    public Scene GetScene(string mapName)
    {
        return new Scene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal string GetFormattedItemList()
    {
        throw new NotImplementedException();
    }
}
