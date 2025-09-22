using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Velocidad de movimiento en unidades/segundo")]
    public float moveSpeed = 6f;
    [Tooltip("Multiplicador para sprint (opcional). Si no lo querés, dejalo 1.")]
    public float sprintMultiplier = 1.5f;

    [Header("Mouse Look")]
    [Tooltip("Sensibilidad del mouse (x = yaw, y = pitch)")]
    public Vector2 mouseSensitivity = new Vector2(200f, 200f);
    [Tooltip("Límite vertical de la cámara (grados)")]
    public float minPitch = -85f;
    public float maxPitch = 85f;
    [Tooltip("Suavizado de rotación (0 = sin suavizado)")]
    [Range(0f, 0.2f)]
    public float lookSmoothTime = 0.03f;

    [Header("References")]
    [Tooltip("Referencia a la cámara (child normalmente)")]
    public Transform playerCamera;

    // Internals
    Rigidbody rb;
    Vector3 velocityInput;         // input en espacio local (x,z)
    float currentYaw;              // rotación Y del jugador (degrees)
    float currentPitch;            // rotación X de la cámara (degrees)

    // Para suavizado del mouse
    Vector2 currentMouseDelta;
    Vector2 smoothMouseVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Recomendado: freeze rotaciones de Rigidbody (usaremos MoveRotation)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (playerCamera == null)
        {
            Debug.LogError("PlayerControllerFPS: playerCamera no asignada. Asignala en el inspector (child Camera).");
        }

        // Inicializamos yaw/pitch con la rotación actual
        currentYaw = transform.eulerAngles.y;
        if (playerCamera != null)
            currentPitch = playerCamera.localEulerAngles.x;
    }

    void Start()
    {
        // Bloquear cursor al iniciar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Input de movimiento ---
        float inputX = Input.GetAxisRaw("Horizontal"); // A, D
        float inputZ = Input.GetAxisRaw("Vertical");   // W, S

        Vector3 rawInput = new Vector3(inputX, 0f, inputZ).normalized;

        // Sprint opcional con Left Shift
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        // Convertir input local (forward/right) a velocidad en world space, pero la aplicamos en FixedUpdate
        velocityInput = rawInput * speed;

        // --- Mouse look ---
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Suavizado (opcional)
        if (lookSmoothTime > 0f)
        {
            currentMouseDelta.x = Mathf.SmoothDamp(currentMouseDelta.x, mouseDelta.x, ref smoothMouseVelocity.x, lookSmoothTime);
            currentMouseDelta.y = Mathf.SmoothDamp(currentMouseDelta.y, mouseDelta.y, ref smoothMouseVelocity.y, lookSmoothTime);
        }
        else
        {
            currentMouseDelta = mouseDelta;
        }

        // Aplicar sensibilidad
        float yawDelta = currentMouseDelta.x * mouseSensitivity.x * Time.deltaTime;
        float pitchDelta = currentMouseDelta.y * mouseSensitivity.y * Time.deltaTime;

        currentYaw += yawDelta;
        currentPitch -= pitchDelta; // invertimos Y para comportamiento estándar

        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // Rotación del jugador (yaw) se aplica con Rigidbody.MoveRotation en FixedUpdate,
        // pero podemos rotar visualmente aquí para respuesta inmediata:
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        // Rotación de la cámara (pitch)
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

        // Salir con ESC desbloquea cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Movimiento relativo a la orientación del jugador (transform.forward / right)
        Vector3 move = transform.right * velocityInput.x + transform.forward * velocityInput.z;

        // Mantener componente Y del rigidbody (si hay gravedad)
        Vector3 newPosition = rb.position + move * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);

        // Mantener rotación del Rigidbody consistente con transform.rotation
        rb.MoveRotation(Quaternion.Euler(0f, currentYaw, 0f));
    }

    // Método público para cambiar sensibilidad en runtime (si querés UI)
    public void SetMouseSensitivity(float x, float y)
    {
        mouseSensitivity = new Vector2(x, y);
    }
}
