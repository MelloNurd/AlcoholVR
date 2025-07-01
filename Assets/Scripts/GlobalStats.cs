using UnityEngine;

public class GlobalStats : MonoBehaviour
{
    public static GlobalStats Instance { get; private set; }

    public static int DrinkCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
