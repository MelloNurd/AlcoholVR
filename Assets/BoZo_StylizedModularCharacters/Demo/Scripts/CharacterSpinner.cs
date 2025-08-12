using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Bozo.ModularCharacters
{
    public class CharacterSpinner : MonoBehaviour, IDragHandler, IPointerClickHandler
    {
        public float spinDir;
        public Transform character;

        bool spinning;

        public void SetCharacter(Transform character)
        {
            this.character = character;
        }

        public void OnDrag(PointerEventData eventData)
        {
            spinning = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            spinning = true;
        }

        private void Start()
        {
            SetCharacter(character);
        }

        private void Update()
        {


            if (spinning)
            {
                spinDir = -Input.GetAxis("Mouse X") * 5;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                spinning = false;
            }
            character.Rotate(0, spinDir, 0);
            spinDir = Mathf.Lerp(spinDir, 0, Time.deltaTime);
        }

        public void Spin(float amount)
        {
            character.rotation = Quaternion.Euler(0, amount, 0);
        }
    }
}
