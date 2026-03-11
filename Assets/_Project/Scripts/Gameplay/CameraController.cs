using UnityEngine;

namespace Gryd.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -8f);
        [SerializeField] private float _smoothSpeed = 0f; // 0 = sin lerp, >0 = suavizado

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = _target.position + _offset;
            desired.y = transform.position.y + _offset.y; // mantener altura del offset, ignorar salto

            transform.position = _smoothSpeed > 0f
                ? Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime)
                : desired;
        }
    }
}
