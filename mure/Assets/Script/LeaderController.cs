using UnityEngine;

public class LeaderController : MonoBehaviour
{
    private float moveSpeed = 5f;
    private float changeDirectionTime = 3f;
    private float timeSinceLastChange = 0f;
    private Vector3 randomDirection;

    void Start()
    {
        randomDirection = GetRandomDirection();
    }

    void Update()
    {
        transform.Translate(randomDirection * moveSpeed * Time.deltaTime, Space.World);

        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= changeDirectionTime)
        {
            randomDirection = GetRandomDirection();
            timeSinceLastChange = 0f;
        }
    }

    Vector3 GetRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        return new Vector3(randomX, 0f, randomZ).normalized;
    }
}
