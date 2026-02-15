using System;
using System.Collections.Generic;
using UnityEngine;

// 1. 定义所有层类型
public enum LayerType
{
    Conv, Pool, Dense,
    Flatten, Dropout, BatchNorm,
    Add, Concat,
    Input, Output
}

[Serializable]
public class BlockData
{
    // --- 图结构核心 (Graph Logic) ---
    public string id;              // 唯一身份证
    public List<string> inputIds;  // 记录“谁连向了我” (前置节点ID)

    // --- 基础属性 ---
    public LayerType type;
    public string typeName;
    public Vector3 position;
    public string activation = "None"; // 默认为空，插上芯片后更新

    // --- 全参数池 (所有层共用，按需取值) ---
    public int filters = 32;
    public int kernelSize = 3;
    public int stride = 1;
    public int padding = 1;
    public int units = 128;
    public float dropoutRate = 0.5f;

    // 构造函数
    public BlockData(LayerType _type, Vector3 _pos)
    {
        id = Guid.NewGuid().ToString(); // 自动生成 GUID
        inputIds = new List<string>();
        type = _type;
        typeName = _type.ToString();
        position = _pos;
        InitializeDefaults();
    }

    // 智能初始化默认值
    void InitializeDefaults()
    {
        switch (type)
        {
            case LayerType.Conv:
                filters = 16; kernelSize = 3; stride = 1; padding = 1;
                break;
            case LayerType.Pool:
                kernelSize = 2; stride = 2; padding = 0;
                break;
            case LayerType.Dense:
                units = 64;
                break;
            case LayerType.Dropout:
                dropoutRate = 0.5f;
                break;
        }
    }
}