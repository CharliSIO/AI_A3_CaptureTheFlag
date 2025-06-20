using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public Rect ZoneBoundary; // TODO: get rid of it bc it sucks
    [SerializeField] protected Team m_OwningTeam;

    public Team OwningTeam { get => m_OwningTeam; set => m_OwningTeam = value; }

}

public class FieldZone : Zone
{
    public FlagZone m_FlagZone;
    public PrisonZone m_Prison;

    public void Setup()
    {
        if (TryGetComponent<Renderer>(out var renderer))
        {
            Bounds bounds = renderer.bounds;

            Vector2 size2D = new Vector2(bounds.size.x, bounds.size.y);
            Vector2 center2D = new Vector2(bounds.center.x, bounds.center.y);
            Vector2 min = center2D - size2D / 2f;

            ZoneBoundary = new Rect(min, size2D);
        }
    }

    public bool IsPosInField(Vector3 _point)
    {
        return ZoneBoundary.Contains(_point);
    }
}