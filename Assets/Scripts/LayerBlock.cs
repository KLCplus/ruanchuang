using UnityEngine;
using TMPro;

public class LayerBlock : MonoBehaviour
{
    [Header("配置")]
    public LayerType myType;
    public Transform socketPoint;
    public TextMeshPro infoText;

    public BlockData data;
    private ActivationChip currentChip;

    public void Init(BlockData _data)
    {
        data = _data;
        UpdateVisual();
    }

    // --- 下面是之前的辅助逻辑，保持不变 ---
    public void AttachChip(ActivationChip chip)
    {
        if (currentChip != null && currentChip != chip) Destroy(currentChip.gameObject);
        currentChip = chip;
        data.activation = chip.activationName;

        chip.transform.SetParent(socketPoint);
        chip.transform.localPosition = Vector3.zero;
        chip.transform.localRotation = Quaternion.identity;
        if (chip.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;

        UpdateVisual();
    }

    public void DetachChip()
    {
        currentChip = null;
        data.activation = "None";
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (infoText == null) return;
        string txt = $"<b>{myType}</b>";
        if (myType == LayerType.Conv) txt += $"\n{data.filters} ({data.kernelSize}x{data.kernelSize})";
        else if (myType == LayerType.Dense) txt += $"\n{data.units} Units";
        else if (myType == LayerType.Dropout) txt += $"\nDrop: {data.dropoutRate}";
        if (data.activation != "None") txt += $"\n<color=yellow>{data.activation}</color>";
        infoText.text = txt;
    }
}