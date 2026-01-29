using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class StorageBoggy : MonoBehaviour
{
    [Header("Stack Settings")]
    public Transform debriesContainer;
    public Vector3 spacing = new Vector3(0.0f, 0.0f, 0.0f); // Cube ki size ke hisaab se gap
    public StorageBoggyConfig storageBoggyConfig;
    public TextMeshProUGUI coinAmountText;
    private int collectedCount = 0; // Total kitne cubes aaye
    private IdleCurrency totalCoinCollected = 0;

    public List<Debries> collectedDebries;
    private StorageBoggyDataForSave storageBoggyDataForSave = new();


    private string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName + ".json");
    private static string storageBoggySaveKey = HomeScene.instance != null ? $"{HomeScene.instance.GetCurrentLevelID}_storage_boggy_collected_data" : "level_1_storage_boggy_collected_data";


    void Awake()
    {
        storageBoggyConfig = Instantiate(storageBoggyConfig);
        storageBoggyConfig.SetUp();
    }

    void Start()
    {
        LoadStorageBoggyData();
    }

    void OnDisable()
    {
        SaveStorageBoggyData();
    }
    private void LoadStorageBoggyData()
    {
        string path = GetPath(storageBoggySaveKey);
        if (!File.Exists(path))
        {
            Debug.LogError("Storage Boggy Data Not Found");
            return;
        }
        string json = File.ReadAllText(path);
        storageBoggyDataForSave = JsonConvert.DeserializeObject<StorageBoggyDataForSave>(json);
        collectedCount = storageBoggyDataForSave.totalDebris;
        totalCoinCollected = storageBoggyDataForSave.totalCoins;
        storageBoggyConfig.filledCapacity = storageBoggyDataForSave.totalCapacity;

        UpdateText();
    }
    private void SaveStorageBoggyData()
    {
        string json = JsonConvert.SerializeObject(storageBoggyDataForSave);
        File.WriteAllText(GetPath(storageBoggySaveKey), json);
    }

    public void SetUpDebrie(Debries debries)
    {
        collectedDebries.Add(debries);
        var tempDebrie = debries;
        tempDebrie.isCollected = true;
        collectedCount += 1;
        storageBoggyDataForSave.totalDebris = collectedCount;
        ADDDebriesInStorage(tempDebrie.debriCapacity);
        StartCoroutine(MoveCubeToStackLocal(tempDebrie.transform));
        totalCoinCollected += debries.GetCoinAmount();
        storageBoggyDataForSave.totalCoins = totalCoinCollected;
        UpdateText();
    }
    public void UpdateStorage()
    {
        storageBoggyConfig.UpdateCapacity();
    }
    public void UpdateText()
    {
        coinAmountText.text = $"{totalCoinCollected.ToShortString()}";
        float progress = (float)storageBoggyConfig.GetFilled();
        GameManager.instance.uIHandler.UpdateCapacityStorageProgress(progress, (int)storageBoggyConfig.filledCapacity, (int)storageBoggyConfig.GetCapacity);
    }


    public bool CheckIsStorageFull(int amountToStore)
    {
        return storageBoggyConfig.CheckIsStorageFull(amountToStore);
    }
    // public bool IsStorageFull()
    // {
    //     return capcity <= filled;
    // }
    public void ADDDebriesInStorage(int amountToStore)
    {
        storageBoggyConfig.filledCapacity += amountToStore;
        storageBoggyDataForSave.totalCapacity = storageBoggyConfig.filledCapacity;
    }
    public Vector3 CalculateStackPosition()
    {
        // 3x3 Grid Logic
        int x = (collectedCount - 1) % 3;           // Column (0, 1, 2)
        int z = ((collectedCount - 1) / 3) % 3;     // Row (0, 1, 2)
        int y = (collectedCount - 1) / 9;           // Height (Har 9 cubes ke baad upar)

        Vector3 localPos = new Vector3(x * spacing.x, y * spacing.y, z * spacing.z);
        // Local position calculation
        return debriesContainer.TransformPoint(localPos);
    }
    public List<Debries> TransferAndClearStorage()
    {
        GameManager.instance.AddCoinAndCheck(totalCoinCollected);


        List<Debries> temp = new(collectedDebries);
        collectedDebries.Clear();
        storageBoggyConfig.filledCapacity = 0;
        storageBoggyDataForSave.totalCapacity = 0;
        collectedCount = 0;
        storageBoggyDataForSave.totalDebris = collectedCount;
        totalCoinCollected = 0;
        storageBoggyDataForSave.totalCoins = totalCoinCollected;
        UpdateText();
        return temp;
    }

    IEnumerator MoveCubeToStackLocal(Transform cubeTransform)
    {
        int x = (collectedCount - 1) % 3;           // Column (0, 1, 2)
        int z = ((collectedCount - 1) / 3) % 3;     // Row (0, 1, 2)
        int y = (collectedCount - 1) / 9;

        Vector3 localTargetPos = new Vector3(x * spacing.x, y * spacing.y, z * spacing.z);

        // 2. IMPORTANT: Jump shuru hote hi Cube ko Storage ka child bana dein
        // Isse ab cube storage ke saath-saath move karega
        cubeTransform.SetParent(debriesContainer);

        Vector3 localStartPos = cubeTransform.localPosition; // Ab ye storage ke hisaab se local pos hai
        float timer = 0;

        while (timer < 1.0f)
        {
            if (cubeTransform == null) yield break;

            timer += Time.deltaTime * 2f;

            // 3. Local Space mein Lerp karein
            // Ab storage kitna bhi move ya rotate ho, cube uske andar hi move hoga
            Vector3 currentLocalPos = Vector3.Lerp(localStartPos, localTargetPos, timer);

            // Sin wave height (Y axis par jump effect)
            float height = Mathf.Sin(timer * Mathf.PI) * 2;
            currentLocalPos.y += height;

            cubeTransform.localPosition = currentLocalPos;

            yield return null;
        }

        // 4. Final local position fix karein
        if (cubeTransform != null)
        {
            cubeTransform.localPosition = localTargetPos;
            cubeTransform.localRotation = Quaternion.identity;
        }
    }

}
