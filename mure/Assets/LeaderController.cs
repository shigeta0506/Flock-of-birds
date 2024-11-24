using UnityEngine;

public class LeaderController : MonoBehaviour
{
    private float moveSpeed = 5f;    //移動速度
    private float changeDirectionTime = 3f;  //方向転換の間隔
    private float timeSinceLastChange = 0f;
    private Vector3 randomDirection;

    void Start()
    {
        //最初のランダムな方向を設定
        randomDirection = GetRandomDirection();
    }

    void Update()
    {
        //リーダーをランダムな方向に移動させる
        transform.Translate(randomDirection * moveSpeed * Time.deltaTime, Space.World);

        //一定時間経過でランダムな方向を変更
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= changeDirectionTime)
        {
            randomDirection = GetRandomDirection();
            timeSinceLastChange = 0f;
        }
    }

    //ランダムな方向を返す関数
    Vector3 GetRandomDirection()
    {
        //ランダムな方向（XZ平面）
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        return new Vector3(randomX, 0f, randomZ).normalized;
    }
}
