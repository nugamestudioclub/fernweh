using System.Collections.Generic;
using UnityEngine;

public class GizmoDebug : MonoBehaviour
{
    public static GizmoDebug Instance;

    private struct SphereRayData
    {
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public float Radius;
        public Color Color;
        public int Id;
    }

    private const int MAX_STEP_COUNT = 40;

    private int m_currentId;
    private IList<SphereRayData> m_rayData;
    // private IList<float> m_raySpawnTime;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        m_currentId = 0;
        m_rayData = new List<SphereRayData>();
        // m_raySpawnTime = new List<float>();
    }

    public int DrawSphereRay(Vector3 start, Vector3 end, float radius, Color color)
    {
        m_rayData.Add(
            new SphereRayData
            {
                StartPosition = start,
                EndPosition = end,
                Radius = radius,
                Color = color,
                Id = m_currentId++
            }
        );

        return m_currentId;
        // m_raySpawnTime.Add(Time.time);
    }

    private void OnDrawGizmos()
    {
        if (m_rayData == null) return;

        for (int i = 0; i < m_rayData.Count; ++i)
        {
            var ray_data = m_rayData[i];

            Gizmos.color = ray_data.Color;
            for (int step = 0; step < MAX_STEP_COUNT; ++step)
            {
                Gizmos.DrawWireSphere(Vector3.Lerp(ray_data.StartPosition, ray_data.EndPosition, (float)step / MAX_STEP_COUNT), ray_data.Radius);
            }
        }
    }
}
