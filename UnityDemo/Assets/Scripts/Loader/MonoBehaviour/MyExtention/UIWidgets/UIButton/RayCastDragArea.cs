using System.Collections;
using System.Collections.Generic;
using ET;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
/// <summary>
/// Describe:响应
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class RayCastDragArea :RayCastArea,IPointerClickHandler
{
    public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
        where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; i++)
        {
            if (current != results[i].gameObject)
            {
                var go = ExecuteEvents.GetEventHandler<T>(results[i].gameObject);
                if (go != null)
                {
                    var button = go.GetComponent<UIButton>();
                    if (button)
                    {
                        button.OnClick();
                        return;
                    }
                }
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerClickHandler);
    }
}