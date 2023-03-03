using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 按钮缩放脚本
/// </summary>
public class UIButtonScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    public enum EnumScaleType
    {
        auto,
        custom,
    }

    [SerializeField]
    private EnumScaleType scaletype = EnumScaleType.auto;

    private float tovalue = 1.1f;

    [SerializeField]
    private float persentage = 1.1f;

    private float fromvalue = 1;

    [SerializeField]
    private float duration = 0.1f;

    [SerializeField]
    private float from = 0;

    [SerializeField]
    private float to = 1;
    
    private RectTransform target;
    private bool isInit = false;
    public void Init()
    {
        if (isInit)
        {
            return;
        }
        isInit = true;
        target = this.transform as RectTransform;
        if (scaletype == EnumScaleType.auto)
        {
            fromvalue = this.transform.localScale.x == 0 ? 1 : this.transform.localScale.x;
            tovalue = fromvalue * persentage;
        }
        else
        {
            fromvalue = from;
            tovalue = to;
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Init();
        if (target != null)
        {
            target.localScale = new Vector3(fromvalue, fromvalue, fromvalue);
            target.DOScale(tovalue, duration);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Init();
        if (target != null)
        {
            target.localScale = new Vector3(tovalue, tovalue, tovalue);
            target.DOScale(fromvalue, duration);
        }
    }

    public void OnDisable()
    {
        DOTween.Kill(this.gameObject);
    }

    public void OnDestroy()
    {
        DOTween.Kill(this.gameObject);
    }

    public float GetPersentage()
    {
        return persentage;
    }
}
