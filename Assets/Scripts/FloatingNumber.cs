using TMPro;
using UnityEngine;

public class FloatingNumber : MonoBehaviour
{
    [SerializeField] private float speed = 1f; // Speed at which the number will move up
    [SerializeField] private float lifetime = 1f; // Time after which the number will disappear
    private float xSpeed;
    private float zSpeed;

    private TMP_Text text;
    private Transform mainCamera;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        mainCamera = GameManager.instance.cameraMain; // Get the main camera

        xSpeed = Random.Range(-speed, speed);
        zSpeed = Random.Range(-speed, speed);
    }

    public void SetNumber(int number)
    {
        text.text = number.ToString();
    }

    private void Update()
    {
        transform.position += new Vector3(xSpeed * Time.deltaTime, speed * Time.deltaTime, zSpeed * Time.deltaTime);

        // Make the number always face the camera
        transform.LookAt(transform.position + mainCamera.rotation * Vector3.forward,
            mainCamera.rotation * Vector3.up);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
