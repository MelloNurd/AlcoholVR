using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Bozo.ModularCharacters
{
    public class SVImageControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image PickerImage;

        private RawImage SVImage;
        private ColorPickerControl CC;
        private RectTransform rect;
        private RectTransform pickerTransform;

        private bool isDragging = false;
        private PointerEventData currentEventData;

        private void Awake()
        {
            SVImage = GetComponent<RawImage>();
            CC = FindFirstObjectByType<ColorPickerControl>();
            rect = GetComponent<RectTransform>();
            pickerTransform = PickerImage.GetComponent<RectTransform>();

            pickerTransform.localPosition = new Vector2(-(rect.sizeDelta.x * 0.5f), -(rect.sizeDelta.y * 0.5f));
        }

        private void Update()
        {
            if (isDragging && currentEventData != null)
            {
                UpdateColor(currentEventData);
            }
        }

        private void UpdateColor(PointerEventData eventData)
        {
            if (eventData.pressEventCamera == null)
            {
                Debug.LogWarning("EventData pressEventCamera is null.");
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                float halfWidth = rect.sizeDelta.x * 0.5f;
                float halfHeight = rect.sizeDelta.y * 0.5f;

                localPoint.x = Mathf.Clamp(localPoint.x, -halfWidth, halfWidth);
                localPoint.y = Mathf.Clamp(localPoint.y, -halfHeight, halfHeight);

                pickerTransform.localPosition = localPoint;

                float xNorm = (localPoint.x + halfWidth) / rect.sizeDelta.x;
                float yNorm = (localPoint.y + halfHeight) / rect.sizeDelta.y;

                PickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

                CC.SetSV(xNorm, yNorm);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            currentEventData = eventData;
            UpdateColor(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            currentEventData = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            currentEventData = eventData; // Keep updating event data reference
            UpdateColor(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UpdateColor(eventData);
        }
    }
}
