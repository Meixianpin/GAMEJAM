using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier_get_button_press : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] buttons;           // 按钮对象

    private ButtonTrigger[] triggers;

    private bool triggersPressed = false; // 按钮按下状态
    void Start()
    {
        triggers = new ButtonTrigger[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                triggers[i] = buttons[i].GetComponent<ButtonTrigger>();
                if (triggers[i] == null)
                {
                    Debug.LogError($"ButtonTrigger component not found on button GameObject at index {i}.");
                }
            }
            else
            {
                Debug.LogError($"Button GameObject at index {i} is not assigned.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        AnyButtonsPressed();
        AndButtonsPressed();
        ORButtonsPressed();
    }

    public bool AnyButtonsPressed()
    {
        foreach (var trigger in triggers)
        {
            if (trigger != null && trigger.Press)
            {
                return true;
            }
        }
        return false;
    }//任意
    public bool AndButtonsPressed()
    {
        foreach (var trigger in triggers)
        {
            if (trigger == null || !trigger.Press)
            {
                return false;
            }
        }
        return true;
    }//同时
    public bool ORButtonsPressed()
    {
        int count = 0;
        foreach (var trigger in triggers)
        {
            if (trigger != null &&trigger.Press)
            {
                count++;
            }
        }
        if(count==1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }//有且仅有唯一

}
