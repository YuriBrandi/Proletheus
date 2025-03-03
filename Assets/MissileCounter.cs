using UnityEngine;
using UnityEngine.UI;

public class MissileCounter : MonoBehaviour
{
    private int missileCounter;
    private Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        missileCounter = 0;
        text = GetComponent<Text>();
    }

    public void missileCounterIncrement()
    {
        missileCounter++;
        text.text = missileCounter.ToString();
    }

    public void missileCounterDecrement()
    {
        missileCounter--;
        text.text = missileCounter.ToString();
    }
}
