using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class ScaleListener : MonoBehaviour
    {
        public float Threshold = 0.1f;
        public Action TriggerEvent;
        private float curScale;
        private bool isTrigger = false;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        public void Reset()
        {
            isTrigger = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isTrigger)
            {
                return;
            }
            
            if (transform.localScale.x >= Threshold 
                && transform.localScale.y >= Threshold
                && transform.localScale.z >= Threshold)
            {
                isTrigger = true;
                TriggerEvent?.Invoke();
            }
        }
    }
}
