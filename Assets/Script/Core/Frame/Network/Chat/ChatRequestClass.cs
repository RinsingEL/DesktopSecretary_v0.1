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
            public List<Tool> tools; // 使用明确的 Tool 类型
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
            public string description = "根据用户输入或上下文生成带有情感的回复消息，回复包括消息内容和反映语气或情感的情感类型。";
            public Parameters parameters = new Parameters();

            [Serializable]
            public class Parameters
            {
                public string type = "object";
                public Properties properties = new Properties();
                public string[] required = new string[] { "replyContent", "emotion" };
                public bool additionalProperties = false;
            }

            [Serializable]
            public class Properties
            {
                public Property replyContent = new Property { type = "string", description = "回复消息内容，例如 '干得漂亮，任务完成了！' 或 '抱歉，我找不到那个。'" };
                public EmotionProperty emotion = new EmotionProperty();
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
                public string description = "只有情感强烈是才会从以下枚举里面选择表情展现，有 'happy'、'sad'、'confuse'、'mad'、'shy'";
                public string[] @enum = new string[] { "happy", "sad", "confuse", "mad", "shy" }; // @enum 用于避免 C# 关键字冲突
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
                    public FunctionCall function_call; // 保留以兼容旧代码
                    public ToolCall[] tool_calls; // 新增支持工具调用
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
            }
        }
    }
}