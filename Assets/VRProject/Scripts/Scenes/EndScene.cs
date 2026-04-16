using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScene : MonoBehaviour
{
    [SerializeField] private GameObject _drunkDriverStayedPolaroid;
    [SerializeField] private GameObject _drunkDriverCrashedPolaroid;
    [SerializeField] private GameObject _drunkDriverBlackoutPolaroid;

    [SerializeField] private GameObject _obituary;
    [SerializeField] private GameObject _MiP;
    [SerializeField] private GameObject _pregnancyTest;
    [SerializeField] private GameObject _drugTest;

    [SerializeField] private GameObject _cowPlush;
    [SerializeField] private GameObject _trashketballTrophy;

    [SerializeField] private GameObject _phoneFoundPolaroid;
    [SerializeField] private GameObject _phoneLostPolaroid;
    [SerializeField] private GameObject _phoneBlackoutPolaroid;

    [SerializeField] private GameObject _rageGoodPolaroid;
    [SerializeField] private GameObject _rageBadPolaroid;
    [SerializeField] private GameObject _rageBlackoutPolaroid;

    [SerializeField] private GameObject _concertPolaroid;
    [SerializeField] private GameObject _bandShirt;
    [SerializeField] private GameObject _concertBlackoutPolaroid;

    [SerializeField] private MeshRenderer _reportCard;
    [SerializeField] private Material _goodGrades;
    [SerializeField] private Material _badGrades;

    [SerializeField] private BoolValue _foundPhone;

    [SerializeField] private BoolValue _stoppedFire;
    [SerializeField] private GameObject _stoppedFirePolaroid;
    [SerializeField] private GameObject _fireSpreadPolaroid;
    [SerializeField] private GameObject _fireBlackoutPolaroid;

    [SerializeField] private GameObject _pregnancyTextMsgObj;
    [SerializeField] private GroupPhoto _groupPhoto;

    public int MadCowMinScore = 10;
    public int TrashketballMinScore = 10;

    bool FemaleFlirt = false;
    bool NPCDied = false;
    bool DroveDrunk = false;
    bool NPCRaged = false;
    bool blackedOut = false;

    private void Start()
    {
        blackedOut = GlobalStats.blackedOut;
        ConfigureResults();
    }

    private void ConfigureResults()
    {
        DrunkDriverResults();
        RageResults();
        DrinkCountResults();
        ArcadeResults();
        TrashketballResults();
        LostPhoneResults();
        BroughtAlcoholResults();
        PregnancyTestResults();
        MysteryDrinkResults();
        FireResults();
        Called911Results();
        _groupPhoto.SetPhoto(FemaleFlirt, NPCDied, DroveDrunk, NPCRaged);
    }

    // NEED TO FIGURE OUT BLACK OUT RESULT
    private void Called911Results()
    {
        if(GlobalStats.playerDrankMysteryDrink || blackedOut)
        {
            _obituary.SetActive(false);
            return;
        }
        _obituary.SetActive(!GlobalStats.called911);
        NPCDied = !GlobalStats.called911;
    }
    private void DrunkDriverResults()
    {
        if(blackedOut && !GlobalStats.talkedToDrunkDriverNPC)
        {
            _drunkDriverBlackoutPolaroid.SetActive(true);
            _drunkDriverCrashedPolaroid.SetActive(false);
            _drunkDriverStayedPolaroid.SetActive(false);
        }
        else if(GlobalStats.letDrunkFriendDrive)
        {
            _drunkDriverCrashedPolaroid.SetActive(true);
            _drunkDriverStayedPolaroid.SetActive(false);
            _drunkDriverBlackoutPolaroid.SetActive(false);
            DroveDrunk = true;
        }
        else
        {
            _drunkDriverStayedPolaroid.SetActive(true);
            _drunkDriverCrashedPolaroid.SetActive(false);
            _drunkDriverBlackoutPolaroid.SetActive(false);
            DroveDrunk = false;
        }
    }

    private void RageResults()
    {
        if(blackedOut && !GlobalStats.talkedToDrunkRageNPC)
        {
            _rageBlackoutPolaroid.SetActive(true);
            _rageGoodPolaroid.SetActive(false);
            _rageBadPolaroid.SetActive(false);
        }
        else if (GlobalStats.helpedRagingDrunk)
        {
            _rageGoodPolaroid.SetActive(true);
            _rageBadPolaroid.SetActive(false);
            _rageBlackoutPolaroid.SetActive(false);
            NPCRaged = false;
        }
        else
        {
            _rageGoodPolaroid.SetActive(false);
            _rageBadPolaroid.SetActive(true);
            _rageBlackoutPolaroid.SetActive(false);
            NPCRaged = true;
        }
    }

    private void MysteryDrinkResults()
    {
        _drugTest.SetActive(GlobalStats.playerDrankMysteryDrink);
    }

    // NEED TO FIGURE OUT BLACKOUT RESULT
    private async void PregnancyTestResults()
    {
        if(blackedOut && !GlobalStats.talkedToFlirtNPC)
        {
            _pregnancyTest.SetActive(false);
            return;
        }

        if (GlobalStats.Instance.IsFemale)
        {
            FemaleFlirt = false;
        }
        else
        {
            FemaleFlirt = true;
        }

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
        if(blackedOut && !GlobalStats.talkedToCouchNPC)
        {
            _concertBlackoutPolaroid.SetActive(true);
            _concertPolaroid.SetActive(false);
            _bandShirt.SetActive(false);
        }
        else if (GlobalStats.broughtItems == GlobalStats.BroughtOptions.Alcohol)
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
        if(blackedOut && !GlobalStats.talkedToPhoneNPC)
        {
            _phoneBlackoutPolaroid.SetActive(true);
            _phoneFoundPolaroid.SetActive(false);
            _phoneLostPolaroid.SetActive(false);
        }
        else if (_foundPhone.Value)
        {
            _phoneFoundPolaroid.SetActive(true);
            _phoneLostPolaroid.SetActive(false);
            _phoneBlackoutPolaroid.SetActive(false);
        }
        else
        {
            _phoneFoundPolaroid.SetActive(false);
            _phoneLostPolaroid.SetActive(true);
            _phoneBlackoutPolaroid.SetActive(false);
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
                reportCardText.text = "Ugh! I can't believe I slept through that test after the party! I must've drank more than I thought...";
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
        if(blackedOut && !GlobalStats.talkedToFireNPC)
        {
            _fireBlackoutPolaroid.SetActive(true);
            _fireSpreadPolaroid.SetActive(false);
            _stoppedFirePolaroid.SetActive(false);
        }
        else if(_stoppedFire.Value)
        {
            _fireSpreadPolaroid.SetActive(false);
            _stoppedFirePolaroid.SetActive(true);
            _fireBlackoutPolaroid.SetActive(false);
        }
        else
        {
            _fireSpreadPolaroid.SetActive(true);
            _stoppedFirePolaroid.SetActive(false);
            _fireBlackoutPolaroid.SetActive(false);
        }
    }
}
