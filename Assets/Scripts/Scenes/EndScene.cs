using UnityEngine;

public class EndScene : MonoBehaviour
{
    [SerializeField] private GameObject _drunkDriverStayedPolaroid;
    [SerializeField] private GameObject _drunkDriverCrashedPolaroid;
    [SerializeField] private GameObject _phoneFoundPolaroid;
    [SerializeField] private GameObject _phoneLostPolaroid;

    [SerializeField] private BoolValue _foundPhone;

    void Start()
    {
        // Drunk driving outcome
        //_drunkDriverStayedPolaroid.SetActive(!GlobalStats.LetDrunkFriendDrive);
        _drunkDriverCrashedPolaroid.SetActive(GlobalStats.LetDrunkFriendDrive);

        // Phone outcome
        _phoneFoundPolaroid.SetActive(_foundPhone.Value);
        _phoneLostPolaroid.SetActive(!_foundPhone.Value);
    }
}
