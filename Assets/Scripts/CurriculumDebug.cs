
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class CurriculumDebug : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Numero minimo di episodi per calcolare la media stabile.")]
    public int minLessonLength = 100;
    public TMP_Text statisticsText;

    private List<float> rewardHistory = new List<float>();
    public static event Action<float> UpdateRewardHistory;
    public static event Action UpdateEnemyMissileHit;
    public static event Action UpdateEnemyMissileNeutralized;

    private int enemyMissileHit = 0;
    private int enemyMissileNeutralized = 0; //Destroyed by defenceMissile

    private int enemyHitInWindow = 0;
    private int enemyNeutralizedInWindow = 0;
    private float accuracyInWindow = 0f;


    private float averageReward = 0f;

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

        if (count < minLessonLength)
        {
            averageReward = rewardHistory.Average();
            Debug.Log($"<color=orange>[Episode {count}]</color> Reward: <b><color=yellow>{reward:F3}</color></b> | Avg. reward (partial): <b>{averageReward:F3}</b>");
        }
        else
        {
            averageReward = rewardHistory.Skip(count - minLessonLength).Take(minLessonLength).Average();
            Debug.Log($"<color=green>[Episode {count}]</color> Reward: <b><color=yellow>{reward:F3}</color></b> | Avg. reward (last {minLessonLength}): <b><color=green>{averageReward:F3}</color></b>");
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
        enemyHitInWindow++;
        PrintAccuracy();
    }

    private void AddEnemyMissileNeutralized()
    {
        enemyMissileNeutralized++;
        enemyNeutralizedInWindow++;
        PrintAccuracy();
    }

    private int CalculateAccuracyLastHundred()
    {
        int countWindow = enemyNeutralizedInWindow + enemyHitInWindow;
        if (countWindow >= 100)
        {
            accuracyInWindow = enemyNeutralizedInWindow;
            enemyNeutralizedInWindow = enemyHitInWindow = 0;
        }
        return countWindow;
    }

    private void PrintAccuracy()
    {
        float sum = enemyMissileNeutralized + enemyMissileHit;
        string accuracy = "0%";

        if(sum > 0f)
            accuracy = $"{(enemyMissileNeutralized / sum * 100f):F2}%";

        statisticsText.text =
            "<color=orange><b>[Accuracy Report]</b></color>\n" +
            $"<b>Total:</b> {enemyMissileHit + enemyMissileNeutralized}\n" +
            $"<color=#ADFF2F><b>Neutralized:</b> {enemyMissileNeutralized}</color>\n" +
            $"<color=red><b>Enemy Hits:</b> {enemyMissileHit}</color>\n" +
            $"<color=blue><b>Total Accuracy:</b> {accuracy}</color>\n" +
            $"<color=#DA70D6><b>Accuracy ({CalculateAccuracyLastHundred()}/100 steps):</b> {accuracyInWindow}%</color>\n" +
            $"<color=yellow><b>Avg. Reward (last 100):</b> {averageReward:F2}</color>";
    }
}
