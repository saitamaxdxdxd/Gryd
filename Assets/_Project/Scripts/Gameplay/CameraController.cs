using UnityEngine;

namespace Gryd.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 0f; // 0 = sin lerp, >0 = suavizado

        private Vector3 _offset;

        private void Start()
        {
            if (_target == null)
            {
                Debug.LogError("[CameraController] Target no asignado.", this);
                return;
            }

            _offset = transform.position - _target.position;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = _target.position + _offset;
            desired.y = transform.position.y; // ignorar el salto en Y

            transform.position = _smoothSpeed > 0f
                ? Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime)
                : desired;
        }
    }
}
