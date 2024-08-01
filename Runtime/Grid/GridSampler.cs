using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.ProceduralSampling
{
    public static class GridSampler
    {
        [Serializable]
        public struct Settings
        {
        #region Fields
            public float CellSize;
        #endregion

            public Settings (float cellSize)
            {
                CellSize = cellSize;
            }

            public void OnValidate ()
            {
                CellSize = Mathf.Max(0.5f, CellSize);
            }
        }

        public static List<ProceduralPoint> GeneratePoints (Settings settings, ProcedualBounds bounds)
        {
            return new();
        }
    }
}