#if UNITY_EDITOR
using UnityEngine;
using Runtime.ProceduralSampling;

namespace Editors.PCG
{
    [AddComponentMenu("Procedural Sampling/Poisson Disc Sampling Preview")]
    public sealed class PoissonDiscSamplingPreview : MonoBehaviour
    {
    #region Fields
        [SerializeField] private bool _updateOnChange = true;

        [Space(10)]
        [SerializeField] private int _seed = 0;
        [SerializeField] private ProceduralPoint.VisualizationType _visualization = ProceduralPoint.VisualizationType.Radii;
        [SerializeField] private ProcedualBounds _bounds = new(50, 50, 50);
        [SerializeField] private PoissonDiscSampler.Settings _poissonSampler = new
        (
            4,
            new(Space.Self, NoiseSettings.SamplingAlgorithm.None, new(0, 1), 0.8f, 1),
            25
        );

        private ProceduralPoint[] _points;
    #endregion Fields

        private void OnValidate ()
        {
            if (!_updateOnChange) return;

            _poissonSampler.Radius = Mathf.Max(2f, _poissonSampler.Radius);
            GeneratePoints();
        }

        private void OnDrawGizmos ()
        {
            _bounds.DrawRegionGizmo(transform);
            ProceduralPoint.DrawPointsGizmo(_visualization, _bounds.GetOffset(transform), ref _points);
        }

        [ContextMenu("Generate points")]
        private void GeneratePoints ()
        {
            _points = PoissonDiscSampler.GeneratePoints(_seed, _poissonSampler, _bounds.GetOffset(transform), _bounds).ToArray();
        }
    }
}
#endif