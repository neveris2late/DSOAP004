using UnityEngine;

// CreateAssetMenu 让你能在 Project 窗口右键 -> Create -> Game -> Suspect Profile 创建新档案
[CreateAssetMenu(fileName = "NewSuspect", menuName = "Game/Suspect Profile")]
public class SuspectsSO : ScriptableObject
{
    [Header("1. 身份信息 (Identity)")]
    [Tooltip("嫌疑人的显示名称，支持在运行时修改")]
    public string characterName = "Unknown";
    
    [Tooltip("核心谜题答案：他是仿生人吗？")]
    public bool isAndroid = false;

    [Header("2. 叙事资源 (Narrative)")]
    [Tooltip("拖入编译好的 .json 文件 (Ink生成的)")]
    public TextAsset inkJSONAsset;

    [Tooltip("背景故事/档案描述 (仅供策划备忘或UI显示)")]
    [TextArea(5, 10)] // 让输入框变大，方便写小作文
    public string backgroundStory;

    [Header("3. 初始数值 (Base Stats)")]
    [Tooltip("基础异常值 (0-100)：审讯开始时检测条的默认位置")]
    [Range(0, 100)]
    public float baseAnomalyLevel = 10f;

    [Header("4. Base Drift (Drift Nums)")]
    [Tooltip("基础波动值 (-10 - 10)：审讯开始时检测条的默认波动范围")]
    [Range(-10 , 10)]
    public float baseAnomalyDrift = 0f;
    
    //[Header("4. 视觉素材 (Visuals - 可选)")]
    //public Sprite portrait; // 嫌疑人立绘
}