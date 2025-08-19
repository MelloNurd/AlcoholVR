using UnityEngine;

public class EndScene : MonoBehaviour
{
    [SerializeField] private GameObject _drunkDriverStayedPolaroid;
    [SerializeField] private GameObject _drunkDriverCrashedPolaroid;
    [SerializeField] private GameObject _obituary;
    [SerializeField] private GameObject _MiP;
    [SerializeField] private GameObject _cowPlush;
    [SerializeField] private GameObject _trashketballTrophy;

    [SerializeField] private GameObject _phoneFoundPolaroid;
    [SerializeField] private GameObject _phoneLostPolaroid;

    [SerializeField] private MeshRenderer _reportCard;
    [SerializeField] private Material _goodGrades;
    [SerializeField] private Material _badGrades;

    [SerializeField] private BoolValue _foundPhone;

    public int MadCowMinScore = 20;
    public int TrashketballMinScore = 10;

    void Start()
    {
        // Drunk driving outcome
        //_drunkDriverStayedPolaroid.SetActive(!GlobalStats.LetDrunkFriendDrive);
        _drunkDriverCrashedPolaroid.SetActive(GlobalStats.letDrunkFriendDrive);
        _obituary.SetActive(!GlobalStats.called911);
        _MiP.SetActive(GlobalStats.DrinkCount > 0);
        _reportCard.material = GlobalStats.DrinkCount > 0 ? _badGrades : _goodGrades;
        if(GlobalStats.arcadeScore >= MadCowMinScore)
        {
            _cowPlush.SetActive(true);
        }
        else
        {
            _cowPlush.SetActive(false);
        }
        if (GlobalStats.hoopsScore >= TrashketballMinScore)
        {
            _trashketballTrophy.SetActive(true);
        }
        else
        {
            _trashketballTrophy.SetActive(false);
        }

        // Phone outcome
        _phoneFoundPolaroid.SetActive(_foundPhone.Value);
        _phoneLostPolaroid.SetActive(!_foundPhone.Value);
    }
}
