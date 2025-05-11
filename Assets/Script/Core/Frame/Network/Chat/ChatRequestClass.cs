using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Network.ChatSystem
{
    public static class ChatRequestClass
    {
        [Serializable]
        public class ChatRequestBody
        {
            [Serializable]
            public class Message
            {
                public string role;
                public string content;
                public string tool_call_id;
            }

            [Serializable]
            public class Tool
            {
                public string type = "function";
                public object function; // 可以是 SelectFunctionCalling、CrudFunctionCalling 等
            }

            public string model;
            public List<Message> messages;
            public bool safe_mode;
            public List<Tool> tools;
        }

        // 函数 1: 生成 SQL SELECT 查询
        [Serializable]
        public class SelectFunctionCalling
        {
            public string name = "generateSelectQuery";
            public string description = "生成基于用户意图的本地 SQLite 数据库 'Tasks' 表的 SQL SELECT 查询。数据库模式：Tasks 表包含 TaskID (int, 主键), Title (text, 非空), Description (text, 可空), Priority (int, 默认 2), Status (int, 默认 0), DueDate (datetime, 可空), StartedAt (datetime, 非空), UpdatedAt (datetime, 非空)。";
            public Parameters parameters = new Parameters();

            [Serializable]
            public class Parameters
            {
                public string type = "object";
                public Properties properties = new Properties();
                public string[] required = new string[] { "reply", "intent", "generatedSelect" };
                public bool additionalProperties = false;
            }

            [Serializable]
            public class Properties
            {
                public Property reply = new Property { type = "string", description = "中文回复" };
                public Property intent = new Property { type = "string", description = "用户查询意图，例如 '查找今天到期的任务'" };
                public Property generatedSelect = new Property { type = "string", description = "生成的 SQL SELECT 语句，例如 'SELECT * FROM Tasks WHERE DueDate = \"2025-03-05\"'" };
            }

            [Serializable]
            public class Property
            {
                public string type;
                public string description;
            }
        }

        // 函数 2: 生成 CRUD 查询
        [Serializable]
        public class CrudFunctionCalling
        {
            public string name = "generateCrudQuery";
            public string description = "根据用户意图生成针对本地 SQLite 数据库 'Tasks' 表的 SQL 查询（INSERT、UPDATE、DELETE 或 SELECT）。数据库模式：Tasks 表包含 TaskID (int, 主键), Title (text, 非空), Description (text, 可空), Priority (int, 默认 2), Status (int, 默认 0), DueDate (datetime, 可空), StartedAt (datetime, 非空), UpdatedAt (datetime, 非空)。";
            public Parameters parameters = new Parameters();

            [Serializable]
            public class Parameters
            {
                public string type = "object";
                public Properties properties = new Properties();
                public string[] required = new string[] { "reply", "intent", "generatedQuery", "undoQuery" };
                public bool additionalProperties = false;
            }

            [Serializable]
            public class Properties
            {
                public Property reply = new Property { type = "string", description = "中文回复，例如 '好的，我帮你处理了'" };
                public Property intent = new Property { type = "string", description = "用户 CRUD 操作意图，例如 '添加标题为“会议”的任务，明天到期' 或 '删除 ID 为 1 的任务'" };
                public Property generatedQuery = new Property { type = "string", description = "生成的 SQL 查询，例如 'INSERT INTO Tasks (Title, DueDate, StartedAt, UpdatedAt) VALUES (\"Meeting\", \"2025-03-06\", \"2025-03-05\", \"2025-03-05\")'" };
                public Property undoQuery = new Property { type = "string", description = "撤销操作的 SQL 查询，例如 INSERT 的撤销为 'DELETE FROM Tasks WHERE TaskID = (last_inserted_id)'" };
            }

            [Serializable]
            public class Property
            {
                public string type;
                public string description;
            }
        }

        // 函数 3: 生成带情感的回复
        [Serializable]
        public class ReplyWithEmotionFunctionCalling
        {
            public string name = "generateReplyWithEmotion";
            public string description = "根据用户输入或上下文生成带情感的回复消息，返回一个键值对列表，每个键值对包含表情（键）和对应的句子（值），并评估对话对好感度的影响。";
            public Parameters parameters = new Parameters();

            [Serializable]
            public class Parameters
            {
                public string type = "object";
                public Properties properties = new Properties();
                public string[] required = new string[] { "emotionContentPairs", "favorabilityImpact" };
                public bool additionalProperties = false;
            }

            [Serializable]
            public class Properties
            {
                public EmotionContentPairsProperty emotionContentPairs = new EmotionContentPairsProperty();
                public Property favorabilityImpact = new Property { type = "number", description = "对话对好感度的影响值，范围为 -5 到 +3" };
            }

            [Serializable]
            public class EmotionContentPairsProperty
            {
                public string type = "array";
                public string description = "键值对列表，每个元素包含表情（emotion）和对应的句子（content）";
                public PairItem items = new PairItem();
            }

            [Serializable]
            public class PairItem
            {
                public string type = "object";
                public PairProperties properties = new PairProperties();
                public string[] required = new string[] { "emotion", "content" };
            }

            [Serializable]
            public class PairProperties
            {
                public EmotionProperty emotion = new EmotionProperty();
                public Property content = new Property { type = "string", description = "该表情对应的句子，例如 '干得漂亮！' 或 '让我再想想…'" };
            }

            [Serializable]
            public class Property
            {
                public string type;
                public string description;
            }

            [Serializable]
            public class EmotionProperty
            {
                public string type = "string";
                public string description = "表情，例如 'happy'、'sad'、'mad'、'shy'";
                public string[] @enum = new string[] { "happy", "sad", "mad", "shy" };
            }
        }

        //函数4：专注检测
        [Serializable]
        public class CheckFocusFunctionCalling
        {
            public string name = "checkFocus";
            public string description = "检测用户的专注状态，返回 AI 的回复内容和专注检测结果。";
            public Parameters parameters = new Parameters();

            [Serializable]
            public class Parameters
            {
                public string type = "object";
                public Properties properties = new Properties();
                public string[] required = new string[] { "replyContent", "focusResult" };
                public bool additionalProperties = false;
            }

            [Serializable]
            public class Properties
            {
                public Property replyContent = new Property { type = "string", description = "AI 的回复内容，例如 '你看起来很专注！' 或 '你似乎有些分心。'" };
                public Property focusResult = new Property { type = "boolean", description = "专注检测结果，true 表示专注，false 表示不专注" };
            }

            [Serializable]
            public class Property
            {
                public string type;
                public string description;
            }
        }
    }

    namespace Core.Framework.Network.ChatSystem
    {
        public static class ChatResponseClass
        {
            [Serializable]
            public class ChatResponse
            {
                // 其他字段保持不变
                public string id;
                public string @object;
                public long created;
                public string model;
                public Choice[] choices;
                public Usage usage;

                [Serializable]
                public class Choice
                {
                    public int index;
                    public Message message;
                    public string finish_reason;
                }

                [Serializable]
                public class Message
                {
                    public string role;
                    public string content;
                    public FunctionCall function_call;
                    public ToolCall[] tool_calls;
                }

                [Serializable]
                public class FunctionCall
                {
                    public string name;
                    public string arguments;
                }

                [Serializable]
                public class ToolCall
                {
                    public string id;
                    public string type;
                    public FunctionCall function;
                }

                [Serializable]
                public class Usage
                {
                    public int prompt_tokens;
                    public int completion_tokens;
                    public int total_tokens;
                }

                [Serializable]
                public class SelectFunctionArgu
                {
                    public string reply;
                    public string intent;
                    public string generatedSelect;
                }
                [Serializable]
                public class ReplyWithEmotionArgu
                {
                    public EmotionContentPair[] emotionContentPairs;
                    public float favorabilityImpact;

                    [Serializable]
                    public class EmotionContentPair
                    {
                        public string emotion;
                        public string content;
                    }
                }

                [Serializable]
                public class CheckFocusArgu
                {
                    public string replyContent;
                    public bool focusResult;
                }
            }
        }
    }
}