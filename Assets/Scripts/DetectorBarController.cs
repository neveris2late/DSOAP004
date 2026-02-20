using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime; 

public class DetectorBarController : MonoBehaviour
{
    [Header("1. Data Source")]
    public SuspectsSO currentSuspect;

    [Header("2. References")]
    public Image barImage; 
    public InkTagManager inkTagManager;
    
    [Header("3. Core Status")]
    [SerializeField] private float displayFill = 0f;  // 最终发送给Shader的显示值 (包含波动)
    [SerializeField] private float baseFill = 0f;     // 平滑移动的底层基准线
    [SerializeField] private float targetFill = 0f;   // 当前要前往的目标值 (Ink设定或闲置漂移设定)
    [SerializeField] private float currentSpeed = 1f; 

    [Header("4. Continuous Fluctuation (动态波动参数)")]
    [Tooltip("基础波动幅度（能量为0时的上下抖动范围）")]
    public float minFluctuationAmplitude = 0.5f; 
    [Tooltip("最大波动幅度（能量为100时的上下抖动范围）")]
    public float maxFluctuationAmplitude = 8f;
    [Tooltip("基础波动速度（呼吸频率）")]
    public float minFluctuationSpeed = 0.5f;
    [Tooltip("最大波动速度（高压状态下的急促抖动频率）")]
    public float maxFluctuationSpeed = 4f;
    
    [Header("--- DEBUG TEST MODE ---")]
    public TextAsset testInkJson; 
    private Story _testStory;

    // 内部变量
    private Material _barMat;
    private bool _isInkControlling = false; 
    private float _driftTimer = 0f;
    private float _noiseSeed; // 用于产生独一无二的随机波动序列

    void Start()
    {
        // 1. 初始化材质
        if (barImage != null)
        {
            _barMat = Instantiate(barImage.material);
            barImage.material = _barMat;
        }

        // 2. 随机化噪声种子，确保每次游戏波动感不同
        _noiseSeed = Random.Range(0f, 1000f);

        // 3. 初始化数值
        if (currentSuspect != null)
        {
            baseFill = currentSuspect.baseAnomalyLevel;
            targetFill = baseFill;
        }
        
        // 初始化测试故事
        if (testInkJson != null)
        {
            _testStory = new Story(testInkJson.text);
            Debug.Log("测试模式开启：按 [空格键] 继续对话");
        }
        
        UpdateShaderValues();
    }

    void Update()
    {
        // 1. 核心分流：决定 targetFill 是谁给的
        if (!_isInkControlling)
        {
            HandleIdleTarget(); 
        }

        // 2. 基准线平滑追赶 targetFill
        baseFill = Mathf.Lerp(baseFill, targetFill, Time.deltaTime * currentSpeed);

        // 3. 计算波动并叠加 (核心动态效果)
        ApplyContinuousFluctuation();

        // 4. 更新表现层
        UpdateShaderValues();
    }

    // --- 逻辑 A: 闲置时的宏观漂移目标更新 ---
    void HandleIdleTarget()
    {
        if (currentSuspect == null) return;

        _driftTimer -= Time.deltaTime;
        if (_driftTimer <= 0f)
        {
            float baseVal = currentSuspect.baseAnomalyLevel;
            float drift = Random.Range(-10f, 10f);
            
            targetFill = Mathf.Clamp(baseVal + drift, 0f, 100f);
            currentSpeed = Random.Range(0.5f, 2f); // 闲置时移动得慢一点更自然
            _driftTimer = Random.Range(3f, 6f);    // 宏观目标变更的间隔变长，因为每帧都有噪声在动
        }
    }

    // --- 逻辑 C: 永不静止的动态波动计算 ---
    void ApplyContinuousFluctuation()
    {
        // 计算当前基准值所占的比例 (0 到 1)
        float fillRatio = baseFill / 100f;

        // 根据当前能量高低，线性插值计算出【当前的振幅】和【当前的波动速度】
        float currentAmplitude = Mathf.Lerp(minFluctuationAmplitude, maxFluctuationAmplitude, fillRatio);
        float currentNoiseSpeed = Mathf.Lerp(minFluctuationSpeed, maxFluctuationSpeed, fillRatio);

        // 使用柏林噪声生成 -1 到 1 的平滑随机因子
        // Time.time * currentNoiseSpeed 决定了波动的快慢
        float noiseValue = Mathf.PerlinNoise(Time.time * currentNoiseSpeed, _noiseSeed) * 2f - 1f;

        // 最终显示值 = 底层基准线 + (随机因子 * 振幅)
        displayFill = Mathf.Clamp(baseFill + (noiseValue * currentAmplitude), 0f, 100f);
    }

    // --- 公共接口：供 InkTagManager 调用 ---
    public void SetInkInstruction(float target, float speed)
    {
        _isInkControlling = true; 
        targetFill = Mathf.Clamp(target, 0f, 100f); // 直接改变基准目标
        currentSpeed = speed;
    }

    public void ReleaseToIdle()
    {
        _isInkControlling = false;
        _driftTimer = 0f; 
    }

    void UpdateShaderValues()
    {
        if (_barMat != null)
        {
            // 注意这里使用的是计算过波动的 displayFill 
            _barMat.SetFloat("_FillAmount", displayFill / 100f);
        }
    }
}