
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using static Unity.Burst.Intrinsics.X86;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using UnityEngine.UI;

public class CurriculumDebug : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Numero minimo di episodi per calcolare la media stabile.")]
    public int minLessonLength = 100;
    public Text statisticsText;

    private List<float> rewardHistory = new List<float>();
    public static event Action<float> UpdateRewardHistory;
    public static event Action UpdateEnemyMissileHit;
    public static event Action UpdateEnemyMissileNeutralized;

    private int enemyMissileHit = 0;
    private int enemyMissileNeutralized = 0; //Destroyed by defenceMissile

    private void Start()
    {
        UpdateRewardHistory += AddRewardHistory;
        UpdateEnemyMissileHit += AddEnemyMissileHit;
        UpdateEnemyMissileNeutralized += AddEnemyMissileNeutralized;

        PrintAccuracy();

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

    /*
     *  neutralized:
     *      - true if destroyed by enemyMissile
     *      - false if self destroyed
     */
    public static void OnEnemyMissileDestroyed(bool neutralized)
    {
        if (neutralized)
            UpdateEnemyMissileNeutralized?.Invoke();
        else
            UpdateEnemyMissileHit?.Invoke();
    }

    private void AddEnemyMissileHit()
    {
        enemyMissileHit++;
        PrintAccuracy();
    }

    private void AddEnemyMissileNeutralized()
    {
        enemyMissileNeutralized++;
        PrintAccuracy();
    }

    private void PrintAccuracy()
    {
        float sum = enemyMissileNeutralized + enemyMissileHit;
        string accuracy = "0%";

        if(sum > 0f)
            accuracy = $"{(enemyMissileNeutralized / sum * 100f):F2}%";

        statisticsText.text =
            "<color=lime><b>[Accuracy Report]</b></color>\n" +
            $"<b>Total:</b> {enemyMissileHit+enemyMissileNeutralized}\n"+
            $"<color=#ADFF2F><b>Neutralized:</b> {enemyMissileNeutralized}</color>\n" +
            $"<color=red><b>Enemy Hits:</b> {enemyMissileHit}</color>\n" +
            $"<color=cyan><b>Total Accuracy:</b> {accuracy}</color>";
    }
}
