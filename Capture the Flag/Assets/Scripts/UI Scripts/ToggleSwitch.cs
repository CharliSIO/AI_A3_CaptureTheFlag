using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using RangeAttribute = UnityEngine.RangeAttribute; // system also has a range attribute, using unityengine range

// use the slider to make a toggle!
// looks better than a checkbox
// from youtube https://youtu.be/E9AWlbPGi_4 
// with some edits too!
public class ToggleSwitch : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slider Setup")]
    [SerializeField, Range(0, 1f)] private float m_SliderValue;

    public bool m_CurrentValue { get; private set; }
    private bool m_PreviousValue;

    private Slider m_Slider;

    [Header("Animation")]
    [SerializeField, Range(0, 1f)] private float m_AnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve m_SliderEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color startSlideColour = new(0, 192, 255);
    [SerializeField] private Color endSlideColour = new(140, 0, 255);

    [SerializeField] private Image m_HandleSprite;
    [SerializeField] private Color startHandleColour;
    [SerializeField] private Color endHandleColour;
    [SerializeField] private float startScale = 1.1f;

    private Coroutine m_AnimationSliderCoroutine;
    private Coroutine m_HoverAnimationCoroutine;

    [Header("Events")]
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;

    private ToggleSwitchGroupManager m_ToggleSwitchGroupManager;

    protected void OnValidate()
    {
        SetupToggleComponents();

        m_Slider.value = m_SliderValue;
    }

    private void SetupToggleComponents()
    {
        if (m_Slider != null) return;

        SetupSliderComponent();
    }

    private void SetupSliderComponent()
    {
        m_Slider = GetComponent<Slider>();

        if (m_Slider == null)
        {
            Debug.Log("No slider found!", this);
            return;
        }

        m_Slider.interactable = false;
        var sliderColours = m_Slider.colors;
        sliderColours.disabledColor = Color.white;
        m_Slider.colors = sliderColours;
        m_Slider.transition = Selectable.Transition.None;
    }

    // for if theres a group!
    public void SetupForManager(ToggleSwitchGroupManager _manager)
    {
        m_ToggleSwitchGroupManager = _manager;
    }

    private void Awake()
    {
        SetupToggleComponents();
    }

    public void OnPointerClick(PointerEventData _eventData)
    {
        Toggle();
    }

    private void Toggle()
    {
        if (m_ToggleSwitchGroupManager != null)
        {
            m_ToggleSwitchGroupManager.ToggleGroup(this);
        }
        else
        {
            SetStateAndStartAnimation(!m_CurrentValue);
        }
    }

    public void ToggleByGroupManager(bool _valueToSetTo)
    {
        SetStateAndStartAnimation(_valueToSetTo);
    }

    private void SetStateAndStartAnimation(bool _state)
    {
        m_PreviousValue = m_CurrentValue;
        m_CurrentValue = _state;

        if (m_PreviousValue != m_CurrentValue)
        {
            if (m_CurrentValue) onToggleOn?.Invoke();
            else onToggleOff?.Invoke();
        }

        // stops the animation if already animating, then animate
        if (m_AnimationSliderCoroutine != null) StopCoroutine(m_AnimationSliderCoroutine);
        m_AnimationSliderCoroutine = StartCoroutine(AnimateSlider());
    }

    // ease in ease out
    // left/right only!
    // oh yeah we got the juice
    private IEnumerator AnimateSlider()
    {
        float startValue = m_Slider.value;
        float endValue = m_CurrentValue ? 1 : 0;

        float time = 0;
        if (m_AnimationDuration > 0)
        {
            while (time < m_AnimationDuration)
            {
                time += Time.deltaTime;

                float lerpT = m_SliderEase.Evaluate(time / m_AnimationDuration);
                m_Slider.value = m_SliderValue = Mathf.Lerp(startValue, endValue, lerpT);

                yield return null;
            }
        }
    }

    public void OnPointerEnter(PointerEventData _eventData)
    {
        Hover(true);
    }

    public void OnPointerExit(PointerEventData _eventData)
    {
        Hover(false);
    }

    private void Hover(bool _Hovering)
    {
        if (_Hovering)
        {
            // stops the animation if already animating, then animate
            if (m_HoverAnimationCoroutine != null) StopCoroutine(m_HoverAnimationCoroutine);
            m_HoverAnimationCoroutine = StartCoroutine(HoverAnimation());
        }
        else
        {
            if (m_HoverAnimationCoroutine != null) StopCoroutine(m_HoverAnimationCoroutine);
            m_HandleSprite.color = startHandleColour;
            m_HandleSprite.gameObject.transform.localScale = new Vector3(startScale, startScale, 1.0f);
        }
    }

    private IEnumerator HoverAnimation()
    {
        float time = 0;
        if (m_AnimationDuration > 0)
        {
            while (time < m_AnimationDuration)
            {
                time += Time.deltaTime;

                // just using same anim for now
                float lerpT = m_SliderEase.Evaluate(time / (m_AnimationDuration/1.25f)); // but faster
                m_HandleSprite.color = Color.Lerp(startHandleColour, endHandleColour, lerpT);

                float scale = Mathf.Lerp(startScale, 1.25f, lerpT);
                m_HandleSprite.gameObject.transform.localScale = new Vector3(scale, scale, 1.0f);

                yield return null;
            }
        }
    }
}