using UnityEngine;
using UnityEngine.UI; // 如果是Image组件
// using UnityEngine.Rendering; // 如果需要操作材质属性块

public class DetectorController : MonoBehaviour
{
    [Header("Settings")]
    public float fillSpeed = 5.0f; // 填充速度变量，可在Inspector控制
    public float addAmount = 5.0f; // 每次增加的量 (0-100)
    public float brightness = 1.0f; // 亮度控制

    [Header("References")]
    public Image barImage; // 拖入使用了该Shader的UI Image

    private Material _barMat;
    private float _targetFill = 0f; // 目标值 (0-100)
    private float _currentFill = 0f; // 当前实际显示值 (0-100)

    void Start()
    {
        // 获取材质实例，避免修改原始文件
        if (barImage != null)
        {
            _barMat = Instantiate(barImage.material);
            barImage.material = _barMat;
        }
        
        // 初始化
        UpdateShaderValues();
    }

    void Update()
    {
        HandleInput();
        UpdateFillAnimation();
        UpdateShaderValues();
    }

    void HandleInput()
    {
        // 空格键增加能量
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _targetFill += addAmount;
            // 限制在 0-100 之间
            _targetFill = Mathf.Clamp(_targetFill, 0f, 100f);
        }
    }

    void UpdateFillAnimation()
    {
        // 使用 Lerp 进行平滑过渡
        // Time.deltaTime * fillSpeed 决定了过渡的快慢
        if (Mathf.Abs(_currentFill - _targetFill) > 0.01f)
        {
            _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.deltaTime * fillSpeed);
        }
        else
        {
            _currentFill = _targetFill;
        }
    }

    void UpdateShaderValues()
    {
        if (_barMat != null)
        {
            // Shader 里如果是 0-1，这里需要除以 100
            _barMat.SetFloat("_FillAmount", _currentFill / 100f);
            _barMat.SetFloat("_Brightness", brightness);
        }
    }
}