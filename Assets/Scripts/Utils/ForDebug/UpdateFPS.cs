using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UpdateFPS : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private Queue<float> deltaTimeQueue = new Queue<float>();
    private const int Interval = 10;

    private int counter = 0;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        deltaTimeQueue.Enqueue(Time.deltaTime);

        if (deltaTimeQueue.Count > Interval)
        {
            deltaTimeQueue.Dequeue();
        }

        if (counter++ < Interval) return;

        // •½‹ÏFPS‚ðŒvŽZ
        float sum = 0f;
        foreach (var dt in deltaTimeQueue)
        {
            sum += dt;
        }

        float averageDeltaTime = sum / deltaTimeQueue.Count;
        float averageFPS = 1f / averageDeltaTime;

        // •\Ž¦
        textComponent.text = $"Update FPS : {averageFPS:F1}";

        counter = 0;
    }
}
