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
        Button button = transform.Find("Select Button").GetComponent<Button>();

        _objectiveText = transform.Find("Text").GetComponent<TMP_Text>();
        _buttonImage = button.GetComponent<Image>();
        _buttonText = button.GetComponentInChildren<TMP_Text>();

        button.onClick.AddListener(ToggleButton);

        _objectiveData = objective;
        SetText(_objectiveData.text);
        
        if(objective.IsTracking)
        {
            EnableButton();
        }
        else
        {
            DisableButton();
        }
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
        buttonState = true;
        _buttonImage.sprite = _enabledImage;
        _buttonText.color = Color.black;
        _objectiveData.IsTracking = true;
    }

    public void DisableButton()
    {
        buttonState = false;
        _buttonImage.sprite = _disabledImage;
        _buttonText.color = Color.white;
        _objectiveData.IsTracking = false;
    }
}
