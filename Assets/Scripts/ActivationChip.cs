using UnityEngine;

public class ActivationChip : MonoBehaviour
{
    [Header("芯片类型")]
    public string activationName = "ReLU"; // 在 Inspector 改为 ReLU, Sigmoid 等

    private bool isDragging = false;
    private Vector3 offset;
    private float zCoord;
    private Rigidbody rb;
    [HideInInspector]
    public string parentLayerId;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true; // 默认有重力
    }

    void OnMouseDown()
    {
        isDragging = true;
        zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();

        // 拔出逻辑：如果它是某个层的子物体，断开关系
        if (transform.parent != null && transform.parent.name == "Socket")
        {
            // 通知父层级清空激活函数
            var layer = transform.parent.parent.GetComponent<LayerBlock>();
            if (layer) layer.DetachChip();

            transform.SetParent(null);
        }

        rb.isKinematic = true; // 拿在手里时不受物理影响
    }

    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + offset;
    }

    void OnMouseUp()
    {
        isDragging = false;
        CheckForSocket();
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    // 核心：寻找最近的插槽
    void CheckForSocket()
    {
        LayerBlock[] layers = FindObjectsOfType<LayerBlock>();
        LayerBlock nearest = null;
        float minDist = 1.0f; // 吸附半径

        foreach (var layer in layers)
        {
            if (layer.socketPoint == null) continue; // 有些层可能没插槽(如Flatten)
            float dist = Vector3.Distance(transform.position, layer.socketPoint.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = layer;
            }
        }

        if (nearest != null)
        {
            nearest.AttachChip(this); // 吸附！
        }
        else
        {
            rb.isKinematic = false; // 没插中，掉下去
            rb.useGravity = true;
        }
    }
}