using Com.Module.Chat;
using Com.Module.Schedule;
using Core.Framework.FGUI;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class DesktopManager : MonoBehaviour
{
    private IntPtr hwnd;

    #region Win API 常量和函数
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    private static extern uint SetLayeredWindowAttributes(IntPtr hwnd, int crKey, int bAlpha, int dwFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_BORDER = 0x00800000;
    private const int WS_CAPTION = 0x00C00000;
    private const int SWP_SHOWWINDOW = 0x0040;
    private const int LWA_COLORKEY = 0x00000001;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int SW_HIDE = 0;
    private const int SW_RESTORE = 9;

    // 全局热键相关常量
    private const int HOTKEY_ID = 1; // 热键 ID
    private const uint MOD_CONTROL = 0x0002; // Ctrl
    private const uint HOTKEYF_ALT = 0x0004;//Alt
    private const uint VK_G = 0x47; // G
    #endregion

    private static DesktopManager instance;
    public static DesktopManager Instance => instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDesktopSettings();
    }

    private void InitializeDesktopSettings()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;

#if !UNITY_EDITOR
        hwnd = FindWindow(null, Application.productName);
        if (hwnd == IntPtr.Zero)
        {
            Debug.LogError("Failed to find window handle!");
            return;
        }

        // 设置窗口样式（无边框、无标题栏）
        int intExTemp = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, intExTemp | WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
        SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_BORDER & ~WS_CAPTION);

        // 设置窗口位置（右下角）
        int currentX = Screen.currentResolution.width - 900;
        int currentY = Screen.currentResolution.height - 800;
        SetWindowPos(hwnd, -1, currentX, currentY, 1200, 900, SWP_SHOWWINDOW);

        // 扩展窗口到客户端区域，实现透明
        var margins = new MARGINS() { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        // 设置背景透明（黑色部分可穿透）
        SetLayeredWindowAttributes(hwnd, 0, 255, LWA_COLORKEY);

        // 初始化托盘
        InitializeTray();

        // 注册全局热键
        RegisterGlobalHotKey();
#endif
    }

    private System.Windows.Forms.NotifyIcon notifyIcon;

    private void InitializeTray()
    {
#if !UNITY_EDITOR
        try
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = "桌面秘书",
                Visible = true,
                Icon = LoadTrayIcon(Application.streamingAssetsPath + "/icon.png", 100, 100)
            };

            var closeMenu = new System.Windows.Forms.MenuItem("关闭");
            var settingsMenu = new System.Windows.Forms.MenuItem("设置");
            var calendarMenu = new System.Windows.Forms.MenuItem("日程");
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new[] { closeMenu, settingsMenu, calendarMenu });
            closeMenu.Click += (sender, e) =>
            {
                notifyIcon.Visible = false;
                Application.Quit();
            };
            settingsMenu.Click += (sender, e) =>
            {
                GUIManager.Instance.ShowWindow<SettingWindow>();
            };
            calendarMenu.Click += (sender, e) =>
            {
                GUIManager.Instance.ShowWindow<CalendarWindow>();
            };
            notifyIcon.MouseDoubleClick += (sender, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    SetForegroundWindow(hwnd);
                }
            };
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
#endif
    }

    private System.Drawing.Icon LoadTrayIcon(string iconPath, int width, int height)
    {
        try
        {
            var bitmap = new System.Drawing.Bitmap(iconPath);
            var resizedBitmap = new System.Drawing.Bitmap(bitmap, width, height);
            return System.Drawing.Icon.FromHandle(resizedBitmap.GetHicon());
        }
        catch (Exception e)
        {
            return System.Drawing.SystemIcons.Application;
        }
    }

    #region 全局热键实现
    private void RegisterGlobalHotKey()
    {
#if UNITY_EDITOR
        // 注册 Ctrl + S 呼出设置窗口
        //if (!RegisterHotKey(hwnd, HOTKEY_ID, MOD_CONTROL, VK_G))
        //{
        //    GUIManager.Instance.ShowWindow<ChatWindow>();
        //}
#endif
    }

    // 处理热键消息（需要 Unity 消息循环支持）
    public void OnHotKeyPressed(int id)
    {
        if (id == HOTKEY_ID)
        {
            ShowWindowByHotkey();
        }
    }

    private void ShowWindowByHotkey()
    {
#if !UNITY_EDITOR
        // 激活窗口并显示设置窗口
        SetForegroundWindow(hwnd);
#endif
    }
    #endregion

    #region 焦点检测与窗口隐藏
    private bool isFocused = true;

    private void Update()
    {
        CheckWindowFocus();
    }

    private void CheckWindowFocus()
    {
#if !UNITY_EDITOR
        IntPtr foregroundWindow = GetForegroundWindow();
        bool newFocusState = (foregroundWindow == hwnd);

        if (newFocusState != isFocused)
        {
            isFocused = newFocusState;
            OnFocusChanged();
        }
#endif
    }

    private void OnFocusChanged()
    {
        if (!isFocused)
        {
            // 失去焦点时隐藏所有窗口
            foreach (var window in GUIManager.Instance._activeWindows.Values)
            {
                window.Hide();
            }
        }
    }
    #endregion

    void OnDestroy()
    {
#if !UNITY_EDITOR
        if (notifyIcon != null)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
        // 注销全局热键
        UnregisterHotKey(hwnd, HOTKEY_ID);
#endif
    }

    // 新增：外部调用以处理热键消息
    public void ProcessHotKeyMessage(int id)
    {
        OnHotKeyPressed(id);
    }
}