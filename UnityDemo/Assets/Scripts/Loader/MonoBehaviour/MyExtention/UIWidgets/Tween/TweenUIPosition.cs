using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ET;

namespace uTools {
	
	public class TweenUIPosition : Tween<Vector3>
    {
        public TweenUILayer Layer = TweenUILayer.UINormal;
        public TweenDirection twDirection;
        
        RectTransform mRectTransform = null;
        Transform mTransform = null;
        Camera uiCamera;

        bool mIs3D = true;
        private bool is3D
        {
            get
            {
                if (mTransform == null)
                {
                    mTransform = transform;
                    RectTransform rect = cachedTransform as RectTransform;
                    mIs3D = (rect != null) ? false : true;
                }
                return mIs3D;
            }
            set
            {
                mIs3D = value;
            }
        }

        Transform cachedTransform
        {
            get
            {
                if (mTransform == null)
                {
                    mTransform = transform;
                }
                return mTransform;
            }
        }


        RectTransform cachedRectTransform
        {
            get
            {
                if (mRectTransform == null)
                {
                    mRectTransform = cachedTransform as RectTransform;
                }
                return mRectTransform;
            }
        }

        public override Vector3 value
        {
            get
            {
                if (is3D)
                {
                    return cachedTransform.localPosition;
                }
                else
                {
                    return cachedRectTransform.anchoredPosition;
                }
            }
            set
            {
                if (is3D)
                {
                    cachedTransform.localPosition = value;
                }
                else
                {
                    cachedRectTransform.anchoredPosition = value;
                }
            }
        }

        private void Awake()
        {
            InitCamera();
            Init();
        }

        private void InitCamera()
        {
            // if (GlobalConfig.Inst != null)
            // {
            //     ReferenceCollectorEx global = GlobalConfig.Inst.GetComponent<ReferenceCollectorEx>();
            //     if (Layer == TweenUILayer.UINormal)
            //     {
            //         uiCamera = global.Get<Camera>("UICamera");   
            //     }
            //     else
            //     {
            //         uiCamera = global.Get<Camera>("UICamera3d");
            //     }
            // }
            // else
            {
                uiCamera = GetComponentInParent<Canvas>().worldCamera;
            }
        }

        private void Init()
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, transform.position);
            RectTransform rect = cachedRectTransform;
            switch (twDirection)
            {
                case TweenDirection.Left:
                {
                    float width = cachedRectTransform.rect.width;
                    float distanceX = screen.x; 
                    float fromX = rect.anchoredPosition.x - distanceX - (1-rect.pivot.x)*width;
                    from = new Vector3(fromX, cachedRectTransform.anchoredPosition.y, 0);
                    break;
                }
                case TweenDirection.Right:
                {
                    float width = cachedRectTransform.rect.width;
                    float distanceX = 1920 - screen.x; 
                    float fromX = rect.anchoredPosition.x + distanceX + rect.pivot.x*width;
                    from = new Vector3(fromX, cachedRectTransform.anchoredPosition.y, 0);
                    break;
                }
                case TweenDirection.Up:
                {
                    float height = cachedRectTransform.rect.height;
                    float distanceY = 1080 - screen.y; 
                    float fromY = rect.anchoredPosition.y + distanceY + rect.pivot.y*height;
                    from = new Vector3(rect.anchoredPosition.x, fromY, 0);
                    break;
                }
                case TweenDirection.Down:
                {
                    float height = cachedRectTransform.rect.height;
                    float distanceY = screen.y; 
                    float fromY = rect.anchoredPosition.y - distanceY - (1-rect.pivot.y)*height;
                    from = new Vector3(rect.anchoredPosition.x, fromY, 0);
                    break;
                }
            }
            
            to = cachedRectTransform.anchoredPosition;

            cachedRectTransform.anchoredPosition = from;
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
        }

        public static TweenUIPosition Begin(GameObject go, Vector3 from, Vector3 to, float duration = 1f, float delay = 0f)
        {
            TweenUIPosition comp = Tweener.Begin<TweenUIPosition>(go, duration);
            comp.value = from;
            comp.from = from;
            comp.to = to;
            comp.duration = duration;
            comp.delay = delay;
            if (duration <= 0)
            {
                comp.Sample(1, true);
                comp.enabled = false;
            }
            return comp;
        }
    }
}
