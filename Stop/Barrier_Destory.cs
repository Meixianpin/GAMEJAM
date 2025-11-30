using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier_Destory : MonoBehaviour
{
    public int ChooseButton_PressWay=0;  // 选择按钮按下方式 0:任意 1:或 2:且
    public GameObject button;

    private ButtonTrigger buttonTrigger;
    private Barrier_get_button_press barrier_Get_Button_Press;
    private bool triggersPressed = false; // 按钮按下状态


    private void Start()
    {
        if(button!=null)
        {
            buttonTrigger = button.GetComponent<ButtonTrigger>();
        }
        else
        {
            barrier_Get_Button_Press = GetComponent<Barrier_get_button_press>();
            // 获取按钮触发器组件
            if (barrier_Get_Button_Press == null)
            {
                Debug.LogError("barrier_Get_Button_Press component not found on the button GameObject.");
            }
        }
    }
    
    private void Update()
    {

        triggersPressed= Choose_PressWay();
        if (triggersPressed)
        {
            Destroy(gameObject);
        }
    }

    private bool Choose_PressWay()
    {
        if (buttonTrigger!=null)
        {
            return buttonTrigger.Press;
        }
        else
        {
            if (ChooseButton_PressWay == 0)
            {
                return barrier_Get_Button_Press.AnyButtonsPressed();
            }
            else if (ChooseButton_PressWay == 1)
            {
                return barrier_Get_Button_Press.ORButtonsPressed();
            }
            else if (ChooseButton_PressWay == 2)
            {
                return barrier_Get_Button_Press.AndButtonsPressed();
            }
            else
            {
                Debug.LogError("Invalid ChooseButton_PressWay value. It should be 0, 1, or 2.");
                return false;
            }
        }   
    }

    private ButtonTrigger trigger;
}
