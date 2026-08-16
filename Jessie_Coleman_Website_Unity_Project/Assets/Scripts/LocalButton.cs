using UnityEngine;

public class LocalButton : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        FaceCamera();
    }

    private void FaceCamera()
    {
        if (targetCamera == null)
        {
            return;
        }

        transform.LookAt(
            transform.position + targetCamera.transform.rotation * Vector3.forward,
            targetCamera.transform.rotation * Vector3.up);
    }

    public void ButtonClicked()
    {
        Debug.Log("Local button clicked!");
    }
}