# Role文件介绍
### 属性结构（主要在RoleController内）
1. **一般属性** : 
- 移动速度系数moveSpeed
- 鼠标灵敏度mouseSensitivity
- 跳跃力度jumpForce
- 是否进入特殊空间isPaused（false为未进入）
-是否可以按下K功能键 canUseK（false时按下无反应）

2. **特殊属性**：
- 
### 文件介绍：
**RoleController.cs**：角色直接的控制器
通过鼠标移动控制左右，左键按下跳跃，目前支持单段跳，更新可跳跃次数需要通过Tag=‘Jumpable’的地块碰撞体检测后才能重新获得跳跃次数。J键进入只有自己可以移动的特殊空间，场景除自己以外的对象全部暂停，此时K键可以使用。K键在特殊空间内按下后记录当前的位置并创造一份复制体，随后回溯至按下J的位置，时间再次流动。R键重置，回到最初的位置。
-Get_XX获取XX的值 Set_XX设置XX的值