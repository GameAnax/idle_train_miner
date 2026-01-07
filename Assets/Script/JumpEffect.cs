using UnityEngine;
using System.Collections;
using System;

public class JumpEffect : MonoBehaviour
{
    public void StartJump(Vector3 start, Vector3 end, float height, float duration, Action Oncomplete)
    {
        StartCoroutine(JumpRoutine(start, end, height, duration, Oncomplete));
    }
    public void StartJump(Vector3 start, Vector3 end, float height, float duration)
    {
        StartCoroutine(JumpRoutine(start, end, height, duration, null));
    }

    IEnumerator JumpRoutine(Vector3 start, Vector3 end, float height, float duration, Action OnComplete = null)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // 1. Horizontal movement (Seedha A se B)
            Vector3 currentPos = Vector3.Lerp(start, end, percent);

            // 2. Vertical movement (Jump ka Arc/Curve)
            // Formula: height * sin(180 degrees * percent)
            // Jab percent 0.5 (beecha) hoga, tab height max hogi
            float yOffset = Mathf.Sin(percent * Mathf.PI) * height;

            currentPos.y += yOffset;
            transform.position = currentPos;

            yield return null;
        }

        // Exact end position par set karein aur destroy karein
        transform.position = end;
        OnComplete?.Invoke();
        // Destroy(gameObject, 0.5f);
    }
}