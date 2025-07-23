using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    public bool buttonState = false;

    [SerializeField] private Sprite _enabledImage;
    [SerializeField] private Sprite _disabledImage;

    private TMP_Text _objectiveText;
    private Image _buttonImage;
    private TMP_Text _buttonText;

    private Objective _objectiveData;

    public void Initialize(Objective objective)
    {
        _objectiveText = transform.Find("Text").GetComponent<TMP_Text>();

        Button button = transform.Find("Select Button").GetComponent<Button>();
        if(objective.point != null)
        {
            _buttonImage = button.GetComponent<Image>();
            _buttonText = button.GetComponentInChildren<TMP_Text>();

            button.onClick.AddListener(ToggleButton);

            if (objective.IsTracking)
            {
                EnableButton();
            }
            else
            {
                DisableButton();
            }
        } 
        else
        {
            button.gameObject.SetActive(false);
        }

            _objectiveData = objective;
        SetText(_objectiveData.text);
    }

    public void SetText(string text)
    {
        _objectiveText.text = text;
    }

    public void ToggleButton()
    {
        if (buttonState)
        {
            DisableButton();
        }
        else
        {
            EnableButton();
        }
    }

    public void EnableButton()
    {
        if(_buttonImage == null || _buttonText == null)
        {
            return;
        }

        buttonState = true;
        _buttonImage.sprite = _enabledImage;
        _buttonText.color = Color.black;
        if (_objectiveData != null)
        {
            _objectiveData.IsTracking = true;
        }
    }

    public void DisableButton()
    {
        if (_buttonImage == null || _buttonText == null)
        {
            return;
        }

        buttonState = false;
        _buttonImage.sprite = _disabledImage;
        _buttonText.color = Color.white;

        if(_objectiveData != null)
        {
            _objectiveData.IsTracking = false;
        }
    }
}
