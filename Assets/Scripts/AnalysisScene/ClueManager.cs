//职责： 管理分析池、记录分数、执行最终的“分析”聚拢动画。

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance;

    [Header("UI References")]
    public Transform analysisPool; // 分析池的父节点
    public Button analyzeButton;   // 分析按钮
    public Transform analyzeCenterPoint; // 聚拢的中心点

    [Header("Prefabs")]
    public GameObject floatingCluePrefab; // 漂浮线索预制体

    // 存储当前池中的线索
    private List<FloatingClue> activeClues = new List<FloatingClue>();

    // 模拟线索数据库 (实际开发中可以读取配置表)
    public Dictionary<string, bool> clueDatabase = new Dictionary<string, bool>()
    {
        {"android_core", true}, // 核心是被破坏的 (+1分)
        {"fake_id", false},     // 伪造的ID (0分)
    };

    private void Awake()
    {
        Instance = this;
        analyzeButton.onClick.AddListener(OnAnalyzeClicked);
    }

    // 供 InteractableText 调用的生成方法
    public void AddClueToPool(string clueID, string clueName)
    {
        // 避免重复添加
        if (activeClues.Exists(c => c.clueID == clueID)) return;

        GameObject newClueObj = Instantiate(floatingCluePrefab, analysisPool);
        FloatingClue clueScript = newClueObj.GetComponent<FloatingClue>();
        
        bool isGood = clueDatabase.ContainsKey(clueID) ? clueDatabase[clueID] : false;
        clueScript.Init(clueID, clueName, isGood);
        activeClues.Add(clueScript);
    }

    public void RemoveClue(FloatingClue clue)
    {
        activeClues.Remove(clue);
        Destroy(clue.gameObject);
    }

    private void OnAnalyzeClicked()
    {
        analyzeButton.interactable = false;
        int totalScore = 0;

        // 1. 聚拢动画
        foreach (var clue in activeClues)
        {
            if (clue.isGood) totalScore++;
            
            // 停止原有的漂浮动画
            clue.transform.DOKill(); 
            // 飞向中心并缩小消失
            clue.transform.DOMove(analyzeCenterPoint.position, 0.5f).SetEase(Ease.InBack);
            clue.transform.DOScale(Vector3.zero, 0.5f).SetDelay(0.3f);
        }

        // 2. 播放循环动画 (这里用延迟模拟1秒的分析过程)
        DOVirtual.DelayedCall(1.5f, () => 
        {
            Debug.Log($"分析完成！总得分: {totalScore}");
            // 在这里触发你的 UI 特效、音效或剧情推进
            
            // 清理池子
            foreach (var clue in activeClues) Destroy(clue.gameObject);
            activeClues.Clear();
            analyzeButton.interactable = true;
        });
    }
}