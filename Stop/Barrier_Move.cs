using UnityEngine;

public class Barrier_Move : MonoBehaviour
{
    [Header("移动设置")]     
    public Vector3 moveOffset = new Vector3(0, 2f, 0); // 移动偏移量，可在Inspector中自定义
    public float moveSpeed = 2f;        // 移动速度
    public bool willreturn = true;     // 是否返回原始位置

    private Vector3 originalPosition;   // 原始位置
    private Vector3 targetPosition;     // 目标位置
    private bool triggersPressed = false; // 按钮按下状态
    private bool isMoving = false;      // 是否正在移动

    private Barrier_get_button_press barrier_Get_Button_Press;

    // Start is called before the first frame update
    void Start()
    {
        // 获取按钮触发器组件
        barrier_Get_Button_Press= GetComponent<Barrier_get_button_press>();
      

        // 记录原始位置
        originalPosition = transform.position;
        // 计算目标位置
        targetPosition = originalPosition + moveOffset;
    }

    // Update is called once per frame
    void Update()
    {
        //有问题
        triggersPressed = barrier_Get_Button_Press.AnyButtonsPressed();
        if ((triggersPressed && !isMoving)||(isMoving&&!willreturn))
        {
            // 当按钮按下时，移动到目标位置
            MoveToPosition(targetPosition);
        }
        else if (!triggersPressed&&willreturn)
        {
            // 当按钮松开时，移动回原始位置
            MoveToPosition(originalPosition);
        }

        // 持续移动
        if (isMoving)
        {
            ContinueMoving();
        }
    }

    /// <summary>
    /// 开始移动到指定位置
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    void MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;
        // 这里可以添加移动开始的回调或效果
    }

    /// <summary>
    /// 持续移动逻辑
    /// </summary>
    void ContinueMoving()
    {
        Vector3 currentTarget;

        // 根据按钮状态决定移动方向
        if (triggersPressed)
        {
            currentTarget = targetPosition;
        }
        else
        {

            currentTarget = originalPosition;
        }

        // 使用插值平滑移动
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            moveSpeed * Time.deltaTime
        );

        // 检查是否到达目标位置
        if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
        {
            transform.position = currentTarget; // 确保精确到达
            isMoving = false;
        }
    }

    /// <summary>
    /// 自定义移动函数 - 可以外部调用
    /// </summary>
    /// <param name="offset">移动偏移量</param>
    /// <param name="speed">移动速度</param>
    public void Move(Vector3 offset, float speed = -1)
    {
        // 如果指定了新的速度，则更新
        if (speed > 0)
        {
            moveSpeed = speed;
        }

        // 更新移动偏移量
        moveOffset = offset;
        // 重新计算目标位置
        targetPosition = originalPosition + moveOffset;

        // 根据当前按钮状态决定移动方向
        if (triggersPressed)
        {
            MoveToPosition(targetPosition);
        }
        else
        {
            MoveToPosition(originalPosition);
        }
    }

    /// <summary>
    /// 在Inspector中可视化移动范围
    /// </summary>
    /// 
    void OnDrawGizmosSelected()
    {
        // 绘制原始位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(originalPosition, Vector3.one * 0.5f);

        // 绘制目标位置
        Gizmos.color = Color.red;
        Vector3 targetPos = Application.isPlaying ? targetPosition : transform.position + moveOffset;
        Gizmos.DrawWireCube(targetPos, Vector3.one * 0.5f);

        // 绘制移动路径
        Gizmos.color = Color.yellow;
        Vector3 startPos = Application.isPlaying ? originalPosition : transform.position;
        Gizmos.DrawLine(startPos, targetPos);
    }
}