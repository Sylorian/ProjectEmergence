using UnityEngine;

public class CharacterMoveControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool flipByDirection = true;

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        transform.Translate(Vector3.right * -inputX * moveSpeed * Time.deltaTime);
        
        if (flipByDirection && inputX != 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(inputX);
            transform.localScale = scale;
        }
    }
}