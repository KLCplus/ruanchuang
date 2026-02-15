using UnityEngine;
using UnityEngine.UI;

public class SidebarUI : MonoBehaviour
{
    [Header("子菜单面板 (拖进来)")]
    public GameObject otherPanel;      // "其他组件"的子菜单
    public GameObject activationPanel; // "激活函数"的子菜单

    // --- 1. 基础组件选择 ---
    public void OnClick_Conv() { Select("Layer_Conv"); }
    public void OnClick_Pool() { Select("Layer_Pool"); }
    public void OnClick_Dense() { Select("Layer_Dense"); }

    // --- 2. 辅助组件选择 (Add, Flatten 等) ---
    public void OnClick_Add() { Select("Layer_Add"); }
    public void OnClick_Flatten() { Select("Layer_Flatten"); }
    public void OnClick_Dropout() { Select("Layer_Dropout"); }
    public void OnClick_BatchNorm() { Select("Layer_BatchNorm"); }
    public void OnClick_Concat() { Select("Layer_Concat"); }

    // --- 3. 激活函数选择 ---
    public void OnClick_ReLU() { Select("Chip_ReLU"); }
    public void OnClick_Sigmoid() { Select("Chip_Sigmoid"); }
    public void OnClick_Tanh() { Select("Chip_Tanh"); }

    // --- 核心逻辑 ---
    void Select(string prefabName)
    {
        if (BuilderSystem.Instance != null)
        {
            BuilderSystem.Instance.SelectPrefabByName(prefabName);
        }
    }

    // --- 菜单折叠/展开 ---
    public void ToggleOtherPanel()
    {
        bool isActive = otherPanel.activeSelf;
        otherPanel.SetActive(!isActive);
        // 如果想互斥(点这个关那个)，可以在这里把 activationPanel.SetActive(false);
    }

    public void ToggleActivationPanel()
    {
        bool isActive = activationPanel.activeSelf;
        activationPanel.SetActive(!isActive);
    }
}