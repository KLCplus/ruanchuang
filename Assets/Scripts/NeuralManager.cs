using System.Collections.Generic;
using UnityEngine;

public class NeuralManager : MonoBehaviour
{
    public static NeuralManager Instance;

    // 存储所有层数据
    public List<BlockData> allBlocks = new List<BlockData>();

    void Awake() { Instance = this; }

    public void RegisterBlock(BlockData data)
    {
        if (!allBlocks.Contains(data))
            allBlocks.Add(data);
    }

    // --- 【新增】安全删除数据的逻辑 ---
    public void RemoveBlock(BlockData dataToRemove)
    {
        if (dataToRemove == null) return;

        // 1. 从列表中移除这个积木的数据
        // 我们通过 ID 来查找并移除，确保准确
        allBlocks.RemoveAll(b => b.id == dataToRemove.id);

        // 2. 清理连线关系
        // 遍历剩下所有的积木，如果谁连接了这个被删的积木，就断开连接
        foreach (var block in allBlocks)
        {
            if (block.inputIds.Contains(dataToRemove.id))
            {
                block.inputIds.Remove(dataToRemove.id);
            }
        }

        Debug.Log($"数据已清理: {dataToRemove.type}");
    }

    public string GenerateJSON()
    {
        GraphWrapper wrapper = new GraphWrapper();
        wrapper.layers = allBlocks;
        return JsonUtility.ToJson(wrapper, true);
    }
}

[System.Serializable]
public class GraphWrapper
{
    public List<BlockData> layers;
}