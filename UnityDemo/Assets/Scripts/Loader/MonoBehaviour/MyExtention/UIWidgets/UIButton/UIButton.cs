using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using ET;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 按钮状态
/// </summary>
public enum EBtnState : int
{
    Normal = 1,//正常
    NoClick,//不能点击
}

[AddComponentMenu("UI/UIButton")]
public class UIButton : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// 按钮事件
    /// </summary>
    public enum EBtnEvent : int
    {
        None = 0,
        Click = 1,          //点击
        LongClick = 2,      //长按
        Drag = 3,           //拖拽
        LongClickDrag = 4,  //长按拖拽
    }

    #region 变量
    /// <summary>
    /// 按钮图片
    /// </summary>
    public Image btnImage { get; private set; }

    /// <summary>
    /// 是否点击缩放
    /// </summary>
    public bool IsUseClickScale = true;
    /// <summary>
    /// 缩放时间
    /// </summary>
    public float ScaleTime = 0.1f;
    /// <summary>
    /// 比例
    /// </summary>
    public float Persentage = 1.1f;
    private Vector3 defaultScale = Vector3.one;
    private Tweener scaleTweener;
    public object data;
    
    [SerializeField]
    private EBtnState mState = EBtnState.Normal;
    public EBtnState BtnState
    {
        get { return mState; }
    }

    /// <summary>
    /// 是否使用长按
    /// </summary>
    public bool mIsUseLongClick = false;
    public bool IsUseLongClick
    {
        get
        {
            return mIsUseLongClick;
        }
        set
        {
            if (mIsUseLongClick == value)
            { return; }
            mIsUseLongClick = value;
        }
    }

    /// <summary>
    /// 响应长按阀值
    /// </summary>
    public float LongClickTime = 1f;
    /// <summary>
    /// 点击的时间
    /// </summary>
    private float clickTime = 0f;
    /// <summary>
    /// 是否使用拖拽
    /// </summary>
    public bool mIsUseDrag = false;
    public bool IsUseDrag
    {
        get
        {
            return mIsUseDrag;
        }
        set
        {
            if (mIsUseDrag == value)
            { return; }
            mIsUseDrag = value;
        }
    }

    /// <summary>
    /// 悬浮图标
    /// </summary>
    public GameObject pointObject;


    /// <summary>
    /// 悬浮特效
    /// </summary>
    public int pointEffect;

    /// <summary>
    /// 是否需要拖动scrollrect
    /// </summary>
    public bool mIsDragScrollRect = false;
    public bool IsDragScrollRect
    {
        get
        {
            return mIsDragScrollRect;
        }
        set
        {
            if (mIsDragScrollRect == value)
            { return; }
            mIsDragScrollRect = value;
        }
    }

    /// <summary>
    /// 所在的scrollrect
    /// </summary>
    // private LoopScrollRectBase exScrollrect;
    /// <summary>
    /// 响应拖拽距离
    /// </summary>
    public float DragOffset = 0;
    /// <summary>
    /// 记录开始拖拽位置
    /// </summary>
    private Vector2 beginDragPos = Vector2.zero;
    /// <summary>
    /// 是否拖拽中
    /// </summary>
    private bool isDragging = false;
    private bool isStopDragging = false;
    private PointerEventData cacheDragData = null;
    /// <summary>
    /// 事件
    /// </summary>
    private EBtnEvent mBtnEvent = EBtnEvent.None;
    private EBtnEvent BtnEvent
    {
        get
        {
            return mBtnEvent;
        }
        set
        {
            if (mBtnEvent == value)
            { return; }
            EBtnEvent old = mBtnEvent;
            mBtnEvent = value;
            //处理事件变化 
            BtnEventChange(old);
        }
    }

    /// <summary>
    /// 点击音效
    /// </summary>
    public int SoundId = 10001;

    private int oldSoundId = 0;
    #endregion

    #region 点击、长按、拖拽事件
    public Action<GameObject> onClickDown;
    public Action<GameObject, PointerEventData> onClickUp;
    public Action<GameObject> onLongClick;
    public Action<GameObject> onEndLongClick;
    public Action<GameObject, PointerEventData> onPointerEnter;
    public Action<GameObject, PointerEventData> onPointerExit;
    public Action<GameObject, PointerEventData> onBeginDrag;
    public Action<GameObject, PointerEventData> onDragging;
    public Action<GameObject, PointerEventData> onEndDrag;

    /// <summary>
    /// 点击响应
    /// </summary>
    public void OnClick()
    {
        if (mState != EBtnState.NoClick)
        {
            //播放点击音效            
            PlayAudio();
            onClick?.Invoke();
            SetPointObject(false);
        }
    }

    /// <summary>
    /// 长按响应
    /// </summary>
    public void OnLongClick()
    {
        if (IsUseLongClick)
        {
            //Debug.Log("OnLongClick");
            if (null != onLongClick)
            {
                onLongClick.Invoke(this.gameObject);
            }
        }
    }

    /// <summary>
    /// 结束长按响应
    /// </summary>
    public void OnEndLongClick()
    {
        if (IsUseLongClick)
        {
            if (null != onEndLongClick)
            {
                onEndLongClick.Invoke(this.gameObject);
            }
        }
        else
        {
            if(null != onPointerExit)
            {
                onPointerExit?.Invoke(gameObject,null);
            }
            SetPointObject(false);
        }
    }

    /// <summary>
    /// 拖拽开始
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsUseDrag)
        {
            // if (IsDragScrollRect && null == exScrollrect)
            // {
            //     exScrollrect = this.GetComponentInParent<LoopScrollRectBase>();
            // }
            cacheDragData = eventData;
            beginDragPos = eventData.position;
        }
    }
    
    /// <summary>
    /// 拖拽中
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (IsUseDrag)
        {
            cacheDragData = eventData;
            if (BtnEvent == EBtnEvent.Drag || BtnEvent == EBtnEvent.LongClickDrag)
            {
                // //Debug.Log("OnDrag");
                // if (IsDragScrollRect && null != exScrollrect && isStopDragging)
                // {
                //     exScrollrect.OnDrag(eventData);
                // }
                if (null != onDragging)
                {
                    //通知拖拽中
                    onDragging.Invoke(this.gameObject, eventData);
                }
            }
            else
            {
                if (Vector2.SqrMagnitude(eventData.position - beginDragPos) >= DragOffset * DragOffset)
                {
                    if (BtnEvent == EBtnEvent.LongClick || BtnEvent == EBtnEvent.LongClickDrag)
                        BtnEvent = EBtnEvent.LongClickDrag;
                    else
                        BtnEvent = EBtnEvent.Drag;
                    isDragging = true;
                    isStopDragging = true;
                    //Debug.Log("OnBeginDrag");
                    // if (IsDragScrollRect && null != exScrollrect)
                    // {
                    //     exScrollrect.OnBeginDrag(eventData);
                    // }
                    if (null != onBeginDrag)
                    {
                        //通知开始拖拽
                        onBeginDrag.Invoke(this.gameObject, eventData);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 拖拽结束
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsUseDrag && isDragging)
        {
            isDragging = false;
            //Debug.Log("OnEndDrag");
            // if (IsDragScrollRect && null != exScrollrect)
            // {
            //     exScrollrect.OnEndDrag(eventData);
            // }
            if (null != onEndDrag)
            {
                onEndDrag.Invoke(this.gameObject, eventData);
            }
            BtnEvent = EBtnEvent.None;
        }
    }

    /// <summary>
    /// 按钮事件变化
    /// </summary>
    private void BtnEventChange(EBtnEvent old)
    {
        switch (BtnEvent)
        {
            case EBtnEvent.Click:
                {
                    //记录点击时间
                    clickTime = Time.realtimeSinceStartup;
                }
                break;
            case EBtnEvent.LongClick:
                {
                    //响应长按
                    OnLongClick();
                }
                break;
            case EBtnEvent.Drag:
                {
                    
                }
                break;
            case EBtnEvent.None:
                {
                    if (old == EBtnEvent.LongClick || old == EBtnEvent.LongClickDrag)
                    {
                        //结束长按
                        OnEndLongClick();
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 强制停止拖拽scrollrect
    /// </summary>
    public void EndDragScollRect()
    {
        // if (IsUseDrag && IsDragScrollRect && null != exScrollrect && isStopDragging && null != cacheDragData)
        // {
        //     isStopDragging = false;
        //     exScrollrect.OnEndDrag(cacheDragData);
        //     exScrollrect.StopMovement();
        // }
    }
    #endregion

    #region 重写方法
    private void Awake()
    {
        base.Awake();
        defaultScale = transform.localScale;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void OnEnable()
    {
        base.OnEnable();
        // exScrollrect = null;
        // if (IsUseDrag && IsDragScrollRect)
        // {
        //     exScrollrect = this.GetComponentInParent<LoopScrollRect>();
        // }
        SetPointObject(false);
    }

    private void OnDisable()
    {
        base.OnDisable();
        BtnEvent = EBtnEvent.None;
        
        if (isDragging)
        {
            EndDragScollRect();
        }
    }

    private void OnDestroy()
    {
        base.OnDestroy();
        onClickDown = null;
        onClickUp = null;
        onLongClick = null;
        onEndLongClick = null;
        onBeginDrag = null;
        onDragging = null;
        onEndDrag = null;
        onClick = null;
        if (scaleTweener != null)
        {
            scaleTweener.Kill();
            scaleTweener = null;
        }
    }

    /// <summary>
    /// 弹起
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData != null)
            base.OnPointerUp(eventData);
        if (null != onClickUp)
        {
            onClickUp.Invoke(this.gameObject, eventData);
        }
        //Debug.Log("OnPointerUp");
        if (mState != EBtnState.NoClick)
        {
            if (BtnEvent == EBtnEvent.LongClick)
            {
                BtnEvent = EBtnEvent.None;
            }
            else if (BtnEvent == EBtnEvent.Click)
            {
                OnClick();
                BtnEvent = EBtnEvent.None;
            }
            else
            {
                BtnEvent = EBtnEvent.None;
            }
        }

        StopScale();
    }

    /// <summary>
    /// 按下
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData != null)
            base.OnPointerDown(eventData);
        //Debug.Log("OnPointerDown");
        if (mState != EBtnState.NoClick)
        {
            BtnEvent = EBtnEvent.Click;
            if (null != onClickDown)
            {
                onClickDown.Invoke(this.gameObject);
            }

            if (IsUseClickScale)
            {
                StopScale();
                scaleTweener = transform.DOScale(Persentage, ScaleTime);
            }
        }
    }

    private void StopScale()
    {
        if (!IsUseClickScale)
        {
            return;
        }
        
        if (scaleTweener != null)
        {
            scaleTweener.Kill();
            transform.localScale = defaultScale;
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (BtnEvent == EBtnEvent.Click)
        {
            BtnEvent = EBtnEvent.None;
        }
        base.OnPointerExit(eventData);
        //结束长按
        OnEndLongClick();

        StopScale();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        
        onPointerEnter?.Invoke(gameObject, eventData);
        SetPointObject(true);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        //base.OnPointerClick(eventData);
    }

    private void Update()
    {
        if (IsUseLongClick)
        {
            if (BtnEvent == EBtnEvent.Click)
            {
                if ((Time.realtimeSinceStartup - clickTime) >= LongClickTime)
                {
                    //长按
                    BtnEvent = EBtnEvent.LongClick;
                }
            }
#if UNITY_IOS || UNITY_ANDROID
                else if (BtnEvent == EBtnEvent.LongClick)
                {
                    if (Input.touchCount <= 0)
                    {
                        //结束长按
                        OnEndLongClick();
                    }
                }
#endif
        }
    }
    #endregion

    public void PlayAudio()
    {
        if (SoundId == 0)
        {
            return;
        }
        
        if (oldSoundId != SoundId)
        {
            // UIAudioCriMgr.Instance.Stop((uint)SoundId);    
        }
        oldSoundId = SoundId;
        // UIAudioCriMgr.Instance.Play(SoundId);
    }

    private void SetPointObject(bool isAcitive)
    {
        if(pointObject != null)
        {
            pointObject.SetActive(isAcitive);
        }
    }
}
