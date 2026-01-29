using System;
using System.Collections.Generic;
using System.IO;
using EasyButtons;
using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScene : MonoBehaviour
{
    public static HomeScene instance;


    public LevelDataForSave levelDataForSave;
    public LevelSceneData levelSceneData;

    public string GetCurrentLevelID => levelDataForSave.currentActiveLevel.levelID;

    private HashSet<string> _existingIds = new HashSet<string>();


    [Serializable]
    public class LevelScene
    {
        public string levelID;
        public string scene_name;
    }
    [Serializable]
    public class LevelSceneData
    {

        public List<LevelScene> levelScenes;
    }



    private string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName + ".json");


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Load();
        SyncLevelData(levelSceneData.levelScenes, levelDataForSave.levelDatas);
        if (levelDataForSave.currentActiveLevel == null)
        {
            Debug.Log("No Any Active Level, Start From Scratch");
            //TODO:- start from first level
            levelDataForSave.currentActiveLevel = levelDataForSave.levelDatas[0];
            StartGame();
        }
        else
        {
            levelDataForSave.currentActiveLevel = levelDataForSave.FindLevel(levelDataForSave.currentActiveLevel.levelID);
            Debug.Log($"Found Current Level - {levelDataForSave.currentActiveLevel.scene_name}");
            //TODO:- check is current Level complete or not
            var isLevelCompleted = levelDataForSave.currentActiveLevel.isLevelCompleted;
            //TODO:- if complete Get New Level And start
            if (isLevelCompleted)
            {
                Debug.Log($"Need To Get Next Level For Start");
            }
            //TODO:- if not Complete Than Load this level and continue
            else
            {
                Debug.Log($"Level is not completed - Start {levelDataForSave.currentActiveLevel.scene_name}");
            }
            StartGame();
        }
    }
    void OnDisable()
    {
        Save();
    }

    [Button]
    private void StartGame()
    {
        SceneManager.LoadScene(levelDataForSave.currentActiveLevel.scene_name);
    }
    [Button]
    private void MarkCurrentLevelComplete()
    {
        levelDataForSave.currentActiveLevel.isLevelCompleted = true;
    }
    [Button]
    private void LoadNextLevel()
    {
        if (!levelDataForSave.currentActiveLevel.isLevelCompleted)
        {
            Debug.Log($"Current Level Not Completed");
            return;
        }
        levelDataForSave.SetNextLevel();
    }




    public void SyncLevelData(List<LevelScene> scenes, List<LevelData> dataList)
    {
        if (scenes == null || dataList == null) return;

        // 1. Fill the HashSet with IDs currently in LevelData
        _existingIds.Clear();
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i] != null)
                _existingIds.Add(dataList[i].levelID);
        }

        // 2. Iterate through scenes and add missing ones
        for (int i = 0; i < scenes.Count; i++)
        {
            string sceneId = scenes[i].levelID;

            if (!_existingIds.Contains(sceneId))
            {
                // Only "new" is called when data is actually missing
                LevelData newData = new LevelData();
                newData.levelID = sceneId;
                newData.scene_name = scenes[i].scene_name;
                newData.isLevelCompleted = false;

                dataList.Add(newData);
                _existingIds.Add(sceneId); // Add to set so we don't add duplicates

                Debug.Log($"Added missing LevelData: {sceneId}");
            }
        }
    }
    public void MarkeAsDoneCurrentLevel()
    {
        levelDataForSave.currentActiveLevel.isLevelCompleted = true;
        LoadNextLevel();
    }




    private void Load()
    {
        string path = GetPath("level_data");
        if (!File.Exists(path))
        {
            Debug.LogError("Level Not Found");
            levelDataForSave = new();
            return;
        }
        string json = File.ReadAllText(path);
        levelDataForSave = JsonConvert.DeserializeObject<LevelDataForSave>(json);
    }
    private void Save()
    {
        string path = GetPath("level_data");
        string json = JsonConvert.SerializeObject(levelDataForSave);
        File.WriteAllText(path, json);
    }
}






[Serializable]
public class LevelDataForSave
{
    public List<LevelData> levelDatas = new();
    public LevelData currentActiveLevel;

    public void SetNextLevel()
    {
        var item = levelDatas.Find(i => !i.isLevelCompleted);
        if (item != null)
        {
            currentActiveLevel = item;
        }
        else
        {
            Debug.Log($"All Level Completed");
            Debug.Log($"Reseting All Level");
            //TODO:- Set False All level
            int count = levelDatas.Count;
            for (int i = 0; i < count; i++)
            {
                levelDatas[i].isLevelCompleted = false;
            }
            Debug.Log($"Reset Complete Assiging Level");
            //TODO:- Start from level 1
            currentActiveLevel = levelDatas[0];
        }
    }
    public LevelData FindLevel(string levelId)
    {
        return levelDatas.Find(i => i.levelID == levelId);
    }
}

[Serializable]
public class LevelData
{
    public string levelID;
    public bool isLevelCompleted;
    public string scene_name;
}
