using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Zone : MonoBehaviour
{
    [SerializeField] private Bounds m_ZoneBounds;
    [SerializeField] private Team m_OwningTeam;

    private void Awake()
    {
        m_ZoneBounds = new();
    }
    public Bounds ZoneBoundary { get => m_ZoneBounds; set => m_ZoneBounds = value; }
    public Team OwningTeam { get => m_OwningTeam; set => m_OwningTeam = value; }

}

public class FieldZone : Zone
{
    public FlagZone m_FlagZone;
    public PrisonZone m_Prison;

    public bool IsPosInField(Vector3 _point)
    {
        return ZoneBoundary.Contains(_point);
    }
}