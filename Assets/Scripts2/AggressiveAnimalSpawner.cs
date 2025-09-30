using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggressiveAnimalSpawner : MonoBehaviour
{
    public List<AggressiveAnimalDen> animalDens;
    public float checkInterval = 10f;

    void Start()
    {
        int totalMax = 0;
        foreach (var den in animalDens)
        {
            if (den != null)
                totalMax += den.maxAnimals;
        }
        AggressiveAnimalManager.Instance.SetGlobalMaxCount(totalMax);

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            foreach (var den in animalDens)
            {
                den?.TrySpawnAnimal();
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }
}
