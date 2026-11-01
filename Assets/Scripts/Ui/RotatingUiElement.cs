using UnityEngine;

namespace Hyperplasia.View
{
    public class RotatingUiElement : MonoBehaviour
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }
        public enum RotationDirection
        {
            Clockwise = -1,
            CounterClockwise = 1
        }
        
        public RotationAxis rotationAxis = RotationAxis.Z;
        public float rotationSpeed = 50f;
        public RotationDirection rotationDirection = RotationDirection.Clockwise;

        void Update()
        {
            switch (rotationAxis)
            {
                case RotationAxis.X:
                    transform.Rotate(Vector3.right, (int)rotationDirection * rotationSpeed * Time.deltaTime);
                    break;
                case RotationAxis.Y:
                    transform.Rotate(Vector3.up, (int)rotationDirection * rotationSpeed * Time.deltaTime);
                    break;
                case RotationAxis.Z:
                    transform.Rotate(Vector3.forward, (int)rotationDirection * rotationSpeed * Time.deltaTime);
                    break;
            }
        }
    }
}