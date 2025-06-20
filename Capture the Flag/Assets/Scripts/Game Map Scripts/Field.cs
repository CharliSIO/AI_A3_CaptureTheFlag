using System.Collections.Generic;
using UnityEngine;

// this is the whole field
// it has the field zones
// which have their subzones
public class Field : MonoBehaviour
{
    [SerializeField] private int m_iZoneCount = 2;
    [SerializeField] private FieldZone m_FieldZonePrefab; // Assign in Inspector

    public List<FieldZone> m_FieldZones = new();

    private void Awake()
    {
        GameManager.Instance.Field = this;

        m_iZoneCount = GameManager.Instance.TeamCount; // 2 or 4 fields?

        for (int i = 0; i < m_iZoneCount; i++)
        {
            FieldZone zoneInstance = Instantiate(m_FieldZonePrefab, transform); // make them all
            zoneInstance.name = $"Zone_{i}";

            // move the field, flip it around if necessary
            zoneInstance.transform.localPosition = new Vector3(
                (i == 1 || i == 3 ? 0.25f : -0.25f),
                (m_iZoneCount > 2 ? (i == 2 || i == 3 ? -0.25f : 0.25f) : 0.0f),
                0.0f
            );
            zoneInstance.transform.localScale = new(0.5f, m_iZoneCount > 2 ? 0.5f : 1.0f, 1.0f);
            if (i == 1 || i == 3) zoneInstance.transform.localScale = new(zoneInstance.transform.localScale.x * -1, zoneInstance.transform.localScale.y * -1, 1.0f);

            // set the colours and the owning team and make the zones set themselves up
            zoneInstance.GetComponent<SpriteRenderer>().color = GameManager.Instance.m_TeamColourList[i];
            zoneInstance.OwningTeam = GameManager.Instance.m_Teams[i];
            zoneInstance.OwningTeam.m_Zone = zoneInstance;
            zoneInstance.Setup();

            m_FieldZones.Add(zoneInstance); // its in the list now!
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
