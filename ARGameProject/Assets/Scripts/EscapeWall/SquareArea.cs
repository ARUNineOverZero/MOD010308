using UnityEngine;

namespace EscapeWall
{
    public struct SquareArea
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public Vector3 Diagonal
        {
            get
            {
                Vector3 d = pointA - pointB;
                d.y = 0f;
                return d;
            }
        }

        public float DiagonalLength => Diagonal.magnitude;

        public float SideLength => DiagonalLength / Mathf.Sqrt(2f);

        public Vector3 Center
        {
            get
            {
                Vector3 c = (pointA + pointB) * 0.5f;
                c.y = pointA.y + 0.01f;
                return c;
            }
        }

        public Vector3 Forwards
        {
            get
            {
                if (DiagonalLength < 0.001f)
                    return Vector3.forward;

                Vector3 forwards = Quaternion.Euler(0f, 45f, 0f) * Diagonal.normalized;
                forwards.y = 0f;
                return forwards.normalized;
            }
        }

        public Quaternion Rotation => Quaternion.LookRotation(Forwards, Vector3.up);

        public Vector3 PreviewScale => new Vector3(SideLength / 10f, 1f, SideLength / 10f);

        public bool IsTooSmall(float minSize) => SideLength < minSize;

        public bool IsValid => DiagonalLength >= 0.001f;
    }
}