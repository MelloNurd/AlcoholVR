using System;
using TMPro;
using UnityEngine;

public class EndScene : MonoBehaviour
{
    [SerializeField] private GameObject _drunkDriverStayedPolaroid;
    [SerializeField] private GameObject _drunkDriverCrashedPolaroid;
    [SerializeField] private GameObject _obituary;
    [SerializeField] private GameObject _MiP;
    [SerializeField] private GameObject _pregnancyTest;
    [SerializeField] private GameObject _drugTest;

    [SerializeField] private GameObject _cowPlush;
    [SerializeField] private GameObject _trashketballTrophy;

    [SerializeField] private GameObject _phoneFoundPolaroid;
    [SerializeField] private GameObject _phoneLostPolaroid;

    [SerializeField] private GameObject _concertPolaroid;
    [SerializeField] private GameObject _bandShirt;

    [SerializeField] private MeshRenderer _reportCard;
    [SerializeField] private Material _goodGrades;
    [SerializeField] private Material _badGrades;

    [SerializeField] private BoolValue _foundPhone;

    [SerializeField] private BoolValue _stoppedFire;
    [SerializeField] private GameObject _stoppedFirePicture;
    [SerializeField] private GameObject _fireSpreadPicture;

    public int MadCowMinScore = 10;
    public int TrashketballMinScore = 10;

    void Start()
    {
        ConfigureResults();
    }

    private void ConfigureResults()
    {
        DrunkDriverResults();
        DrinkCountResults();
        ArcadeResults();
        TrashketballResults();
        LostPhoneResults();
        BroughtAlcoholResults();
        PregnancyTestResults();
        MysteryDrinkResults();
        FireResults();
        Called911Results();
    }

    private void Called911Results()
    {
        _obituary.SetActive(!GlobalStats.called911);
    }
    private void DrunkDriverResults()
    {
        if(GlobalStats.letDrunkFriendDrive)
        {
            _drunkDriverCrashedPolaroid.SetActive(true);
            //_drunkDriverStayedPolaroid.SetActive(false);
        }
        else
        {
            _drunkDriverCrashedPolaroid.SetActive(false);
            //_drunkDriverStayedPolaroid.SetActive(true);
        }
    }

    private void MysteryDrinkResults()
    {
        if (!GlobalStats.playerDrankMysteryDrink)
        {
            _drugTest.SetActive(false);
            _obituary.SetActive(!GlobalStats.called911);
        }
        else
        {
            _drugTest.SetActive(true);
        }
    }

    private void PregnancyTestResults()
    {
        _pregnancyTest.SetActive(GlobalStats.playerWentWithFlirt);
    }

    private void BroughtAlcoholResults()
    {
        if (GlobalStats.broughtItems == GlobalStats.BroughtOptions.Alcohol)
        {
            _concertPolaroid.SetActive(true);
            _bandShirt.SetActive(false);
        }
        else
        {
            _concertPolaroid.SetActive(false);
            _bandShirt.SetActive(true);
        }
    }

    private void LostPhoneResults()
    {
        if (_foundPhone.Value)
        {
            _phoneFoundPolaroid.SetActive(true);
            _phoneLostPolaroid.SetActive(false);
        }
        else
        {
            _phoneFoundPolaroid.SetActive(false);
            _phoneLostPolaroid.SetActive(true);
        }
    }

    private void TrashketballResults()
    {
        if (GlobalStats.hoopsScore >= TrashketballMinScore)
        {
            _trashketballTrophy.SetActive(true);
        }
        else
        {
            _trashketballTrophy.SetActive(false);
        }
    }

    private void ArcadeResults()
    {
        if (GlobalStats.arcadeScore >= MadCowMinScore)
        {
            _cowPlush.SetActive(true);
        }
        else
        {
            _cowPlush.SetActive(false);
        }
    }

    private void DrinkCountResults()
    {
        if (GlobalStats.DrinkCount > 0)
        {
            _MiP.SetActive(true);
            _reportCard.material = _badGrades;
            _reportCard.GetComponentInChildren<TMP_Text>().text = "Should have spent more time studying...";
        }
        else
        {
            _MiP.SetActive(false);
            _reportCard.material = _goodGrades;
            _reportCard.GetComponentInChildren<TMP_Text>().text = "All that hard work paid off!";
        }
    }

    private void FireResults()
    {
        if(_stoppedFire.Value)
        {
            _fireSpreadPicture.SetActive(false);
            _stoppedFirePicture.SetActive(true);
        }
        else
        {
            _fireSpreadPicture.SetActive(true);
            _stoppedFirePicture.SetActive(false);
        }
    }
}
