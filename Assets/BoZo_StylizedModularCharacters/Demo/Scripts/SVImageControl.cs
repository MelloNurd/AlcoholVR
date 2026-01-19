using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Bozo.ModularCharacters
{


    public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler, IPointerDownHandler
    {
        [SerializeField] Image PickerImage;

        private RawImage SVImage;

        private ColorPickerControl CC;
        private RectTransform rect;
        private RectTransform pickerTransform;
        private Canvas canvas;

        private void Awake()
        {
            SVImage = GetComponent<RawImage>();
            CC = FindFirstObjectByType<ColorPickerControl>();
            rect = GetComponent<RectTransform>();
            pickerTransform = PickerImage.GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            pickerTransform = PickerImage.GetComponent<RectTransform>();
            pickerTransform.position = new Vector2(-(rect.sizeDelta.x * 0.5f), -(rect.sizeDelta.y * 0.5f));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateColor(eventData);
        }

        private void UpdateColor(PointerEventData eventData)
        {
            Vector3 localPoint;
            
            // Convert the pointer position to local coordinates
            // This works for both mouse and VR raycast hits
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, 
                eventData.position, 
                eventData.pressEventCamera ?? eventData.enterEventCamera, 
                out Vector2 localPos))
            {
                localPoint = localPos;
            }
            else
            {
                // Fallback to old method if screen point conversion fails
                localPoint = rect.InverseTransformPoint(eventData.position);
            }

            float deltaX = rect.sizeDelta.x * 0.5f;
            float deltaY = rect.sizeDelta.y * 0.5f;

            // Clamp to bounds
            localPoint.x = Mathf.Clamp(localPoint.x, -deltaX, deltaX);
            localPoint.y = Mathf.Clamp(localPoint.y, -deltaY, deltaY);

            float x = localPoint.x + deltaX;
            float y = localPoint.y + deltaY;

            float xNorm = x / rect.sizeDelta.x;
            float yNorm = y / rect.sizeDelta.y;

            PickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

            CC.SetSV(xNorm, yNorm);
        }

        public void setPickerPosition(float x, float y)
        {
            if(!rect) rect = GetComponent<RectTransform>();
            if(!pickerTransform) pickerTransform = PickerImage.GetComponent<RectTransform>();

            var xPos = Mathf.Lerp(-rect.sizeDelta.x / 2, rect.sizeDelta.x/ 2, x);
            var yPos = Mathf.Lerp(-rect.sizeDelta.y / 2, rect.sizeDelta.y / 2, y);

            var pos = new Vector2(xPos, yPos);

            pickerTransform.localPosition = pos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateColor(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UpdateColor(eventData);
        }
    }
}
