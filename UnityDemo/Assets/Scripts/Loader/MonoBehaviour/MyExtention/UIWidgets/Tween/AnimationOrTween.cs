using UnityEngine;

namespace AnimationOrTween
{
    [System.Flags]
    public enum EIgnoreXYZ : int
    {
        None = 0,
        X = 0x1,
        Y = 0x2,
        Z = 0x4,
    }

    [System.Flags]
    public enum EIgnoreLRTB : int
    {
        None = 0,
        Left = 0x1,
        Right = 0x2,
        Top = 0x4,
        Bottom = 0x8,
    }

    //public enum Trigger
    //{
    //	OnClick,
    //	OnHover,
    //	OnPress,
    //	OnHoverTrue,
    //	OnHoverFalse,
    //	OnPressTrue,
    //	OnPressFalse,
    //	OnActivate,
    //	OnActivateTrue,
    //	OnActivateFalse,
    //	OnDoubleClick,
    //	OnSelect,
    //	OnSelectTrue,
    //	OnSelectFalse,
    //}

    //public enum Direction
    //{
    //	Reverse = -1,
    //	Toggle = 0,
    //	Forward = 1,
    //}

    public enum EnableCondition
	{
		DoNothing = 0,
		EnableThenPlay,
		IgnoreDisabledState,
	}

	public enum DisableCondition
	{
		DisableAfterReverse = -1,
		DoNotDisable = 0,
		DisableAfterForward = 1,
	}
}
