using UnityEngine;
using UnityEngine.Networking; // 网络请求必须引用
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [Header("后端配置")]
    public string backendUrl = "http://127.0.0.1:5000/predict"; // 你的后端地址

    [Header("UI 引用")]
    public Text resultText; // 拖入 Text_Result，用于显示后端返回的信息
    public Button runButton; // 拖入 Btn_Run

    void Awake() { Instance = this; }

    void Start()
    {
        // 自动绑定按钮事件
        if (runButton) runButton.onClick.AddListener(OnRunClick);
    }

    // --- 1. 点击运行按钮 ---
    public void OnRunClick()
    {
        Debug.Log("开始打包数据...");
        if (resultText) resultText.text = "正在连接大脑...";

        // 从 NeuralManager 获取现有的 JSON
        if (NeuralManager.Instance == null) return;
        string json = NeuralManager.Instance.GenerateJSON();

        // 打印看看发了什么
        Debug.Log("发送数据: " + json);

        // 启动协程发送网络请求
        StartCoroutine(PostRequest(json));
    }

    // --- 2. 发送 POST 请求 ---
    IEnumerator PostRequest(string jsonBody)
    {
        // 创建请求
        UnityWebRequest request = new UnityWebRequest(backendUrl, "POST");

        // 将 JSON 转换为字节流
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // 【关键】设置头文件，告诉后端这是 JSON
        request.SetRequestHeader("Content-Type", "application/json");

        // 发送并等待
        yield return request.SendWebRequest();

        // --- 3. 处理结果 ---
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("网络错误: " + request.error);
            if (resultText) resultText.text = "<color=red>连接失败: " + request.error + "</color>";
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("收到回复: " + responseText);

            // 在 UI 上显示结果
            if (resultText) resultText.text = "预测结果:\n" + ParseResponse(responseText);
        }
    }

    // --- 4. 解析后端返回的 JSON (简单的例子) ---
    string ParseResponse(string json)
    {
        // 假设后端返回 {"message": "Success", "accuracy": 0.95}
        // 这里简单直接显示，你也可以写一个类来解析
        return json;
    }
}