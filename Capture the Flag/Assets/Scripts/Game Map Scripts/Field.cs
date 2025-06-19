using System.Collections.Generic;
using UnityEngine;


public class Field : MonoBehaviour
{
    [SerializeField] private int m_iZoneCount = 2;
    [SerializeField] private FieldZone m_FieldZonePrefab; // Assign in Inspector

    public List<FieldZone> m_FieldZones = new();

    private void Awake()
    {
        GameManager.Instance.Field = this;

        m_iZoneCount = GameManager.Instance.TeamCount;

        for (int i = 0; i < m_iZoneCount; i++)
        {
            FieldZone zoneInstance = Instantiate(m_FieldZonePrefab, transform);
            zoneInstance.name = $"Zone_{i}";

            zoneInstance.transform.localPosition = new Vector3(
                (i == 1 || i == 3 ? 0.25f : -0.25f),
                (m_iZoneCount > 2 ? (i == 2 || i == 3 ? -0.25f : 0.25f) : 0.0f),
                0.0f
            );
            zoneInstance.transform.localScale = new(0.5f, m_iZoneCount > 2 ? 0.5f : 1.0f, 1.0f);
            if (i == 1 || i == 3) zoneInstance.transform.localScale = new(zoneInstance.transform.localScale.x * -1, zoneInstance.transform.localScale.y, 1.0f);
            if (i == 2 || i == 3) zoneInstance.transform.localScale = new(zoneInstance.transform.localScale.x, zoneInstance.transform.localScale.y * -1, 1.0f);

            zoneInstance.GetComponent<SpriteRenderer>().color = GameManager.Instance.m_TeamColourList[i];

            zoneInstance.OwningTeam = GameManager.Instance.m_Teams[i];
            zoneInstance.OwningTeam.m_Zone = zoneInstance;
            zoneInstance.Setup();

            m_FieldZones.Add(zoneInstance);
        }
    }

    public FieldZone GetTeamZone(Team _team)
    {
        foreach (var t in m_FieldZones)
        {
            if (t.OwningTeam == _team) return t;
        }

        return m_FieldZones[0];
    }
}
