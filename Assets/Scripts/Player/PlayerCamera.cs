using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private float lookSpeed = 10f;
    private Vector2 currentLook;
    private Vector2 lookInput;
    [SerializeField] private Transform cineCamera;
    [SerializeField] private Vector2 lookLimits = new Vector2(-90f, 90f);


    private void Update()
    {
        // Get the look input from the InputManager
        lookInput = InputManager.instance.Look;

        // Scale the look input by the look speed and the time since the last frame
        lookInput *= lookSpeed * Time.deltaTime;

        // Update the current look
        currentLook.x = Mathf.Clamp(currentLook.x - lookInput.y, lookLimits.x, lookLimits.y);
        currentLook.y += lookInput.x;

        // Apply the current look to the camera's rotation
        cineCamera.transform.eulerAngles = currentLook;
    }

    public void SetLookSPeed(float newLookSpeed)
    {
        lookSpeed = newLookSpeed;
    }
}
