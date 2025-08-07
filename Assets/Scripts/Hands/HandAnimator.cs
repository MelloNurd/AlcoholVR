using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

[RequireComponent(typeof(Animator))]
public class HandAnimator : MonoBehaviour
{
    private enum Hand
    {
        Left,
        Right
    }

    [SerializeField] private Hand hand = Hand.Left;

    public readonly Finger Thumb = new Finger(FingerType.Thumb);
    public readonly Finger Index = new Finger(FingerType.Index);
    public readonly Finger Middle = new Finger(FingerType.Middle);
    public readonly Finger Ring = new Finger(FingerType.Ring);
    public readonly Finger Pinky = new Finger(FingerType.Pinky);

    public List<Finger> grippingFingers;
    public List<Finger> fistFingers;
    public List<Finger> uiFingers;

    private Animator _handAnimator = null;
    [SerializeField] private NearFarInteractor _interactor;

    private InputDevice CurrentInputDevice => (hand == Hand.Left) 
        ? InputManager.Instance.leftController 
        : InputManager.Instance.rightController;

    private void Awake()
    {
        grippingFingers = new List<Finger> { Middle, Ring, Pinky };
        fistFingers = new List<Finger> { Thumb, Index, Middle, Ring, Pinky };
        uiFingers = new List<Finger> { Thumb, Middle, Ring, Pinky };

        _handAnimator = GetComponent<Animator>();
        //_interactor = transform.parent.GetComponentInChildren<NearFarInteractor>();
    }

    private void Update()
    {
        if (Phone.Instance != null)
        {
            if (CurrentInputDevice == InputManager.Instance.leftController && Phone.Instance.IsActive)
            {
                AnimateFingers(fistFingers, 0f); // open all fingers
                return;
            }

            if (Phone.Instance.IsHandNearPhone)
            {
                AnimateFingers(uiFingers, 1.0f);
                return;
            }
        }

        if(_interactor.hasSelection)
        {
            AnimateFingers(fistFingers, 1.0f); // close all fingers
            return;
        }

        CurrentInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool touchingThumbstick);
        CurrentInputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerPercent);
        CurrentInputDevice.TryGetFeatureValue(CommonUsages.grip, out float gripPercent);

        AnimateFingers(Thumb, touchingThumbstick ? 1.0f : 0.0f);
        AnimateFingers(grippingFingers, gripPercent);
        AnimateFingers(Index, triggerPercent);
    }

    public void AnimateFingers(List<Finger> fingers, float targetValue)
    {
        foreach (Finger finger in fingers)
        {
            finger.target = targetValue;
        }
        AnimateActionInput(fingers);
    }

    public void AnimateFingers(Finger finger, float targetValue)
    {
        finger.target = targetValue;
        AnimateActionInput(finger);
    }

    public void AnimateActionInput(Finger fingerToAnimate)
    {
        //Debug.Log($"Animating {fingerToAnimate.type} to {fingerToAnimate.target}");
        var fingerName = fingerToAnimate.type.ToString();
        var animationBlendValue = fingerToAnimate.target;
        _handAnimator.SetFloat(fingerName, animationBlendValue);
    }

    public void AnimateActionInput(List<Finger> fingersToAnimate)
    {
        foreach (Finger finger in fingersToAnimate)
        {
            AnimateActionInput(finger);
        }
    }
}
