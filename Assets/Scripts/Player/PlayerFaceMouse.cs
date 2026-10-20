using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFaceMouse : MonoBehaviour
{
    [SerializeField] float slerpSpeed = 100;

    private Camera _mainCamera;
    Vector2 _rawMousePos;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Vector3 rawMousePos = _rawMousePos;
        rawMousePos.z = -10;
        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(rawMousePos);

        Vector2 dir = transform.position - mousePosition;
 
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, slerpSpeed * Time.deltaTime);
    }

    public void OnLook(InputValue value)
    {
        _rawMousePos = value.Get<Vector2>();
    }
}
