using UnityEngine;
using System.Collections.Generic;
using Ink.Runtime; // 必须引入 Ink 运行时命名空间

public class InkTagManager : MonoBehaviour
{
    [Header("Dependencies")]
    public DetectorBarController detectorController;
    public TypewriterEffect thisTypewriter; // 引用上面的脚本
    
    // 假设你有一个 DM 脚本持有当前的 Story 对象
    // 你需要在 DM update 或 Continue() 之后调用这里的 HandleTags
    
    /// <summary>
    /// 解析传入的标签列表
    /// </summary>
    /// <param name="tags">通常来自 story.currentTags</param>
    public void HandleTags(List<string> tags)
    {
        if (tags == null || tags.Count == 0) return;

        float targetVal = -1f; // 临时变量，-1代表未在标签中找到
        float speedVal = -1f;

        foreach (string tag in tags)
        {
            // 去除空格并小写化，防止格式错误
            string cleanTag = tag.Trim().ToLower(); 
            
            // 解析 #fill:数值
            if (cleanTag.StartsWith("fill:"))
            {
                string valueStr = cleanTag.Split(':')[1].Trim();
                if (float.TryParse(valueStr, out float v))
                {
                    targetVal = v;
                }
            }
            // 解析 #speed:数值
            else if (cleanTag.StartsWith("speed:"))
            {
                string valueStr = cleanTag.Split(':')[1].Trim();
                if (float.TryParse(valueStr, out float v))
                {
                    speedVal = v;
                }
            }
            // 解析 #reset (可选：重置回漂移模式)
            else if (cleanTag == "reset")
            {
                detectorController.ReleaseToIdle();
                return; // 重置后不再处理其他数值
            }
        }

        // 如果解析到了有效数值，发送给 Controller
        // 这里做一个简单的逻辑：如果有 fill 没 speed，就用默认快速度；
        // 如果只有 speed 没 fill，通常是不合法的，忽略即可。
        
        if (targetVal >= 0)
        {
            // 如果标签没写速度，默认给一个较快的反应速度，比如 5
            float finalSpeed = (speedVal >= 0) ? speedVal : 5f;
            
            detectorController.SetInkInstruction(targetVal, finalSpeed);
        }
        
        foreach (string tag in tags)
        {
            string cleanTag = tag.Trim().ToLower();

            // 解析 #tspeed: 0.1 (Typewriter Speed)
            if (cleanTag.StartsWith("tspeed:"))
            {
                string valStr = cleanTag.Split(':')[1].Trim();
                if (float.TryParse(valStr, out float s))
                {
                    thisTypewriter.SetSpeed(s);
                }
            }
            // 解析 #jitter (这里演示如果想用代码控制全局抖动，通常用TMP标签更好)
        }
    }
}