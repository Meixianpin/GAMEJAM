using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier_Destory : MonoBehaviour
{
    public GameObject button;

    private void Start()
    {
        // 获取按钮触发器组件
        trigger = button.GetComponent<ButtonTrigger>();
        if (trigger == null)
        {
            Debug.LogError("ButtonTrigger component not found on the button GameObject.");
        }
    }
    
    private void Update()
    {
        if (trigger.Press)
        {
            Destroy(gameObject);
        }
    }
    private ButtonTrigger trigger;
}
