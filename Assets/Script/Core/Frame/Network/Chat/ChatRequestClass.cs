using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Network;
using System;
using UnityEngine.UIElements;
namespace Core.Framework.Network.ChatSystem
{
    public static class ChatRequestClass
    {
        [Serializable]
        public class ChatReuestBody
        {
            [Serializable]
            public class Message
            {
                public string role;
                public string content;
                public Message() { }
            }

            [Serializable]
            public class FunctionCall
            {
                public string name;
            }

            public string model;
            public List<Message> messages;
            public bool safe_mode;
            public List<object> functions;
            public object function_call;
        }

        [Serializable]
        public class SelectFunctionCalling
        {
            public string name = "generateSelectQuery";
            public string description = "This function generates a SQL SELECT query for the 'Tasks' table in a local SQLite database based on user intent. Database schema: Table 'Tasks': - TaskID (int, primary key) - Title (text, not null) - Description (text, nullable) - Priority (int, default 2) - Status (int, default 0) - DueDate (datetime, nullable) - StartedAt (datetime, not null) - UpdatedAt (datetime, not null).";
            public ParametersSelect parameters = new ParametersSelect();
        }

        [Serializable]
        public class ParametersSelect
        {
            public string type = "object";
            public PropertiesSelect properties = new PropertiesSelect();
            public string[] required = new string[] { "intent", "generatedSelect" };
            public bool additionalProperties = false;
            public bool strict = true; // 移到 parameters 内部
        }

        [Serializable]
        public class PropertiesSelect
        {
            public Intent intent = new Intent();
            public GeneratedSelect generatedSelect = new GeneratedSelect();
        }

        [Serializable]
        public class Intent
        {
            public string type = "string";
            public string description = "The user's intent for querying data, e.g., 'find tasks due today'.";
        }

        [Serializable]
        public class GeneratedSelect
        {
            public string type = "string";
            public string description = @"The generated SQL SELECT statement, e.g., 'SELECT * FROM Tasks WHERE DueDate = ""2025-03-05""'.
                                          'SELECT * FROM Tasks WHERE StartAt = ""2025-03-05""'";
        }
        // CRUD 操作类
        [Serializable]
        public class CrudFunctionCalling
        {
            public string name = "generateCrudQuery";
            public string description = @"This function generates a SQL query (INSERT, UPDATE, DELETE, or SELECT) for the 'Tasks' table in a local SQLite database based on user intent. 
                                         Database schema: Table 'Tasks': 
                                         - TaskID (int, primary key) 
                                         - Title (text, not null) 
                                         - Description (text, nullable) 
                                         - Priority (int, default 2) 
                                         - Status (int, default 0) 
                                         - DueDate (datetime, nullable) 
                                         - StartedAt (datetime, not null) 
                                         - UpdatedAt (datetime, not null). ";
            public ParametersCrud parameters = new ParametersCrud();
            [Serializable]
            public class ParametersCrud
            {
                public string type = "object";
                public PropertiesCrud properties = new PropertiesCrud();
                public string[] required = new string[] { "intent", "generatedQuery", "undoQuery" };
                public bool additionalProperties = false;
                public bool strict = true;
            }

            [Serializable]
            public class PropertiesCrud
            {
                public Intent intent = new Intent();
                public GeneratedQuery generatedQuery = new GeneratedQuery();
                public UndoQuery undoQuery = new UndoQuery();
            }

            [Serializable]
            public class CrudIntent
            {
                public string type = "string";
                public string description = "The user's intent for the CRUD operation, e.g., 'add a task with title \"Meeting\" due tomorrow' or 'delete task with ID 1'.";
            }

            [Serializable]
            public class GeneratedQuery
            {
                public string type = "string";
                public string description = "The generated SQL query, e.g., 'INSERT INTO Tasks (Title, DueDate, StartedAt, UpdatedAt) VALUES (\"Meeting\", \"2025-03-06\", \"2025-03-05\", \"2025-03-05\")'.";
            }

            [Serializable]
            public class UndoQuery
            {
                public string type = "string";
                public string description = "The SQL query to undo the operation, e.g., 'DELETE FROM Tasks WHERE TaskID = (last_inserted_id)' for INSERT, or 'INSERT INTO Tasks (...) VALUES (...)' for DELETE.";
            }
        }
        [Serializable]
        public class ReplyWithEmotionFunctionCalling
        {
            public string name = "generateReplyWithEmotion";
            public string description = @"This function generates a reply message with an associated emotion based on user input or context. 
                                         The reply includes the content of the message and an emotion type to reflect the tone or sentiment.";
            public ParametersReplyWithEmotion parameters = new ParametersReplyWithEmotion();

            [Serializable]
            public class ParametersReplyWithEmotion
            {
                public string type = "object";
                public PropertiesReplyWithEmotion properties = new PropertiesReplyWithEmotion();
                public string[] required = new string[] { "replyContent", "emotion" };
                public bool additionalProperties = false;
                public bool strict = true;
            }

            [Serializable]
            public class PropertiesReplyWithEmotion
            {
                public ReplyContent replyContent = new ReplyContent();
                public Emotion emotion = new Emotion();
            }

            [Serializable]
            public class ReplyContent
            {
                public string type = "string";
                public string description = "The content of the reply message, e.g., 'Great job completing the task!' or 'Sorry, I couldn’t find that.'.";
            }

            [Serializable]
            public class Emotion
            {
                public string type = "string";
                public string description = "The emotion associated with the reply, e.g., \"happy\", \"sad\", \"confuse\", \"mad\", \"shy\".";
                public string[] enumValues = new string[] { "happy", "sad", "confuse", "mad", "shy" };
            }
        }
    }
    public static class ChatResposeClass
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
                public FunctionCall function_call;
            }

            [Serializable]
            public class FunctionCall
            {
                public string name;
                public string arguments; //要进一步解析
            }
            [Serializable]
            public class SelectFunctionArgu
            {
                public string intent;
                public string generatedSelect;
            }
            [Serializable]
            public class Usage
            {
                public int prompt_tokens;
                public int completion_tokens;
                public int total_tokens;
            }
            [Serializable]
            public class CrudFunctionArgu
            {
                public string crudIntent;
                public string generatedQuery;
                public string undoQuery;
            }
            [Serializable]
            public class EmotionArgu
            {
                public string replyContent;
                public string emotion;
            }
        }
    }
}

