namespace ET.Client
{
    public abstract class AUIEvent
    {
        /// <summary>
        /// 创建的时候调用一次
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="uiType"></param>
        public abstract ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer);
        
        /// <summary>
        /// Destroy的时候调用一次
        /// </summary>
        /// <param name="uiComponent"></param>
        public abstract void OnRemove(UIComponent uiComponent);
        
        /// <summary>
        /// 创建(OnCreate)之后和再次激活显示的时候调用
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="uiType"></param>
        /// <param name="args"></param>
        public abstract void OnShow(UIComponent uiComponentEx, string uiType, params object[] args);
        
        /// <summary>
        /// 隐藏的时候调用
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="uiType"></param>
        public abstract void OnHide(UIComponent uiComponentEx, string uiType);
    }
}