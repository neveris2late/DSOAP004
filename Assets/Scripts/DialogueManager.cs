using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("Test Controls")]
    public Button startTestButton; 
    
    [Header("Ink JSON")]
    public TextAsset inkJsonAsset;
    private Story story;

    [Header("UI Panels")]
    public GameObject suspectPanel; // 嫌疑人对话框父物体
    public TypewriterEffect suspectTypewriter; 
    public TMP_Text suspectNameText; // (可选) 用于更新嫌疑人名字

    public GameObject playerPanel;  // 玩家对话框父物体
    public TypewriterEffect playerTypewriter;  

    [Header("Choices UI")]
    public GameObject choiceButtonContainer; 
    public Button choiceButtonPrefab; 

    [Header("Managers")]
    public InkTagManager tagManager;
    
    private bool isDialoguePlaying = false;

    // 句子之间的停顿时间，让自动播放看起来更自然
    public float autoPlayDelay = 0.5f; 

    void Start()
    {
        // 初始化状态：隐藏玩家面板和选项，显示嫌疑人面板
        suspectPanel.SetActive(true);
        playerPanel.SetActive(false);
        choiceButtonContainer.SetActive(false);

        if (startTestButton != null)
        {
            startTestButton.onClick.AddListener(StartDialogueTest);
            startTestButton.gameObject.SetActive(true);
        }
    }
    
    public void StartDialogueTest()
    {
        if (inkJsonAsset == null)
        {
            Debug.LogError("没有指派 Ink JSON 文件！");
            return;
        }

        if (startTestButton != null) startTestButton.gameObject.SetActive(false);

        story = new Story(inkJsonAsset.text);
        isDialoguePlaying = true;
        
        RefreshView();
    }

    // 核心推进逻辑
    public void RefreshView()
    {
        if (story.canContinue)
        {
            string text = story.Continue().Trim();
            
            // 1. 预判当前是谁在说话（提取你 ParseAndDisplayDialogue 里的逻辑）
            TypewriterEffect currentTyper = suspectTypewriter; // 默认是嫌疑人
            if (text.Contains("我:")) 
            {
                currentTyper = playerTypewriter; // 如果文本包含"我:"，则是玩家
            }

            // 2. 将正确的打字机传递给 TagManager 处理标签
            tagManager.HandleTags(story.currentTags, currentTyper);

            // 3. 解析说话人并播放打字机
            ParseAndDisplayDialogue(text);
        }
        else if (story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
        else
        {
            Debug.Log("对话全部结束");
            isDialoguePlaying = false;
        }
    }

    void ParseAndDisplayDialogue(string fullText)
    {
        string content = fullText;
        bool isPlayer = false;

        // 判断说话人（假设 Ink 中玩家说话以 "我:" 开头）
        if (fullText.Contains(":"))
        {
            string[] parts = fullText.Split(new char[] { ':' }, 2);
            string name = parts[0].Trim();
            content = parts[1].Trim();

            if (name == "我")
            {
                isPlayer = true;
            }
            else
            {
                isPlayer = false;
                // 如果需要动态更新嫌疑人名字，可在这里赋值：
                if(suspectNameText != null) suspectNameText.text = name;
            }
        }

        if (isPlayer)
        {
            // 玩家说话回合
            playerPanel.SetActive(true);
            
            // 传入专门的玩家打字机回调
            playerTypewriter.ShowText(content, OnPlayerTypingFinished);
        }
        else
        {
            // 嫌疑人说话回合
            suspectPanel.SetActive(true);
            
            // 传入专门的嫌疑人打字机回调
            suspectTypewriter.ShowText(content, OnSuspectTypingFinished);
        }
    }

    // --- 回调函数：嫌疑人打字结束 ---
    void OnSuspectTypingFinished()
    {
        // 1. 如果有选项，出选项
        if (story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
        // 2. 如果还有下一句，自动播放
        else if (story.canContinue)
        {
            Invoke(nameof(RefreshView), autoPlayDelay); 
        }
        else
        {
            Debug.Log("本段对话结束");
        }
    }

    // --- 回调函数：玩家打字结束 ---
    void OnPlayerTypingFinished()
    {
        // 按照需求：玩家的语句播放完后，清除玩家 textbox，接着继续播放嫌疑人对话
        Invoke(nameof(ClearPlayerAndContinue), autoPlayDelay);
    }

    void ClearPlayerAndContinue()
    {
        // 清除文本，隐藏面板
        playerTypewriter.textBox.text = "";
        playerPanel.SetActive(false);

        // 继续推进 Ink 故事
        RefreshView();
    }

    // --- 选项处理 ---
    void DisplayChoices()
    {
        // 清理旧按钮
        foreach (Transform child in choiceButtonContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // 出现选项时，显示 Container
        choiceButtonContainer.SetActive(true);

        // 生成新按钮
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            Choice choice = story.currentChoices[i];
            Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer.transform);
            
            TMP_Text btnText = button.GetComponentInChildren<TMP_Text>();
            btnText.text = choice.text;

            int index = i;
            button.onClick.AddListener(() => OnClickChoice(index));
        }
    }

    void OnClickChoice(int choiceIndex)
    {
        // 玩家选择选项后，隐藏选项及其 Container
        choiceButtonContainer.SetActive(false);

        // 告诉 Ink 玩家选了哪个
        story.ChooseChoiceIndex(choiceIndex);

        // 刷新视图，这通常会触发对应选项文本的 `Continue()`
        RefreshView(); 
    }
}