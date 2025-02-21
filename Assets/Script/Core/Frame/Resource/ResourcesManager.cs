using Core.Framework.FGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Resource
{
    public class ResourcesManager : MonoBehaviour
    {
        private static ResourcesManager _instance;
        public static ResourcesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ResourceManager");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<ResourcesManager>();
                }
                return _instance;
            }
        }
        public FGUIResourceManager FGUIResourceManager;
        public DBSourceManager DBSourceManager;
        void Awake()
        {
            FGUIResourceManager = gameObject.AddComponent<FGUIResourceManager>();
            DBSourceManager = gameObject.AddComponent<DBSourceManager>();
            DBSourceManager.Init();
        }

    }
}

