using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Tooltip("Целевой объект, вокруг которого вращается камера.")]
    public Transform target;

    [Tooltip("Чувствительность вращения мышью.")]
    public float mouseSensitivity = 10f;

    [Tooltip("Расстояние от камеры до целевого объекта.")]
    public float distance = 5.0f;

    [Tooltip("Плавность вращения камеры.")]
    public float rotationSmoothTime = 0.12f;

    [Tooltip("Минимальный угол наклона камеры (вверх).")]
    public float minVerticalAngle = -80f;

    [Tooltip("Максимальный угол наклона камеры (вниз).")]
    public float maxVerticalAngle = 80f;

    [Tooltip("Коэффициент затухания инерции вращения.")]
    [Range(0.01f, 0.99f)]
    public float inertiaDamping = 0.8f;  

    private Vector3 currentRotation;
    private Vector3 smoothVelocity;
    private float yaw;
    private float pitch;
    private Vector2 previousMousePosition; 
    private Vector2 mouseDelta; 

    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (target == null)
            {
                target = GameObject.Find("Player")?.transform;
                if (target == null)
                {
                    Debug.LogWarning("CameraOrbit: Target not set!");
                    enabled = false;
                    return;
                }

            }
        }

        Vector3 eulerAngles = transform.eulerAngles;
        yaw = eulerAngles.y;
        pitch = eulerAngles.x;
        RotateCamera();
        previousMousePosition = Input.mousePosition; 
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1))
        {
            
            Vector2 currentMousePosition = Input.mousePosition;
            
            mouseDelta = currentMousePosition - previousMousePosition;

            yaw += mouseDelta.x * mouseSensitivity * Time.deltaTime * 50f;  
            pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime * 50f;


            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            
            previousMousePosition = currentMousePosition;
            RotateCamera();

        }
        else
        {
            
            mouseDelta *= inertiaDamping; 
                                         
            if (mouseDelta.magnitude < 0.01f) 
            {
                mouseDelta = Vector2.zero;
                smoothVelocity = Vector3.zero; 
            }
            yaw += mouseDelta.x * mouseSensitivity * Time.deltaTime * 50f;
            pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime * 50f;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            RotateCamera();


        }

        
        if (!Input.GetMouseButton(1)) 
            previousMousePosition = Input.mousePosition;

    }

    private void RotateCamera()
    {
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref smoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;
        transform.position = target.position - (transform.rotation * Vector3.forward * distance);

    }
}