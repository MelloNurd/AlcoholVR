using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Bozo.ModularCharacters
{
    public class BlendSlider : MonoBehaviour 
    {
        Slider slider;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text sliderValue;

        [SerializeField] OutfitSystem system;
        [SerializeField] string shape;

        public void Init(OutfitSystem system, string key) 
        {
            slider = GetComponentInChildren<Slider>();
            slider.onValueChanged.AddListener(Apply);

            this.system = system;
            title.text = key;
            
            // Configure auto-sizing for the title text
            if (title is TextMeshProUGUI tmpUGUI)
            {
                tmpUGUI.enableAutoSizing = true;
                tmpUGUI.fontSizeMin = 12;  // Minimum readable size
                tmpUGUI.fontSizeMax = 30;  // Maximum size before shrinking
            }
            
            // Configure auto-sizing for the slider value text
            if (sliderValue is TextMeshProUGUI tmpValue)
            {
                tmpValue.enableAutoSizing = true;
                tmpValue.fontSizeMin = 12;
                tmpValue.fontSizeMax = 30;
            }
            
            // Ensure proper layout by constraining the title width
            LayoutElement titleLayout = title.GetComponent<LayoutElement>();
            if (titleLayout == null)
            {
                titleLayout = title.gameObject.AddComponent<LayoutElement>();
            }
            titleLayout.preferredWidth = 150;  // Constrain title to reasonable width
            
            shape = key;
            var weight = system.GetShapeValue(key);
            slider.value = weight;

        }

        private void OnEnable()
        {
            var weight = system.GetShapeValue(shape);
            slider.value = weight;
        }

        private void UpdateSlider()
        {

        }

        public void Apply(float value) 
        {
            system.SetShape(shape, value);
            sliderValue.text = $"{slider.value}%";
        }
    }
}
