using UnityEngine;

public class CityClock : MonoBehaviour
{
    public static CityClock Instance { get; private set; }
    [Header("One full day in real seconds")]
    public float dayLengthSeconds = 600f;
    public float startHour = 6f;
    public float CurrentHour { get; private set; }

    private float elapsed;

    private void Awake()
    {
        Instance = this;
        CurrentHour = startHour;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float normalized = (elapsed % dayLengthSeconds) / dayLengthSeconds;
        CurrentHour = (startHour + normalized * 24f) % 24f;
    }
}
