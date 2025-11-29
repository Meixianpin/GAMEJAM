using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall_Destory : MonoBehaviour
{
    public GameObject button;
    private bool buttonPressed = false;
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
        if(!trigger)
        {
            buttonPressed = trigger.Press;
        }
        else
        {
            buttonPressed = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && buttonPressed)
        {
            Destroy(gameObject);
        }
    }
    private ButtonTrigger trigger;
}
