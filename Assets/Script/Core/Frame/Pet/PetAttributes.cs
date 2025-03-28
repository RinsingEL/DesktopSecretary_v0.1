using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Frame.Pet
{
    [Serializable]
    public class Memory
    {
        public string content;        // 记忆内容
        public DateTime timestamp;    // 记忆时间戳
        public float importance;      // 记忆重要程度
    }

    public class PetAttributes : MonoBehaviour
    {
        // 短期记忆系统
        private List<Memory> shortTermMemories = new List<Memory>();
        private const int MAX_MEMORIES = 10;  // 最大记忆数量

        // 用户评价系统
        public float userTrustScore { get; private set; } = 50f;    // 信任度 (0-100)
        public float userAffectionScore { get; private set; } = 50f; // 亲密度 (0-100)
        public float userRespectScore { get; private set; } = 50f;   // 尊重度 (0-100)

        // 添加新的记忆
        public void AddMemory(string content, float importance = 1.0f)
        {
            Memory newMemory = new Memory
            {
                content = content,
                timestamp = DateTime.Now,
                importance = importance
            };

            shortTermMemories.Add(newMemory);

            // 如果超过最大记忆数量，删除最旧的记忆
            if (shortTermMemories.Count > MAX_MEMORIES)
            {
                shortTermMemories.RemoveAt(0);
            }
        }

        // 获取所有记忆
        public List<Memory> GetMemories()
        {
            return new List<Memory>(shortTermMemories);
        }

        // 更新用户评价分数
        public void UpdateUserTrust(float delta)
        {
            userTrustScore = Mathf.Clamp(userTrustScore + delta, 0f, 100f);
        }

        public void UpdateUserAffection(float delta)
        {
            userAffectionScore = Mathf.Clamp(userAffectionScore + delta, 0f, 100f);
        }

        public void UpdateUserRespect(float delta)
        {
            userRespectScore = Mathf.Clamp(userRespectScore + delta, 0f, 100f);
        }

        // 获取总体评价
        public float GetOverallScore()
        {
            return (userTrustScore + userAffectionScore + userRespectScore) / 3f;
        }

        // 根据记忆和评分生成性格特征
        public string GetPersonality()
        {
            float overallScore = GetOverallScore();
            if (overallScore >= 80)
                return "非常友好和信任";
            else if (overallScore >= 60)
                return "友好但有些谨慎";
            else if (overallScore >= 40)
                return "略显疏离";
            else
                return "较为冷淡";
        }
    }
}