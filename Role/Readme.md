# 属性结构（主要在 RoleController 内）
1. ## 一般属性 ## :
- 材质系统相关：当前材质类型（currentMaterial）、各材质对应的预制体（CloudPrefab等）、各材质对应的 Sprite（CloudSprite等）
- 移动设置：移动速度系数（moveSpeed）
- 跳跃设置：跳跃力度（jumpForce）、可跳跃碰撞体标签（groundTag）
- 状态存储：角色当前位置 / 旋转 / 速度、J 键记录的位置 / 旋转 / 速度 / 材质、状态记录标记（isStateRecorded）
- 组件引用：刚体组件（rb）、Sprite 渲染器（spriteRenderer）、地面检测标记（isGrounded）等
- 冷却设置：K 键生成功能冷却时间（spawnCooldown）
2. ## 特殊属性 ##：
- 特殊能力设置：二段跳开关（doubleJumpEnabled）、二段跳力度比例（doubleJumpForceRatio）、Honey 材质摩擦力（honeyFriction）、Slime 材质反弹力度（slimeBounceForce）、Slime 最小下落速度（slimeMinFallSpeed）
- 材质映射字典：材质 - 预制体映射（materialPrefabDict）、材质 - Sprite 映射（materialSpriteDict）
- 特殊能力状态：二段跳使用标记（hasJumped）、原始摩擦力（originalDrag）、Slime 反弹标记（isSlimeBouncing）
- 冷却相关变量：上一次生成时间（lastSpawnTime）
3. ## 文件介绍 ##：
- ## RoleController.cs ## ：角色直接的控制器，负责角色的核心逻辑控制
- Get_XX/Set_XX 方法：Get_moveSpeed()/Set_moveSpeed()获取 / 设置移动速度；Get_jumpForce()/Set_jumpForce()获取 / 设置跳跃力度
- 材质系统：支持 Cloud、Slime、Dirt、Stone、Sand、Honey 六种材质，每种材质有对应的预制体和 Sprite，具备不同的物理特性
- 核心功能：
- 角色移动与跳跃（支持 Cloud 材质二段跳）
- J 键记录状态、K 键生成对应材质预制体（带冷却机制）
- R 键重置角色到起始位置
- 材质特性实现（Honey 高摩擦力、Slime 自动反弹等）
- 地面检测与碰撞响应