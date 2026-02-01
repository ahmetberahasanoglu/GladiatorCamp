using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (_mainCamera != null)
        {

            transform.rotation = _mainCamera.transform.rotation;
        }
    }
}