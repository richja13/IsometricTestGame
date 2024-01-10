using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRandomStats
{
    public static float GenerateRandomValue(float min, float max)
    {
        float value = Random.Range(min,max);
        return value;
    }
}
