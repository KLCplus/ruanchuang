using UnityEngine;
using TMPro;

public class PropertyManager : MonoBehaviour
{
    public static PropertyManager Instance;

    [Header("UI引用 (拖入Canvas里的对象)")]
    public GameObject panelRoot;
    public TMP_InputField inputFilters;
    public TMP_InputField inputKernel;
    public TMP_InputField inputStride;
    public TMP_InputField inputDropout;

    private LayerBlock currentLayer;

    void Awake() { Instance = this; }

    void Start()
    {
        if (panelRoot) panelRoot.SetActive(false);

        // 绑定事件
        if (inputFilters) inputFilters.onEndEdit.AddListener(val => SaveData());
        if (inputKernel) inputKernel.onEndEdit.AddListener(val => SaveData());
        if (inputStride) inputStride.onEndEdit.AddListener(val => SaveData());
        if (inputDropout) inputDropout.onEndEdit.AddListener(val => SaveData());
    }

    public void ShowPanel(LayerBlock layer)
    {
        currentLayer = layer;
        if (panelRoot) panelRoot.SetActive(true);

        // 先隐藏所有框
        SetVisible(inputFilters, false);
        SetVisible(inputKernel, false);
        SetVisible(inputStride, false);
        SetVisible(inputDropout, false);

        // 根据类型显示
        var type = layer.myType;
        if (type == LayerType.Conv)
        {
            ShowInput(inputFilters, layer.data.filters);
            ShowInput(inputKernel, layer.data.kernelSize);
            ShowInput(inputStride, layer.data.stride);
        }
        else if (type == LayerType.Dense)
        {
            ShowInput(inputFilters, layer.data.units);
        }
        else if (type == LayerType.Dropout)
        {
            ShowInput(inputDropout, layer.data.dropoutRate);
        }
    }

    void SetVisible(TMP_InputField input, bool state)
    {
        if (input && input.transform.parent)
            input.transform.parent.gameObject.SetActive(state);
    }

    void ShowInput(TMP_InputField input, float val)
    {
        SetVisible(input, true);
        input.text = val.ToString();
    }
    // 添加这个方法，用于关闭面板
    public void ClosePanel()
    {
        if (panelRoot) panelRoot.SetActive(false);
        currentLayer = null;
    }
    void SaveData()
    {
        if (currentLayer == null) return;

        // 解析数据
        if (inputFilters) int.TryParse(inputFilters.text, out currentLayer.data.filters);
        currentLayer.data.units = currentLayer.data.filters; // 同步

        if (inputKernel) int.TryParse(inputKernel.text, out currentLayer.data.kernelSize);
        if (inputStride) int.TryParse(inputStride.text, out currentLayer.data.stride);
        if (inputDropout) float.TryParse(inputDropout.text, out currentLayer.data.dropoutRate);

        currentLayer.UpdateVisual();
    }
}