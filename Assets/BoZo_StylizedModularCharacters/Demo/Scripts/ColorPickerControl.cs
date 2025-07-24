using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bozo.ModularCharacters
{
    public class ColorPickerControl : MonoBehaviour
    {
        public float currentHue;
        public float currentSat;
        public float currentVal;
        public float currentColor;

        [SerializeField] private RawImage hueImage;
        [SerializeField] private RawImage satValImage;
        [SerializeField] private RawImage outputImage;

        [SerializeField] private Slider hueSlider;

        private Texture2D hueTexture;
        private Texture2D svTexture;
        private Texture2D outputTexture;

        public Renderer colorObject;
        public Material colorMaterial;
        public int MaterialSlot;

        [SerializeField] Text objectName;
        [SerializeField] Image[] Swatches;
        [SerializeField] int currentSwatch;

        private int ActiveReferneceSet;
        private List<string[]> ReferneceSet = new List<string[]>();
        [SerializeField] string[] OutfitReferneceIDs;
        [SerializeField] string[] SkinReferneceIDs;
        [SerializeField] string[] AccessoryReferneceIDs;
        [SerializeField] string[] EyesReferneceIDs;

        [SerializeField] HandColorer LeftHand;
        [SerializeField] HandColorer RightHand;

        private void Start()
        {
            CreateHueImage();
            CreateSVImage();
            CreateOutputImage();
            UpdateOutputImage();

            ReferneceSet.Add(OutfitReferneceIDs);
            ReferneceSet.Add(SkinReferneceIDs);
            ReferneceSet.Add(AccessoryReferneceIDs);
            ReferneceSet.Add(EyesReferneceIDs);
        }

        private void CreateHueImage()
        {
            hueTexture = new Texture2D(1, 16);
            hueTexture.wrapMode = TextureWrapMode.Clamp;
            hueTexture.name = "HueTexture";

            for (int i = 0; i < hueTexture.height; i++)
            {
                hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1));
            }

            hueTexture.Apply();

            currentHue = 0;
            hueImage.texture = hueTexture;
        }

        private void CreateSVImage()
        {
            svTexture = new Texture2D(16, 16);
            svTexture.wrapMode = TextureWrapMode.Clamp;
            svTexture.name = "SVTexture";

            for (int y = 0; y < svTexture.height; y++)
            {
                for (int x = 0; x < svTexture.width; x++)
                {
                    svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
                }
            }

            svTexture.Apply();

            currentSat = 0;
            currentVal = 0;
            satValImage.texture = svTexture;
        }

        private void CreateOutputImage()
        {
            outputTexture = new Texture2D(1, 16);
            outputTexture.wrapMode = TextureWrapMode.Clamp;
            outputTexture.name = "OutputTexture";

            Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

            for (int i = 0; i < hueTexture.height; i++)
            {
                outputTexture.SetPixel(0, 1, currentColor);
            }

            outputTexture.Apply();

            currentHue = 0;
            outputImage.texture = outputTexture;
        }

        private void UpdateOutputImage()
        {
            Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

            if (ActiveReferneceSet == 1)
            {
                LeftHand.UpdateHandColor(currentColor);
                RightHand.UpdateHandColor(currentColor);
            }

            for (int i = 0; i < outputTexture.height; i++)
            {
                outputTexture.SetPixel(0, i, currentColor);
            }

            outputTexture.Apply();

            if (!colorObject) return;

            Swatches[currentSwatch].color = currentColor;

            var oldColor = colorObject.materials[MaterialSlot].GetColor(ReferneceSet[ActiveReferneceSet][currentSwatch]);
            currentColor.a = oldColor.a;
            colorObject.materials[MaterialSlot].SetColor(ReferneceSet[ActiveReferneceSet][currentSwatch], currentColor);

            colorObject.materials[MaterialSlot] = colorMaterial;
        }

        public void SetSV(float S, float V)
        {
            currentSat = S;
            currentVal = V;
            UpdateOutputImage();
        }

        public void SetHSV(float H, float S, float V)
        {
            currentHue = H;
            currentSat = S;
            currentVal = V;
            UpdateOutputImage();
        }

        public void UpdateSVImage()
        {
            currentHue = hueSlider.value;

            for (int y = 0; y < svTexture.height; y++)
            {
                for (int x = 0; x < svTexture.width; x++)
                {
                    svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));

                }
            }

            svTexture.Apply();
            UpdateOutputImage();
        }

        public void ChangeSwatch(int value)
        {
            currentSwatch = value;
            var swatchColor = Swatches[currentSwatch].color;
            Color.RGBToHSV(swatchColor, out float h, out float s, out float v);
            hueSlider.value = h;
            SetHSV(h, s, v);
            UpdateSVImage();
        }

        public void ChangeObject(Transform ob)
        {
            if (ob == null)
            {
                return;
            }

            for (int i = 0; i < Swatches.Length; i++)
            {
                Swatches[i].gameObject.SetActive(true);
            }

            var renderers = ob.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            colorObject = renderers[0];
            if (colorObject == null) return;

            ActiveReferneceSet = 0;

            for (int i = 0; i < colorObject.materials.Length; i++)
            {
                var sort = colorObject.materials[i].name.Split("_");

                if (sort[1] == "Outfit")
                {
                    colorMaterial = colorObject.materials[i];
                    MaterialSlot = i;
                    for (int u = 1; u < renderers.Length; u++)
                    {
                        renderers[u].materials = colorObject.materials;
                    }
                    break;
                }
                else
                {
                    colorMaterial = null;
                }
            }

            //if (colorMaterial == null)
            //{
            //    colorObject = null;
            //    foreach (var item in Swatches) { item.color = new Color(0, 0, 0, 0); }
            //    return;
            //}

            for (int i = 0; i < Swatches.Length; i++)
            {
                string colorProperty = "_Color_" + (i + 1);
                if (colorMaterial.HasProperty(colorProperty))
                {
                    Swatches[i].color = colorMaterial.GetColor(colorProperty);
                }
                else
                {
                    Swatches[i].color = new Color(0, 0, 0, 0);
                }
            }

            for (int i = 0; i < Swatches.Length; i++)
            {
                if (Swatches[i].color.a == 0)
                {
                    Swatches[i].gameObject.SetActive(false);
                }
            }

            ChangeSwatch(0);

            objectName.text = colorObject.name.Replace("(Clone)", "");

        }

        public void SelectSkin(Transform transform)
        {
            var system = transform.GetComponent<OutfitSystem>();

            colorMaterial = system.CharacterMaterial;
            colorObject = system.GetCharacterBody();
            ActiveReferneceSet = 1;
            MaterialSlot = 0;

            // Ensure ReferneceSet is properly initialized
            if (ReferneceSet == null || ReferneceSet.Count <= ActiveReferneceSet)
            {
                Debug.LogError("ReferneceSet is not properly initialized or doesn't contain enough elements.");
                return;
            }

            // Get the current reference array
            string[] currentReferenceArray = ReferneceSet[ActiveReferneceSet];
            if (currentReferenceArray == null)
            {
                Debug.LogError("SkinReferneceIDs array is null.");
                return;
            }

            for (int i = 0; i < Swatches.Length; i++)
            {
                Swatches[i].gameObject.SetActive(true);
            }

            for (int i = 0; i < Swatches.Length; i++)
            {
                // Add bounds checking for the reference array
                if (i < currentReferenceArray.Length && currentReferenceArray[i] != "")
                {
                    if (colorMaterial.HasProperty(currentReferenceArray[i]))
                    {
                        Swatches[i].color = colorMaterial.GetColor(currentReferenceArray[i]);
                    }
                    else
                    {
                        Debug.LogWarning($"Material doesn't have property: {currentReferenceArray[i]}");
                        Swatches[i].color = new Color(0, 0, 0, 0);
                        Swatches[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    Swatches[i].color = new Color(0, 0, 0, 0);
                    Swatches[i].gameObject.SetActive(false);
                }
            }

            // Add bounds checking for currentSwatch
            if (currentSwatch < currentReferenceArray.Length && currentReferenceArray[currentSwatch] != "")
            {
                string propertyName = currentReferenceArray[currentSwatch];
                if (colorMaterial.HasProperty(propertyName))
                {
                    colorObject.materials[MaterialSlot].SetColor(propertyName,
                        colorObject.materials[MaterialSlot].GetColor(propertyName));
                }
                else
                {
                    Debug.LogWarning($"Material doesn't have property for currentSwatch: {propertyName}");
                }
            }
            else
            {
                Debug.LogWarning($"currentSwatch ({currentSwatch}) is out of bounds for the reference array or the property is empty.");
                // Reset currentSwatch to a valid index
                currentSwatch = 0;
            }

            UpdateHandSkins();
            system.SetSkin(colorObject.materials[MaterialSlot]);
        }


        public void UpdateHandSkins()
        {
            // Add null check to prevent UnassignedReferenceException
            if (colorObject == null)
            {
                Debug.LogWarning("Cannot update hand skins: colorObject is not assigned yet.");
                return;
            }

            // Add bounds checking for ReferneceSet and currentSwatch
            if (ReferneceSet == null || ReferneceSet.Count <= ActiveReferneceSet)
            {
                Debug.LogWarning("Cannot update hand skins: ReferneceSet is not properly initialized.");
                return;
            }

            string[] currentReferenceArray = ReferneceSet[ActiveReferneceSet];
            if (currentReferenceArray == null || currentSwatch >= currentReferenceArray.Length)
            {
                Debug.LogWarning("Cannot update hand skins: currentSwatch is out of bounds or reference array is null.");
                return;
            }

            string propertyName = currentReferenceArray[currentSwatch];
            if (string.IsNullOrEmpty(propertyName))
            {
                Debug.LogWarning("Cannot update hand skins: property name is empty.");
                return;
            }

            if (colorObject.materials[MaterialSlot].HasProperty(propertyName))
            {
                Color handColor = colorObject.materials[MaterialSlot].GetColor(propertyName);
                LeftHand.UpdateHandColor(handColor);
                RightHand.UpdateHandColor(handColor);
            }
            else
            {
                Debug.LogWarning($"Cannot update hand skins: Material doesn't have property {propertyName}.");
            }
        }


        public void SelectEyes(Transform transform)
        {
            var system = transform.GetComponent<OutfitSystem>();

            MaterialSlot = 1;
            colorMaterial = system.GetCharacterBody().materials[1];
            colorObject = system.GetCharacterBody();
            ActiveReferneceSet = 3;

            for (int i = 0; i < Swatches.Length; i++)
            {
                Swatches[i].gameObject.SetActive(true);
            }

            colorObject.materials[MaterialSlot].SetColor(ReferneceSet[ActiveReferneceSet][currentSwatch],
            colorObject.materials[MaterialSlot].GetColor(ReferneceSet[ActiveReferneceSet][currentSwatch]));
            system.SetEyes(colorObject.materials[MaterialSlot]);

            for (int i = 0; i < Swatches.Length; i++)
            {
                if (ReferneceSet[ActiveReferneceSet][i] != "")
                {
                    Swatches[i].color = colorMaterial.GetColor(ReferneceSet[ActiveReferneceSet][i]);
                }
                else
                {
                    Swatches[i].color = new Color(0, 0, 0, 0);
                    Swatches[i].gameObject.SetActive(false);
                }
            }


        }

        public void SelectAcc(Transform transform)
        {
            var system = transform.GetComponent<OutfitSystem>();

            colorMaterial = system.CharacterMaterial;
            colorObject = system.GetCharacterBody();
            ActiveReferneceSet = 2;
            MaterialSlot = 0;

            for (int i = 0; i < Swatches.Length; i++)
            {
                if (ReferneceSet[ActiveReferneceSet][i] != "")
                {
                    Swatches[i].color = colorMaterial.GetColor(ReferneceSet[ActiveReferneceSet][i]);
                }
                else
                {
                    Swatches[i].color = new Color(0, 0, 0, 0);
                }
            }

            colorObject.materials[MaterialSlot].SetColor(ReferneceSet[ActiveReferneceSet][currentSwatch],
                colorObject.materials[MaterialSlot].GetColor(ReferneceSet[ActiveReferneceSet][currentSwatch]));
            system.SetSkin(colorObject.materials[MaterialSlot]);
        }

        public void CopyColor(Renderer copyOutfit)
        {
            Material FromMaterial = colorMaterial;
            Material ToMaterial = null;

            for (int i = 0; i < copyOutfit.materials.Length; i++)
            {
                var sort = copyOutfit.materials[i].name.Split("_");

                if (sort[1] == "Outfit")
                {
                    ToMaterial = copyOutfit.materials[i];
                    MaterialSlot = i;
                }
                else
                {
                    return;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                ToMaterial.SetColor("_Color_" + (i + 1), FromMaterial.GetColor("_Color_" + (i + 1)));
            }
        }
    }
}
