using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("Test Controls")]
    public Button startTestButton; // [新增] 拖入你的测试按钮
    
    [Header("Ink JSON")]
    public TextAsset inkJsonAsset;
    private Story story;

    [Header("UI Panels")]
    public GameObject suspectPanel; // 嫌疑人对话框父物体
    public TypewriterEffect suspectTypewriter; // 嫌疑人打字机
    public TMP_Text suspectNameText;

    public GameObject playerPanel;  // 玩家对话框父物体
    public TypewriterEffect playerTypewriter;  // 玩家打字机 (可选)

    [Header("Choices UI")]
    public GameObject choiceButtonContainer; // 按钮的父容器 (Grid Layout Group)
    public Button choiceButtonPrefab; // 按钮预制体

    [Header("Managers")]
    public InkTagManager tagManager;
    
    private bool isDialoguePlaying = false;
    
    private bool isWaitingForChoice = false;

    void Start()
    {
        // 初始化时隐藏对话框
        suspectPanel.SetActive(true);
        playerPanel.SetActive(false);
        choiceButtonContainer.SetActive(false);

        // [新增] 绑定按钮事件
        if (startTestButton != null)
        {
            startTestButton.onClick.AddListener(StartDialogueTest);
            startTestButton.gameObject.SetActive(true);
        }
    }
    
    // [新增] 点击按钮后调用的方法
    public void StartDialogueTest()
    {
        if (inkJsonAsset == null)
        {
            Debug.LogError("没有指派 Ink JSON 文件！");
            return;
        }

        // 隐藏开始按钮
        if (startTestButton != null) startTestButton.gameObject.SetActive(false);

        story = new Story(inkJsonAsset.text);
        isDialoguePlaying = true;
        
        RefreshView();
    }

    void StartStory()
    {
        story = new Story(inkJsonAsset.text);
        RefreshView();
    }

    // 核心循环函数
    public void RefreshView()
    {
        // 1. 如果有内容继续播放
        if (story.canContinue)
        {
            string text = story.Continue().Trim();
            
            // 处理标签
            tagManager.HandleTags(story.currentTags);

            // 解析说话人
            ParseAndDisplayDialogue(text);
        }
        // 2. 如果没有内容，检查是否有选项
        else if (story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
        else
        {
            Debug.Log("对话结束");
            isDialoguePlaying = false;
            // 可选：结束后重新显示开始按钮
            // if (startTestButton != null) startTestButton.gameObject.SetActive(true);
        }
    }

    void ParseAndDisplayDialogue(string fullText)
    {
        string content = fullText;
        bool isSuspect = true; // 默认为嫌疑人

        // 简单的说话人判断逻辑
        if (fullText.Contains(":"))
        {
            string[] parts = fullText.Split(new char[] { ':' }, 2);
            string name = parts[0].Trim();
            content = parts[1].Trim();

            // 名字是"我"
            if (name == "我")
            {
                isSuspect = false;
            }
            else
            {
                
            }
        }

        // 根据是谁在说话，显示不同的面板
        if (isSuspect)
        {
            suspectPanel.SetActive(true);
            //playerPanel.SetActive(false); // 或者不隐藏，看设计
            //choiceButtonContainer.SetActive(false); // 隐藏选项

            // 嫌疑人说话：调用打字机
            // 关键：传入的回调函数 RefreshView，意味着打完字后尝试继续
            suspectTypewriter.ShowText(content, OnTypingFinished);
        }
        else
        {
            // 玩家说话 (通常是选完选项后的详细文本)
            //suspectPanel.SetActive(false); // 或者变暗
            playerPanel.SetActive(true);
            choiceButtonContainer.SetActive(false);

            // 玩家直接显示或也用打字机
            playerTypewriter.ShowText(content, OnTypingFinished);
        }
    }

    // 打字机打完后的回调
    void OnTypingFinished()
    {
        // 1. 优先检查是否有选项 (Ink 在文本结束前就会预加载选项)
        if (story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
        // 2. [关键修复] 如果没有选项，但还有后续文本，自动播放下一句
        else if (story.canContinue)
        {
            // 添加一个微小的延迟可以让对话节奏更自然，不加也可以
            Invoke("RefreshView", 0.5f); // 0.5秒后自动播放下一句
            // 或者直接调用: RefreshView(); 
        }
        // 3. 既没选项也没文本，说明对话彻底结束
        else
        {
            Debug.Log("本段对话结束");
        }
    }
    // 显示选项按钮
    void DisplayChoices()
    {
        // 清理旧按钮
        foreach (Transform child in choiceButtonContainer.transform)
        {
            Destroy(child.gameObject);
        }

        choiceButtonContainer.SetActive(true);
        isWaitingForChoice = true;

        // 生成新按钮
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            Choice choice = story.currentChoices[i];
            Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer.transform);
            
            // 设置按钮文字 (简介)
            TMP_Text btnText = button.GetComponentInChildren<TMP_Text>();
            btnText.text = choice.text;

            // 绑定点击事件
            int index = i;
            button.onClick.AddListener(() => OnClickChoice(index));
        }
    }

    // 玩家点击选项
    void OnClickChoice(int choiceIndex)
    {
        choiceButtonContainer.SetActive(false);
        isWaitingForChoice = false;

        // 告诉 Ink 玩家选了哪个
        story.ChooseChoiceIndex(choiceIndex);

        // 这里的关键：Ink 的机制是选了选项后，Continue() 会输出该选项内部的内容
        RefreshView(); 
    }
}