using UnityEngine;

public class owlroar : MonoBehaviour
{
    public float maxAngle = 90f;
    public float speedAtZero = 5f;     // 0° 时的速度(deg/s)，防止卡死
    public float speedAtMax = 200f;    // 90° 时的速度(deg/s)

    private float angle = 0f;
    private int dir = 1;               // 1: 0→90, -1: 90→0

    void Update()
    {
        float k = angle / maxAngle;                          // 0~1
        float speed = Mathf.Lerp(speedAtZero, speedAtMax, k); // 角度越大越快

        angle += dir * speed * Time.deltaTime;

        if (angle >= maxAngle) { angle = maxAngle; dir = -1; }
        else if (angle <= 0f)  { angle = 0f;       dir =  1; }

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }
}