using UnityEngine;
using System.Collections.Generic;
using Ink.Runtime;

public class InkTagManager : MonoBehaviour
{
    [Header("Dependencies")]
    public DetectorBarController detectorController;
    public TypewriterEffect thisTypewriter; 
    
    /// <summary>
    /// 解析传入的标签列表
    /// </summary>
    /// <param name="tags">通常来自 story.currentTags</param>
    /// <param name="currentTypewriter">当前正在使用的打字机特效（如果有）</param>
    public void HandleTags(List<string> tags, TypewriterEffect currentTypewriter)
    {
        if (tags == null || tags.Count == 0) return;

        float targetVal = -1f; // 临时变量，-1代表未在标签中找到
        float speedVal = -1f;

        // 所有的标签解析只需要遍历一次即可
        foreach (string tag in tags)
        {
            // 去除空格并小写化，防止格式错误
            string cleanTag = tag.Trim().ToLower(); 
            
            // 1. 解析 #fill:数值
            if (cleanTag.StartsWith("fill:"))
            {
                string valueStr = cleanTag.Split(':')[1].Trim();
                if (float.TryParse(valueStr, out float v))
                {
                    targetVal = v;
                }
            }
            // 2. 解析 #speed:数值
            else if (cleanTag.StartsWith("speed:"))
            {
                string valueStr = cleanTag.Split(':')[1].Trim();
                if (float.TryParse(valueStr, out float v))
                {
                    speedVal = v;
                }
            }
            // 3. 解析 #reset
            else if (cleanTag == "reset")
            {
                if (detectorController != null)
                {
                    detectorController.ReleaseToIdle();
                }
                // 注意：这里不要用 return，否则会打断后续标签（比如 tspeed）的解析
            }
            // 4. 解析 #tspeed:数值 (打字机速度)
            else if (cleanTag.StartsWith("tspeed:"))
            {
                string valStr = cleanTag.Split(':')[1].Trim();
                if (float.TryParse(valStr, out float s))
                {
                    // 优先使用传入的 currentTypewriter，如果没有则使用面板上挂载的 thisTypewriter
                    TypewriterEffect targetWriter = currentTypewriter != null ? currentTypewriter : thisTypewriter;
                    
                    if (targetWriter != null)
                    {
                        targetWriter.SetSpeed(s);
                    }
                }
            }
        }

        // 如果解析到了有效的 fill 数值，最后统一发送给 Controller
        if (targetVal >= 0 && detectorController != null)
        {
            // 如果标签没写速度，默认给一个较快的反应速度，比如 5
            float finalSpeed = (speedVal >= 0) ? speedVal : 5f;
            detectorController.SetInkInstruction(targetVal, finalSpeed);
        }
    }
}