using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class CurriculumDebug : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Numero minimo di episodi per calcolare la media stabile.")]
    public int minLessonLength = 100;

    private List<float> rewardHistory = new List<float>();

    public static event Action<float> UpdateRewardHistory;

    private void Start()
    {
        UpdateRewardHistory += AddRewardHistory;
        Debug.Log("<b><color=cyan>[CurriculumDebug]</color></b> Inizializzato.");
    }

    public void AddRewardHistory(float reward)
    {
        rewardHistory.Add(reward);

        int count = rewardHistory.Count;
        float avg;

        if (count < minLessonLength)
        {
            avg = rewardHistory.Average();
            Debug.Log($"<color=orange>[Episode {count}]</color> Reward: <b><color=yellow>{reward:F3}</color></b> | Avg. reward (partial): <b>{avg:F3}</b>");
        }
        else
        {
            avg = rewardHistory.Skip(count - minLessonLength).Take(minLessonLength).Average();
            Debug.Log($"<color=green>[Episode {count}]</color> Reward: <b><color=yellow>{reward:F3}</color></b> | Avg. reward (last {minLessonLength}): <b><color=green>{avg:F3}</color></b>");
        }
    }

    public static void OnEpisodeFinish(float totalReward)
    {
        UpdateRewardHistory?.Invoke(totalReward);
    }
}
