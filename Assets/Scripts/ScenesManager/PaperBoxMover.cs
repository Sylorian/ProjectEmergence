
using UnityEngine;


public class PaperBoxMover : MonoBehaviour
{
    private bool paperPosition = false;
    private bool IsOpen = false;

    private Transform PaperMover;

    private void Start()
    {
        PaperMover = ScenePropertyRegistry.GetInstance<Transform>("0006");
    }

    void Update()
    {

        // 按键只负责切换状态
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            paperPosition = !paperPosition;
        }

        // 每帧朝当前目标逐步移动
        Vector3 target = paperPosition
            ? new Vector3(-0.11f, -7f, -5f)
            : new Vector3(-0.11f, 0f, -5f);

        PaperMover.localPosition = Vector3.MoveTowards(
            PaperMover.localPosition, target, 20 * Time.deltaTime);
    }
}