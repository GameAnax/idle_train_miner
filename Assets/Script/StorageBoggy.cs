using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageBoggy : MonoBehaviour
{
    public int capcity;
    public int filled;

    [Header("Stack Settings")]
    public Transform debriesContainer;
    public Vector3 spacing = new Vector3(0.0f, 0.0f, 0.0f); // Cube ki size ke hisaab se gap
    public StorageBoggyConfig storageBoggyConfig;
    private int collectedCount = 0; // Total kitne cubes aaye

    public List<Debries> collectedDebries;

    void Start()
    {
        storageBoggyConfig = Instantiate(storageBoggyConfig);
    }

    public void SetUpDebrie(Debries debries)
    {
        collectedDebries.Add(debries);
        var tempDebrie = debries;
        tempDebrie.isCollected = true;
        collectedCount += 1;
        ADDDebriesInStorage(tempDebrie.debriCapacity);
        StartCoroutine(MoveCubeToStackLocal(tempDebrie.transform));
    }
    public void UpdateStorage()
    {
        storageBoggyConfig.UpdateCapacity();
    }


    public bool CheckIsStorageFull(int amountToStore)
    {
        return storageBoggyConfig.CheckIsStorageFull(amountToStore);
    }
    public bool IsStorageFull()
    {
        return capcity <= filled;
    }
    public void ADDDebriesInStorage(int amountToStore)
    {
        filled += amountToStore;
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
        List<Debries> temp = new(collectedDebries);
        collectedDebries.Clear();
        filled = 0;
        collectedCount = 0;
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
        cubeTransform.localPosition = localTargetPos;
        cubeTransform.localRotation = Quaternion.identity;
    }

}
