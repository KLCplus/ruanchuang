using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public SimpleMobileController joystick; // 引用摇杆

    [Header("移动设置")]
    public float moveSpeed = 5f;          // 固定的移动速度
    public float mouseSensitivity = 100f; // 视角灵敏度
    public float mobileRotationFactor = 0.05f;

    [Header("飞行限制")]
    public float minHeight = 0.5f; // 最低飞行高度 (地板高度+0.5)

    [Header("点击设置")]
    public float clickThreshold = 0.2f;
    public float dragThreshold = 20f;

    private Transform cam;
    private float verticalRotation = 0f;

    // --- 状态变量 ---
    private bool isPressingMouse = false;
    private float mousePressStartTime = 0f;
    private Vector3 mousePressStartPos;
    private int lookFingerId = -1;
    private bool isTouchingUI = false;

    // --- 手机飞行按钮状态 ---
    private bool isMobileUp = false;
    private bool isMobileDown = false;

    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovement();
        HandleCameraLook();
        HandleInteraction();
    }

    // --- 给 UI 按钮绑定的方法 ---
    public void SetMobileUp(bool active) { isMobileUp = active; }
    public void SetMobileDown(bool active) { isMobileDown = active; }

    // ================= 纯净版移动逻辑 ==================
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float upDown = 0f;

        // 1. 获取平面输入 (摇杆 + 键盘WASD)
        if (joystick != null)
        {
            Vector3 joyDir = joystick.GetInputDirection();
            h += joyDir.x;
            v += joyDir.z;
        }

        // 2. 垂直升降 (仅限 UI按钮 或 调试键E/Q)
        // 已删除 Space 和 Shift 的相关逻辑
        if (isMobileUp || Input.GetKey(KeyCode.E))
            upDown = 1f;

        if (isMobileDown || Input.GetKey(KeyCode.Q))
            upDown = -1f;

        // 3. 计算移动
        if (cam != null)
        {
            Vector3 forward = cam.forward;
            Vector3 right = cam.right;

            // 混合方向 (平面 + 垂直)
            Vector3 moveDir = (right * h + forward * v).normalized;
            moveDir += Vector3.up * upDown;

            // 4. 执行移动 (无加速逻辑)
            Vector3 targetPosition = transform.position + moveDir * moveSpeed * Time.deltaTime;

            // 5. 高度限制 (空气墙)
            if (targetPosition.y < minHeight)
            {
                targetPosition.y = minHeight;
            }

            transform.position = targetPosition;
        }
    }

    // ================= 交互逻辑 (保持修复版) ==================
    void HandleInteraction()
    {
        // 按下
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) { isTouchingUI = true; isPressingMouse = false; return; }

            // 避开左侧摇杆区
            if (Input.mousePosition.x > Screen.width * 0.4f)
            {
                isTouchingUI = false; isPressingMouse = true;
                mousePressStartTime = Time.time; mousePressStartPos = Input.mousePosition;
            }
        }

        // 抬起
        if (Input.GetMouseButtonUp(0))
        {
            if (isTouchingUI) { isTouchingUI = false; return; }

            if (isPressingMouse)
            {
                float dragDistance = Vector3.Distance(Input.mousePosition, mousePressStartPos);
                float pressTime = Time.time - mousePressStartTime;

                if (dragDistance < dragThreshold && pressTime < clickThreshold)
                {
                    DoClickRaycast();
                }
            }
            isPressingMouse = false;
        }
    }

    // 统一射线检测 (支持点文字、模型、空地)
    void DoClickRaycast()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            var layer = hit.collider.GetComponentInParent<LayerBlock>();

            if (layer != null)
            {
                // 点中积木
                if (BuilderSystem.Instance != null)
                {
                    if (BuilderSystem.Instance.toolMode == 0 && PropertyManager.Instance)
                        PropertyManager.Instance.ShowPanel(layer);
                    else if (BuilderSystem.Instance.toolMode == 1 && ConnectionManager.Instance)
                        ConnectionManager.Instance.OnBlockSelected(layer);
                }
                return;
            }

            // 点中空地 -> 取消连线选中
            if (BuilderSystem.Instance != null && BuilderSystem.Instance.toolMode == 1)
            {
                if (ConnectionManager.Instance) ConnectionManager.Instance.CancelSelection();
            }
        }
        else
        {
            // 点中天空 -> 取消连线选中
            if (BuilderSystem.Instance != null && BuilderSystem.Instance.toolMode == 1)
            {
                if (ConnectionManager.Instance) ConnectionManager.Instance.CancelSelection();
            }
        }
    }

    // ================= 视角与UI辅助 ==================
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        if (Input.touchCount > 0)
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        return false;
    }

    void HandleCameraLook()
    {
        if (Input.GetMouseButton(1)) // PC 右键旋转
        {
            RotateCamera(Input.GetAxis("Mouse X") * 2f, Input.GetAxis("Mouse Y") * 2f);
            return;
        }

        if (Input.touchCount > 0) // 手机 触摸旋转
        {
            foreach (Touch touch in Input.touches)
            {
                if (IsPointerOverUI()) continue;

                if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width * 0.4f)
                    lookFingerId = touch.fingerId;

                if (touch.fingerId == lookFingerId && touch.phase == TouchPhase.Moved)
                {
                    float deltaX = touch.deltaPosition.x * mouseSensitivity * mobileRotationFactor * Time.deltaTime;
                    float deltaY = touch.deltaPosition.y * mouseSensitivity * mobileRotationFactor * Time.deltaTime;
                    RotateCamera(deltaX, deltaY);
                }

                if (touch.phase == TouchPhase.Ended && touch.fingerId == lookFingerId)
                    lookFingerId = -1;
            }
        }
    }

    void RotateCamera(float x, float y)
    {
        transform.Rotate(Vector3.up * x);
        verticalRotation -= y;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        if (cam) cam.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}