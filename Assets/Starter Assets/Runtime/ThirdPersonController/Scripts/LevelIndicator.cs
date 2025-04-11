using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelIndicator : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private int levelNumber = 1;
    
    void Start()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + levelNumber;
        }
    }
}