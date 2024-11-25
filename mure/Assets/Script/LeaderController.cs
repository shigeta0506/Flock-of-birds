using UnityEngine;

public class LeaderController : MonoBehaviour
{
    private float moveSpeed = 5f;    //�ړ����x
    private float changeDirectionTime = 3f;  //�����]���̊Ԋu
    private float timeSinceLastChange = 0f;
    private Vector3 randomDirection;

    void Start()
    {
        //�ŏ��̃����_���ȕ�����ݒ�
        randomDirection = GetRandomDirection();
    }

    void Update()
    {
        //���[�_�[�������_���ȕ����Ɉړ�������
        transform.Translate(randomDirection * moveSpeed * Time.deltaTime, Space.World);

        //��莞�Ԍo�߂Ń����_���ȕ�����ύX
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= changeDirectionTime)
        {
            randomDirection = GetRandomDirection();
            timeSinceLastChange = 0f;
        }
    }

    //�����_���ȕ�����Ԃ��֐�
    Vector3 GetRandomDirection()
    {
        //�����_���ȕ����iXZ���ʁj
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        return new Vector3(randomX, 0f, randomZ).normalized;
    }
}
