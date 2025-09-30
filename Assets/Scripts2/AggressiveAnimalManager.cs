using UnityEngine;

public class AggressiveAnimalManager : MonoBehaviour
{
    public static AggressiveAnimalManager Instance;

    private int currentAnimalCount = 0;
    private int maxAnimalCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterAnimal() => currentAnimalCount++;
    public void UnregisterAnimal() => currentAnimalCount = Mathf.Max(0, currentAnimalCount - 1);

    public int GetCurrentCount() => currentAnimalCount;
    public int GetMaxCount() => maxAnimalCount;

    public bool CanSpawn() => currentAnimalCount < maxAnimalCount;

    public void SetGlobalMaxCount(int count) => maxAnimalCount = count;
}
