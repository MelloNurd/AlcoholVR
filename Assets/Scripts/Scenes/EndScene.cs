using UnityEngine;

public class EndScene : MonoBehaviour
{
    [SerializeField] private GameObject _happyPolaroid;
    [SerializeField] private GameObject _madPolaroid;

    void Start()
    {
        int state = PlayerPrefs.GetInt("BroughtBeer", 0);

        _happyPolaroid.SetActive(state == 0);
        _madPolaroid.SetActive(state == 1);
    }
}
