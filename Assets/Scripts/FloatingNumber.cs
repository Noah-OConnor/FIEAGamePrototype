using TMPro;
using UnityEngine;

public class FloatingNumber : MonoBehaviour
{
    [SerializeField] private float speed = 1f; // Speed at which the number will move up
    [SerializeField] private float lifetime = 1f; // Time after which the number will disappear

    private TMP_Text text;
    private Camera mainCamera;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        mainCamera = Camera.main; // Get the main camera
    }

    public void SetNumber(int number)
    {
        text.text = number.ToString();
    }

    private void Update()
    {
        transform.position += new Vector3(0, speed * Time.deltaTime, 0);

        // Make the number always face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
