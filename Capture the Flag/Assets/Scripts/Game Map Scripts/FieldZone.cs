using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public Bounds ZoneBoundary; // TODO: get rid of it bc it sucks
    [SerializeField] protected Team m_OwningTeam;

    public Team OwningTeam { get => m_OwningTeam; set => m_OwningTeam = value; }

}

public class FieldZone : Zone
{
    public FlagZone m_FlagZone;
    public PrisonZone m_Prison;

    public void Setup()
    {
        m_FlagZone.OwningTeam = OwningTeam;
        if (OwningTeam == GameManager.Instance.m_Teams[1]) 
            m_FlagZone.ZoneBoundary.center = new Vector3(m_FlagZone.ZoneBoundary.center.x * -1, m_FlagZone.ZoneBoundary.center.y, m_FlagZone.ZoneBoundary.center.z);
        m_Prison.OwningTeam = OwningTeam;
        if (OwningTeam == GameManager.Instance.m_Teams[1])
            m_Prison.ZoneBoundary.center = new Vector3(m_Prison.ZoneBoundary.center.x * -1, m_Prison.ZoneBoundary.center.y, m_Prison.ZoneBoundary.center.z);

        ZoneBoundary = new(transform.position, gameObject.transform.lossyScale);
    }

    public bool IsPosInField(Vector3 _point)
    {
        return ZoneBoundary.Contains(_point);
    }
}