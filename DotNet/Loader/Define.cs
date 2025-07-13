namespace ET
{
	public static class Define
	{
#if UNITY_EDITOR
		public static bool IsEditor = true;
#else
        public static bool IsEditor = false;
#endif
		
#if UNITY_WEBGL && !UNITY_EDITOR
		public static bool IsUnityStandaloneWebGL = true;
#else
		public static bool IsUnityStandaloneWebGL = false;
#endif
	}
}