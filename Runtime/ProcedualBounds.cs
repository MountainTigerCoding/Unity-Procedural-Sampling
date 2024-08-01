using System;
using UnityEngine;

namespace Runtime.ProceduralSampling
{
    [Serializable]
    public struct ProcedualBounds
    {
    #region Fields
        public Vector3 Size;
    #endregion

        public ProcedualBounds (float xSize, float ySize, float zSize) : this(new(xSize, ySize, zSize)) {}

        public ProcedualBounds (Vector3 size)
        {
            Size = size;
        }

        public void Validate ()
        {
            float min = 0.1f;
            Size.x = Math.Max(Size.x, min);
            Size.y = Math.Max(Size.y, min);
            Size.z = Math.Max(Size.z, min);
        }

        public readonly bool IsInside (Vector2 candidate)
        {
            // candidate.y swizzles to z position in world space
            return candidate.x >= 0 && candidate.x < Size.x && candidate.y >= 0 && candidate.y < Size.z;
        }

        public readonly bool IsInsideVertical (float yPositionLocal)
        {
            return yPositionLocal >= -(Size.y / 2) && yPositionLocal <= (Size.y / 2);
        }

        public readonly Vector3 GetOffset (Transform transform) => GetOffset(transform.position);
        public readonly Vector3 GetOffset (Vector3 origin) => origin - GetOffset();
        public readonly Vector3 GetOffset () => Size / 2;

#if UNITY_EDITOR
        public readonly void DrawRegionGizmo (Transform transform)
        {
            Vector3 positionOffset = transform.position - Size / 2;
            Gizmos.DrawWireCube(positionOffset + Size / 2, Size);
        }
#endif
    }
}