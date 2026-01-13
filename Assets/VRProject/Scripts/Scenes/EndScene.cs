using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private GameObject _pregnancyTextMsgObj;

    public int MadCowMinScore = 10;
    public int TrashketballMinScore = 10;

    private void Start()
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
        if(GlobalStats.playerDrankMysteryDrink)
        {
            _obituary.SetActive(false);
            return;
        }
        _obituary.SetActive(!GlobalStats.called911);
    }
    private void DrunkDriverResults()
    {
        if(GlobalStats.letDrunkFriendDrive)
        {
            _drunkDriverCrashedPolaroid.SetActive(true);
            _drunkDriverStayedPolaroid.SetActive(false);
        }
        else
        {
            _drunkDriverCrashedPolaroid.SetActive(false);
            _drunkDriverStayedPolaroid.SetActive(true);
        }
    }

    private void MysteryDrinkResults()
    {
        _drugTest.SetActive(GlobalStats.playerDrankMysteryDrink);
    }

    private async void PregnancyTestResults()
    {
        _pregnancyTest.SetActive(false);

        if (!GlobalStats.playerWentWithFlirt)
        {
            return;
        }

        if (GlobalStats.Instance.IsFemale)
        {
            _pregnancyTest.SetActive(true);
        }
        else
        {
            // Setup phone texts for male
            await UniTask.Delay(5000); // Wait 5 seconds before sending texts

            Phone.Instance.QueueNotification(new PhoneMessage()
            {
                Sender = "Alice",
                Content = "Hey, we should talk...",
            });

            Phone.Instance.QueueNotification(new PhoneMessage()
            {
                Sender = "Alice",
                Content = "message contains Image.",
            });

            // Messages dont currently support images so this is just manually shown, will redo if we need more images later
            GameObject msg = Instantiate(_pregnancyTextMsgObj, Phone.Instance._messagesContainer.transform.parent);
            msg.transform.localScale = Vector3.one;
            Phone.Instance._messagesContainer.GetComponent<CanvasGroup>().alpha = 0f;
        }
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
        TMP_Text reportCardText = _reportCard.GetComponentInChildren<TMP_Text>();
        if (GlobalStats.DrinkCount > 0)
        {
            _MiP.SetActive(true);
            _reportCard.material = _badGrades;
            if(reportCardText)
                reportCardText.text = "Should have spent more time studying...";
        }
        else
        {
            _MiP.SetActive(false);
            _reportCard.material = _goodGrades;
            if(reportCardText)
                reportCardText.text = "All that hard work paid off!";
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
