using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    private float moveSpeed = 5f;
    private float lookSpeedX = 2f;
    private float lookSpeedY = 2f;
    private float minY = -40f;
    private float maxY = 80f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        //スピード上昇
        float currentMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentMoveSpeed *= 2f;
        }

        //カメラリセット
        if (Input.GetMouseButton(1))
        {
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
            rotationY += Input.GetAxis("Mouse X") * lookSpeedX;
            rotationX = Mathf.Clamp(rotationX, minY, maxY);

            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        float moveForwardBackward = Input.GetAxis("Vertical") * currentMoveSpeed * Time.deltaTime;
        float moveLeftRight = Input.GetAxis("Horizontal") * currentMoveSpeed * Time.deltaTime;

        transform.Translate(moveLeftRight, 0, moveForwardBackward);

        //下降
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(0, -currentMoveSpeed * Time.deltaTime, 0);
        }

        //上昇
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(0, currentMoveSpeed * Time.deltaTime, 0);
        }
    }
}
