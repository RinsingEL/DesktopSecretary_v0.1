using System;
using UnityEngine;
namespace Core.Framework.Network
{
    [Serializable]
    public class GenerateMemory
    {
        public string description = "if there're important information needs to store, call this function. e.g.'i like tea but not milk'";
        public string name = "generateMemory";
        [Serializable]
        public class GenerateMemory_parameters
        {
            public bool additionalProperties = false;
            [Serializable]
            public class GenerateMemory_parameters_properties
            {
                [Serializable]
                public class GenerateMemory_parameters_properties_memory
                {
                    public string description = "the memory generated in Chinese , e.g.'用户喜欢晚上做作业'";
                    public string type = "string";
                }

                public GenerateMemory_parameters_properties_memory memory = new GenerateMemory_parameters_properties_memory();
                [Serializable]
                public class GenerateMemory_parameters_properties_replaceMemory
                {
                    public string description = "the content reply user";
                    public string type = "string";
                }

                public GenerateMemory_parameters_properties_replaceMemory reply = new GenerateMemory_parameters_properties_replaceMemory();
            }

            public GenerateMemory_parameters_properties properties = new();
            public string[] required = { "memory", "reply" };
            public string type = "object";
        }

        public GenerateMemory_parameters parameters = new GenerateMemory_parameters();
    }
    [Serializable]
    public class GenerateMemory_response_arr
    {
        public string memory;
        public string reply;
    }
}

