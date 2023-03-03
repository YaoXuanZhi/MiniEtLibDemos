using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace uTools {
	
	public class TweenScrollbar : Tween<float> {

        private float mValue;

        private Scrollbar mScrollbar;
        Scrollbar cacheSlider
        {
            get
            {
                mScrollbar = GetComponent<Scrollbar>();
                if (mScrollbar == null)
                {
                    Debug.LogError("'uTweenScrollbar' can't find 'Scrollbar'");
                }
                return mScrollbar;
            }
        }

        /// <summary>
        /// The need carry.
        /// when is true, value==1 equal value=0
        /// </summary>
        public bool NeedCarry = true;

        public float sliderValue
        {
            set
            {
                if (NeedCarry)
                {
                    if (value >= 1)
                    {
                        cacheSlider.value = value - Mathf.Floor(value);
                    }
                    else
                    {
                        cacheSlider.value = value;
                    }
                    //cacheSlider.value = (value >= 1) ? value - Mathf.Floor(value) : value;
                }
                else
                {
                    if (value > 1)
                    {
                        cacheSlider.value = value - Mathf.Floor(value);
                    }
                    else
                    {
                        cacheSlider.value = value;
                    }
                    //cacheSlider.value = (value > 1) ? value - Mathf.Floor(value) : value;
                }
            }
        }

        public override float value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                sliderValue = value;
            }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
        }

        public static TweenSlider Begin(Scrollbar scrollbar, float from, float to, float duration, float delay) {
			TweenSlider comp = Begin<TweenSlider>(scrollbar.gameObject, duration);
            comp.value = from;
			comp.from = from;
			comp.to = to;
			comp.delay = delay;
			
			if (duration <=0) {
				comp.Sample(1, true);
				comp.enabled = false;
			}
			return comp;
		}
	}
}
