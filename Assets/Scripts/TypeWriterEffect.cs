using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text textBox;
    
    // 当前打字速度 (秒/字)
    private float currentDelay = 0.05f;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private Action onCompleteCallback;

    // 设置打字速度 (供 Manager 调用)
    public void SetSpeed(float speed)
    {
        currentDelay = speed;
    }

    // 开始打字
    public void ShowText(string content, Action onComplete = null)
    {
        StopTyping(); //以此防重叠
        textBox.text = ""; // 清空
        onCompleteCallback = onComplete;
        
        // 开启协程
        typingCoroutine = StartCoroutine(TypeTextRoutine(content));
    }

    // 立即完成 (玩家点击跳过时调用)
    public void CompleteImmediately(string fullText)
    {
        StopTyping();
        textBox.text = fullText;
        onCompleteCallback?.Invoke();
    }

    public bool IsTyping => isTyping;

    private void StopTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
    }

    IEnumerator TypeTextRoutine(string content)
    {
        isTyping = true;
        textBox.text = ""; // 确保清空

        // TMP 的特殊处理：如果我们要逐字显示，需要考虑到富文本标签 <color> 等不应该被拆分
        // 这里使用最简单的 maxVisibleCharacters 方法，这是 TMP 自带的神器
        
        textBox.text = content; // 先把全文赋给它
        textBox.maxVisibleCharacters = 0; // 设为0，全部隐藏

        int totalChars = content.Length; // 注意：这里简单处理，实际上TMP有 textInfo.characterCount

        // 这里的逻辑稍微调整为依赖 TMP 的解析结果
        textBox.ForceMeshUpdate(); 
        int parsedCharCount = textBox.textInfo.characterCount;

        for (int i = 0; i <= parsedCharCount; i++)
        {
            textBox.maxVisibleCharacters = i;
            
            // 简单抖动效果：如果是情绪激动的文字，可以随机轻微改变位置
            // 但更好的抖动是直接在 Ink 里写 <shake>文本</shake>，TMP会自动处理
            
            yield return new WaitForSeconds(currentDelay);
        }

        isTyping = false;
        onCompleteCallback?.Invoke();
    }
}