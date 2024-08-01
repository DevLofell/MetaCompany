using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;


[System.Serializable]
public class PlanetData : CommandData
{
    public int price;  // JSON의 "price"와 일치하도록 변경
}

[System.Serializable]
public class PlanetDataList
{
    public List<PlanetData> companyplanets;
    public List<PlanetData> easeplanets;
    public List<PlanetData> normalplanets;
    public List<PlanetData> hardplanets;
}
public class PlanetManager : SubCommand
{
    private static readonly string PLANET_FILE_PATH = Path.Combine(Application.streamingAssetsPath, "Data", "PlanetList.json");

    public PlanetDataList planetDataList;

    public List<SceneAsset> assetlist;
    public List<SceneReference> referencelist;



    public override string GetFormattedList()
    {
        string companyplanetsStr = FormatItems(planetDataList.companyplanets);
        string reaseplanetsStr = FormatItems(planetDataList.easeplanets);
        string normalplanetsStr = FormatItems(planetDataList.normalplanets);
        string hardplanetsStr = FormatItems(planetDataList.hardplanets);

        //이거 {0},{1}등등 사이에 바로 넣는듯?
        return string.Format(ConsoleManager.instance.GetCommandDescription("moons"),
                             companyplanetsStr, reaseplanetsStr, normalplanetsStr, hardplanetsStr);
    }

    private string FormatItems(List<PlanetData> planets)
    {
        string result = "";
        foreach(var planet in planets)
        {
            result += $"* {planet.name}\n";
        }
        return result;
    }

    public override List<CommandData> LoadDataList()
    {
        List<CommandData> result = new List<CommandData>();
        if (File.Exists(PLANET_FILE_PATH))
        {
            string jsonContent = File.ReadAllText(PLANET_FILE_PATH, System.Text.Encoding.UTF8);
            planetDataList = JsonUtility.FromJson<PlanetDataList>(jsonContent);
            result.AddRange(planetDataList.companyplanets);
            result.AddRange(planetDataList.easeplanets);
            result.AddRange(planetDataList.normalplanets);
            result.AddRange(planetDataList.hardplanets);

            AssignSceneAssets(result); // 씬 에셋 할당
        }
        else
        {
            Debug.LogError("Command.json file not found at path: " + PLANET_FILE_PATH);
            planetDataList = new PlanetDataList();
            result = null;
        }
        
        return result;
    }

    private void AssignSceneAssets(List<CommandData> dataList)
    {
        foreach (var data in dataList)
        {
            var planetData = data as PlanetData;
            if (planetData != null)
            {
                foreach (var sceneAsset in assetlist)
                {
                    if (string.IsNullOrEmpty(planetData.assetname)) return;
                    if (sceneAsset.name.IndexOf(planetData.assetname, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        planetData.asset = sceneAsset;
                        break;
                    }
                }
            }
        }
    }

    public PlanetData FindPlanetByName(string name)
    {
        List<List<PlanetData>> allPlanets = new List<List<PlanetData>>
    {
        planetDataList.companyplanets,
        planetDataList.easeplanets,
        planetDataList.normalplanets,
        planetDataList.hardplanets
    };

        foreach (var planetList in allPlanets)
        {
            PlanetData planet = planetList.Find(p => p.name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (planet != null)
            {
                return planet;
            }
        }

        return null;
    }

}
