using UnityEngine;
using System.Collections;

namespace uTools {
    public enum Direction
    {
		Reverse = -1,
		Toggle = 0,
		Forward = 1
	}
    
    public enum TweenDirection
    {
	    [EnumLabel("上")]
	    Up = 0,
	    [EnumLabel("下")]
	    Down,
	    [EnumLabel("左")]
	    Left,
	    [EnumLabel("右")]
	    Right,
    }

    public enum TweenUILayer
    {
	    [EnumLabel("普通UI")]
	    UINormal,
	    [EnumLabel("3D UI")]
	    UIWorld,
    }

    public enum Trigger {
		OnPointerEnter,
		OnPointerDown,
		OnPointerClick,
		OnPointerUp,
		OnPointerExit,
	}

    public enum ShakeType
    {
        ePosition,
        eScale,
        eRotation
    }
}