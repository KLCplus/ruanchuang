using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleMobileController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("UI 组件引用")]
    public RectTransform handle; // 摇杆中间的小圆点

    private Vector3 inputVector; // 存储输入值 (-1 到 1)

    // --- 接口实现：处理 UI 拖拽 ---
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / (transform as RectTransform).sizeDelta.x);
            pos.y = (pos.y / (transform as RectTransform).sizeDelta.y);

            inputVector = new Vector3(pos.x * 2, 0, pos.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // 移动小圆点
            handle.anchoredPosition = new Vector2(inputVector.x * (transform as RectTransform).sizeDelta.x / 3,
                                                  inputVector.z * (transform as RectTransform).sizeDelta.y / 3);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector3.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    // --- 【关键】只提供数据，不执行移动 ---
    // 给 PlayerController 调用
    public Vector3 GetInputDirection()
    {
        return new Vector3(inputVector.x, 0, inputVector.z);
    }
}