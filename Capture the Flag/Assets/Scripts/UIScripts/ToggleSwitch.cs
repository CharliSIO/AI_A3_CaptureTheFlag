using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using RangeAttribute = UnityEngine.RangeAttribute; // system also has a range attribute, using unityengine range

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Slider Setup")]
    [SerializeField, Range(0, 1f)] private float m_SliderValue;

    public bool m_CurrentValue { get; private set; }
    private bool m_PreviousValue;

    private Slider m_Slider;

    [Header("Animation")]
    [SerializeField, Range(0, 1f)] private float m_AnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve m_SliderEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine m_AnimationSliderCoroutine;

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

        if (m_AnimationSliderCoroutine != null) StopCoroutine(m_AnimationSliderCoroutine);
        m_AnimationSliderCoroutine = StartCoroutine(AnimateSlider());
    }

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
}