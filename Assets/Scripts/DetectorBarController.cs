using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime; // [新增] 必须引入 Ink 运行时

public class DetectorBarController : MonoBehaviour
{
    [Header("1. Data Source")]
    public SuspectsSO currentSuspect; // 拖入当前的嫌疑人SO

    [Header("2. References")]
    public Image barImage; 
    // 需要引用 TagManager 来解析标签
    public InkTagManager inkTagManager;
    
    [Header("3. Status Debug")]
    [SerializeField] private float currentFill = 0f;  // 当前显示值
    [SerializeField] private float targetFill = 0f;   // 目标值
    [SerializeField] private float currentSpeed = 1f; // 当前移动速度
    
    [Header("--- DEBUG TEST MODE ---")]
    [Tooltip("拖入编译好的 Ink JSON 文件进行测试")]
    public TextAsset testInkJson; 
    private Story _testStory;

    // 内部变量
    private Material _barMat;
    private bool _isInkControlling = false; // 是否正受Ink指令控制
    private float _driftTimer = 0f;         // 漂移计时器

    void Start()
    {
        // 1. 初始化材质
        if (barImage != null)
        {
            _barMat = Instantiate(barImage.material);
            barImage.material = _barMat;
        }

        // 2. 初始化数值
        if (currentSuspect != null)
        {
            // 初始直接设为基准值
            currentFill = currentSuspect.baseAnomalyLevel;
            targetFill = currentFill;
        }
        
        // [新增] 初始化测试故事
        if (testInkJson != null)
        {
            _testStory = new Story(testInkJson.text);
            Debug.Log("测试模式开启：按 [空格键] 继续对话");
        }
        else
        {
            Debug.LogError("请拖入 Ink JSON 文件以开启测试！");
        }
        
        UpdateShaderValues();
    }

    void Update()
    {
        // 核心分流逻辑
        if (_isInkControlling)
        {
            HandleInkMovement();
        }
        else
        {
            HandleIdleDrift();
        }

        UpdateShaderValues();
    }

    // --- 逻辑 A: 闲置时的随机漂移 (符合你要求的Drift) ---
    void HandleIdleDrift()
    {
        if (currentSuspect == null) return;

        _driftTimer -= Time.deltaTime;

        if (_driftTimer <= 0f)
        {
            // 1. 读取基准值
            float baseVal = currentSuspect.baseAnomalyLevel;
            
            // 2. 在 -10 到 10 之间随机选取 Drift
            float drift = Random.Range(-10f, 10f);
            
            // 3. 设定新目标 (基准 + Drift)
            targetFill = Mathf.Clamp(baseVal + drift, 0f, 100f);
            
            // 4. 填充速度降为 1-3 之间的随机值
            currentSpeed = Random.Range(1f, 3f);

            // 重置计时器 (每 2-4 秒变换一次漂移目标)
            _driftTimer = Random.Range(2f, 4f);
        }

        // 执行平滑移动
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * currentSpeed);
    }

    // --- 逻辑 B: Ink 指令执行 ---
    void HandleInkMovement()
    {
        // 向 Ink 指定的目标移动
        // 这里使用 MoveTowards 还是 Lerp 取决于你想不想让到达终点时有减速感，Lerp更自然
        if (Mathf.Abs(currentFill - targetFill) > 0.1f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * currentSpeed);
        }
        else
        {
            currentFill = targetFill;
            // 可选：当到达 Ink 指定的目标后，是否自动切回漂移模式？
            // 如果希望一直保持在此高度直到下一句话，就保持 _isInkControlling = true
            // 如果希望说完话慢慢回落，可以在这里加计时器重置 _isInkControlling = false
        }
    }

    // --- 公共接口：供 InkTagManager 调用 ---
    
    /// <summary>
    /// 接收 Ink 的标签指令
    /// </summary>
    /// <param name="target">目标能量值 (0-100)</param>
    /// <param name="speed">填充速度变量</param>
    public void SetInkInstruction(float target, float speed)
    {
        _isInkControlling = true; // 标记为正在受控，暂停随机漂移
        targetFill = Mathf.Clamp(target, 0f, 100f);
        currentSpeed = speed;
    }

    /// <summary>
    /// (可选) 手动释放控制，让其回到基准值漂移
    /// </summary>
    public void ReleaseToIdle()
    {
        _isInkControlling = false;
        // 这里的 driftTimer 设为 0 会让它在下一帧立即计算一个新的漂移点
        _driftTimer = 0f; 
    }

    void UpdateShaderValues()
    {
        if (_barMat != null)
        {
            _barMat.SetFloat("_FillAmount", currentFill / 100f);
            // 可以在这里加上 brightness 的控制
        }
    }
}