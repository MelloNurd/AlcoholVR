using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Bozo.ModularCharacters
{
    public class CharacterSpinner : MonoBehaviour
    {
        [SerializeField] GameObject ObjectToRotate;

        public void RotateObject(Slider callingSlider)
        {
            ObjectToRotate.transform.rotation = Quaternion.Euler(0f, callingSlider.value, 0f);
        }
    }
}
