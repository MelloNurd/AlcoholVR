using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Bozo.ModularCharacters
{
    public class CharacterSpinner : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float spinDir;
        public Transform character;
        private Animator anim;
        float dizzyTimer = 1;

        bool spinning;
        bool isPointerOver;

        public void SetCharacter(Transform character)
        {
            this.character = character;
            anim = character.GetComponentInChildren<Animator>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerOver = true;
            spinning = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerOver = false;
            spinning = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isPointerOver)
            {
                spinning = true;
            }
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
            if (spinning && isPointerOver)
            {
                Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
                spinDir = -mouseDelta.x * 0.5f;
                dizzyTimer = 0.5f;
            }

            character.Rotate(0, spinDir, 0);
            spinDir = Mathf.Lerp(spinDir, 0, Time.deltaTime);
            dizzyTimer -= Time.deltaTime;

            if (dizzyTimer <= 0)
            {
                if (spinDir >= 5 || spinDir <= -5)
                {
                    anim.SetBool("Dizzy", true);
                }
                else
                {
                    anim.SetBool("Dizzy", false);
                }
            }
        }
    }
}
