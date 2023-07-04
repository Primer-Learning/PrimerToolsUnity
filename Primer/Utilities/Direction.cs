using UnityEngine;

namespace Primer
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Front,
        Back,
    }

    public static class DirectionExtensions
    {
        public static Vector3 ToVector(this Direction direction)
        {
            return direction switch {
                Direction.Up => Vector3.up,
                Direction.Down => Vector3.down,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                Direction.Front => Vector3.forward,
                Direction.Back => Vector3.back,
                _ => throw new System.ArgumentOutOfRangeException(nameof(direction), direction, null),
            };
        }

        public static void ApplyTo(this Direction direction, Component component)
        {
            ApplyTo(direction, component.transform);
        }

        public static void ApplyTo(this Direction direction, Transform transform)
        {
            transform.LookAt(transform.position + direction.ToVector());
        }
    }
}
