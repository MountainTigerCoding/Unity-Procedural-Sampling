using UnityEngine;

namespace Runtime.ProceduralSampling
{
    public struct ProceduralPoint
    {
    #region Fields
        public readonly Vector3 Position;
        public readonly float Radius;
    #endregion Field

        public ProceduralPoint (Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

#if UNITY_EDITOR
    #region Visualization
        public enum VisualizationType
        {
            None,
            Radii,
            Density,
        }

        public static void DrawPointsGizmo (VisualizationType visualization, Vector3 wsOffset, ref ProceduralPoint[] points)
        {
            if (visualization == VisualizationType.None) return;
            if (points == null) return;
            if (points.Length == 0) return;

            int maxPoints = 2000;
            if (points.Length > maxPoints) {
                Debug.Log("Could not draw gizmos 'Poisson Disc Points' as the request has exeeded " + maxPoints + " points.");
                return;
            }
            
            foreach (ProceduralPoint point in points)
            {
                float radius = point.Radius / 5;
                Color color = visualization switch
                {
                    VisualizationType.Density => Color.Lerp(Color.red, Color.white, point.Radius / 8.2f),
                    _ => Color.white,
                };
                Gizmos.color = color;
                Gizmos.DrawSphere(wsOffset + point.Position, radius);
            }
        }
    #endregion Visualizations
#endif
    }
}