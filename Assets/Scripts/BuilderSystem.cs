using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class BuilderSystem : MonoBehaviour
{
    public static BuilderSystem Instance;

    [Header("UI 引用")]
    public Text modeText;
    public Text statusText; // 显示当前拿的是什么

    [Header("资源库 (名字必须和Sidebar一致)")]
    public List<GameObject> layerPrefabs; // 积木: Layer_Conv, Layer_Pool...
    public List<GameObject> chipPrefabs;  // 芯片: Chip_ReLU, Chip_Tanh...

    [Header("特效")]
    public GameObject explosionPrefab;

    [Header("状态")]
    public int toolMode = 0; // 0=编辑, 1=建造
    
    // --- 核心变量 ---
    public int currentPrefabIndex = 0;
    public int currentChipIndex = 0;
    public bool isPlacingChip = false; // 当前是造芯片吗？

    void Awake() { Instance = this; }

    void Start() { UpdateUI(); }

    void Update()
    {
        UpdateAimingFeedback(); // 实时显示准星瞄准了什么
    }

    // ==========================================
    // 1. 旧版按钮逻辑 (点击 Switch 循环切换)
    // ==========================================
    public void OnClickSwitch()
    {
        // 逻辑：在 "积木列表" 和 "芯片列表" 之间无限循环
        
        if (!isPlacingChip)
        {
            // 当前是积木，切换到下一个积木
            currentPrefabIndex++;
            
            // 如果积木选完了，进入芯片模式
            if (currentPrefabIndex >= layerPrefabs.Count)
            {
                currentPrefabIndex = 0; // 重置积木索引
                isPlacingChip = true;   // 切换到芯片模式
                currentChipIndex = 0;   // 选中第一个芯片
            }
        }
        else
        {
            // 当前是芯片，切换到下一个芯片
            currentChipIndex++;
            
            // 如果芯片选完了，回到积木模式
            if (currentChipIndex >= chipPrefabs.Count)
            {
                currentChipIndex = 0;   // 重置芯片索引
                isPlacingChip = false;  // 切换回积木模式
                currentPrefabIndex = 0; // 选中第一个积木
            }
        }
        
        // 更新 UI 显示，让你知道切换到了什么
        UpdateAimingFeedback();
    }

    // ==========================================
    // 2. 新版侧边栏逻辑 (点击按钮直接选中)
    // ==========================================
    // ==========================================
    // 2. 新版侧边栏逻辑 (带侦探调试功能)
    // ==========================================
    public void SelectPrefabByName(string name)
    {
        Debug.Log($"正在尝试查找组件: [{name}]...");

        // --- A. 在积木列表里找 ---
        if (layerPrefabs == null || layerPrefabs.Count == 0)
        {
            Debug.LogError("【严重错误】Layer Prefabs 列表是空的！请在 GameManager 里把积木预制体拖进去！");
        }
        else
        {
            for (int i = 0; i < layerPrefabs.Count; i++)
            {
                // 防止空引用
                if (layerPrefabs[i] == null) continue;

                // 打印列表里的名字，帮你检查
                // Debug.Log($"列表项 {i}: {layerPrefabs[i].name}"); 

                if (layerPrefabs[i].name.Contains(name))
                {
                    currentPrefabIndex = i;
                    isPlacingChip = false;
                    Debug.Log($"✅ 成功选中积木: {layerPrefabs[i].name}");
                    UpdateAimingFeedback();
                    return;
                }
            }
        }

        // --- B. 在芯片列表里找 ---
        if (chipPrefabs == null || chipPrefabs.Count == 0)
        {
            Debug.LogError("【严重错误】Chip Prefabs 列表是空的！请在 GameManager 里把芯片预制体拖进去！");
        }
        else
        {
            for (int i = 0; i < chipPrefabs.Count; i++)
            {
                if (chipPrefabs[i] == null) continue;

                if (chipPrefabs[i].name.Contains(name))
                {
                    currentChipIndex = i;
                    isPlacingChip = true;
                    Debug.Log($"✅ 成功选中芯片: {chipPrefabs[i].name}");
                    UpdateAimingFeedback();
                    return;
                }
            }
        }

        // --- C. 如果运行到这里，说明彻底没找到 ---
        string allLayers = "";
        foreach (var p in layerPrefabs) if (p) allLayers += p.name + ", ";

        string allChips = "";
        foreach (var p in chipPrefabs) if (p) allChips += p.name + ", ";

        Debug.LogError($"❌ 找不到名字包含 '[{name}]' 的物体！\n" +
                       $"当前积木列表里有: {allLayers}\n" +
                       $"当前芯片列表里有: {allChips}\n" +
                       $"请检查：1.GameManager里拖了吗？ 2.SidebarUI里的名字写对了吗？");
    }
    // ==========================================
    // 3. 建造逻辑 (点击 Build)
    // ==========================================
    public void OnClickPlace()
    {
        if (toolMode == 0) { Debug.Log("请先切换到建造模式"); return; }
        if (Camera.main == null) return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // --- A. 造芯片 ---
        if (isPlacingChip)
        {
            if (chipPrefabs.Count == 0) return;
            GameObject prefab = chipPrefabs[currentChipIndex];

            if (Physics.Raycast(ray, out hit, 10f))
            {
                LayerBlock targetLayer = hit.collider.GetComponentInParent<LayerBlock>();
                if (targetLayer != null && targetLayer.socketPoint != null)
                {
                    GameObject chipObj = Instantiate(prefab, hit.point, Quaternion.identity);
                    ActivationChip chipScript = chipObj.GetComponent<ActivationChip>();
                    if (chipScript != null)
                    {
                        targetLayer.AttachChip(chipScript);
                        chipScript.parentLayerId = targetLayer.data.id;
                    }
                }
            }
        }
        // --- B. 造积木 ---
        else
        {
            if (layerPrefabs.Count == 0) return;
            GameObject prefab = layerPrefabs[currentPrefabIndex];

            Vector3 spawnPos = Vector3.zero;
            if (Physics.Raycast(ray, out hit, 20f))
            {
                spawnPos = hit.point;
                spawnPos.y = Mathf.Max(0, Mathf.Round(spawnPos.y));
                spawnPos.x = Mathf.Round(spawnPos.x);
                spawnPos.z = Mathf.Round(spawnPos.z);
            }
            else
            {
                spawnPos = ray.GetPoint(4.0f);
                spawnPos.x = Mathf.Round(spawnPos.x);
                spawnPos.y = Mathf.Round(spawnPos.y);
                spawnPos.z = Mathf.Round(spawnPos.z);
            }

            // 防重叠
            if (Physics.CheckSphere(spawnPos, 0.4f, LayerMask.GetMask("Default"))) 
            {
                 // 简单检查一下有没有东西
                 Collider[] colliders = Physics.OverlapSphere(spawnPos, 0.4f);
                 foreach(var c in colliders) 
                    if(c.GetComponentInParent<LayerBlock>()) return; 
            }

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            // 注册数据
            string cleanName = prefab.name.Replace("Layer_", "").Replace("(Clone)", "");
            LayerType typeEnum = LayerType.Conv;
            try { typeEnum = (LayerType)Enum.Parse(typeof(LayerType), cleanName); } catch { }
            if (cleanName.Contains("Batch")) typeEnum = LayerType.BatchNorm;

            BlockData newData = new BlockData(typeEnum, spawnPos);
            var layerScript = obj.GetComponent<LayerBlock>();
            if (layerScript) layerScript.Init(newData);
            if (NeuralManager.Instance) NeuralManager.Instance.RegisterBlock(newData);
        }
    }

    // --- 4. 辅助功能 ---
    public void OnClickToggleMode()
    {
        toolMode = 1 - toolMode;
        if (toolMode == 0 && statusText) statusText.text = "";
        if (toolMode == 1 && PropertyManager.Instance) PropertyManager.Instance.ClosePanel();
        UpdateUI();
    }
    
    public void OnClickDelete()
    {
        if (Camera.main == null) return;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 20f))
        {
            ActivationChip chip = hit.collider.GetComponentInParent<ActivationChip>();
            if (chip != null) { 
                if(chip.transform.parent) chip.transform.parent.SendMessage("DetachChip", SendMessageOptions.DontRequireReceiver);
                if(explosionPrefab) Instantiate(explosionPrefab, hit.point, Quaternion.identity);
                Destroy(chip.gameObject); return; 
            }
            LayerBlock layer = hit.collider.GetComponentInParent<LayerBlock>();
            if (layer != null) {
                if (NeuralManager.Instance) NeuralManager.Instance.RemoveBlock(layer.data);
                if(explosionPrefab) Instantiate(explosionPrefab, hit.point, Quaternion.identity);
                Destroy(layer.gameObject);
            }
        }
    }

    void UpdateAimingFeedback()
    {
        if (statusText == null || toolMode == 0) return;
        
        string itemName = "None";
        if (isPlacingChip && chipPrefabs.Count > currentChipIndex) 
            itemName = chipPrefabs[currentChipIndex].name;
        else if (!isPlacingChip && layerPrefabs.Count > currentPrefabIndex) 
            itemName = layerPrefabs[currentPrefabIndex].name;

        statusText.text = $"当前: <color=yellow>{itemName}</color>";
    }

    void UpdateUI()
    {
        if (modeText) modeText.text = (toolMode == 0) ? "模式: 编辑" : "模式: 建造";
    }
}