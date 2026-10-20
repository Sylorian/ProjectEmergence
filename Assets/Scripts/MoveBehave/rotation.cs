using UnityEngine;

public class rotation : MonoBehaviour
{
    [Tooltip("旋转速度，单位：度/秒。正值逆时针，负值顺时针")]
    public float rotateSpeed = 90f;

    void Update()
    {
        // 每帧绕 Z 轴旋转 rotateSpeed * deltaTime 度
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}