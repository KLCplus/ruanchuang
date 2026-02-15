using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    [Header("连线设置")]
    public Material lineMaterial;
    public float lineWidth = 0.1f;

    private LayerBlock startLayer; // 记录起点

    void Awake() { Instance = this; }

    public void OnBlockSelected(LayerBlock layer)
    {
        if (BuilderSystem.Instance == null || BuilderSystem.Instance.toolMode != 1) return;

        if (startLayer == null)
        {
            // 1. 选中起点
            startLayer = layer;
            HighlightBlock(startLayer, true);
            Debug.Log($"选中起点: {layer.myType}");
        }
        else if (startLayer == layer)
        {
            // 2. 点了同一个 -> 取消
            CancelSelection();
        }
        else
        {
            // 3. 连线
            CreateConnection(startLayer, layer);
            CancelSelection(); // 连完后自动取消
        }
    }

    // --- 【新增】强制取消选中 ---
    public void CancelSelection()
    {
        if (startLayer != null)
        {
            HighlightBlock(startLayer, false); // 变回原大小
            startLayer = null;
            Debug.Log("已取消选中");
        }
    }

    // --- 创建连线 ---
    void CreateConnection(LayerBlock from, LayerBlock to)
    {
        // 连线逻辑
        if (!to.data.inputIds.Contains(from.data.id))
            to.data.inputIds.Add(from.data.id);

        GameObject lineObj = new GameObject($"Line_{from.myType}_to_{to.myType}");
        lineObj.transform.SetParent(this.transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        if (lineMaterial) lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.positionCount = 2;
        lr.SetPosition(0, from.transform.position);
        lr.SetPosition(1, to.transform.position);
    }

    // --- 视觉反馈 ---
    void HighlightBlock(LayerBlock block, bool isActive)
    {
        if (block == null) return;
        // 选中变大 1.2 倍，取消除以 1.2
        float scaleFactor = isActive ? 1.2f : (1f / 1.2f);
        block.transform.localScale *= scaleFactor;
    }
}