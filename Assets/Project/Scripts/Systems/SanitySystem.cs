using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SanitySystem : MonoBehaviour
{
    public float maxSanity = 100f;
    public float currentSanity;
    public float drainRate = 1f;

    public int erosionStack = 0;

    void Start()
    {
        currentSanity = maxSanity;
    }

    void Update()
    {
        float multiplier = 1f + erosionStack * 0.5f;
        currentSanity -= drainRate * multiplier * Time.deltaTime;

        if (currentSanity <= 0)
        {
            Debug.Log("Game Over");
        }
    }
}
