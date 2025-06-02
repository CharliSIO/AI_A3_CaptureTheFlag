using System.Collections.Generic;
using UnityEngine;

public class ToggleSwitchGroupManager : MonoBehaviour
{
    [Header("Start Value")]
    [SerializeField] private ToggleSwitch m_InitialToggle;

    [Header("ToggleOptions")]
    [SerializeField] private bool m_AllCanBeToggledOff;

    private List<ToggleSwitch> m_ToggleSwitches = new();

    private void Awake()
    {
        ToggleSwitch[] allSwitches = GetComponentsInChildren<ToggleSwitch>();
        foreach (var toggle in allSwitches)
        {
            RegisterToggleButtonToGroup(toggle);
        }
    }

    private void RegisterToggleButtonToGroup(ToggleSwitch _switch)
    {
        if (m_ToggleSwitches.Contains(_switch)) return;

        m_ToggleSwitches.Add(_switch);
        _switch.SetupForManager(this);
    }

    private void Start()
    {
        bool areAllToggledOff = true;
        foreach (var toggle in m_ToggleSwitches)
        {
            if (!toggle.m_CurrentValue) continue;

            areAllToggledOff = false;
            break;
        }

        if (!areAllToggledOff || m_AllCanBeToggledOff) return;

        if (m_InitialToggle != null) m_InitialToggle.ToggleByGroupManager(true);
        else m_ToggleSwitches[0].ToggleByGroupManager(true);
    }


    public void ToggleGroup(ToggleSwitch _toggleSwitch)
    {
        if (m_ToggleSwitches.Count <= 1) return;

        if (m_AllCanBeToggledOff && _toggleSwitch.m_CurrentValue)
        {
            foreach (var toggle in m_ToggleSwitches)
            {
                if (toggle == null) continue;

                toggle.ToggleByGroupManager(false);
            }
        }
        else
        {
            foreach (var toggle in m_ToggleSwitches)
            {
                if (toggle == null) continue;

                if (toggle == _toggleSwitch) toggle.ToggleByGroupManager(true);
                else toggle.ToggleByGroupManager(false);
            }
        }
    }
}

