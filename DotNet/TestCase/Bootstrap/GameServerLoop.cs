// 模拟Unity PlayerLoop 主循环系统
public class GameServerLoop
{
    public delegate void LoopAction();
    
    public event LoopAction OnUpdate;
    
    private bool _isRunning;
    
    public void Start()
    {
        _isRunning = true;
        
        while (_isRunning)
        {
            OnUpdate?.Invoke();
            Thread.Sleep(1);
        }
    }
    
    public void Stop() => _isRunning = false;
}