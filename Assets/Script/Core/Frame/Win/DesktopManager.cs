using Com.Module.Chat;
using Com.Module.Schedule;
using Core.Framework.FGUI;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class DesktopManager : MonoBehaviour
{
    private IntPtr hwnd;

    #region Win API �����ͺ���
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

    // ȫ���ȼ���س���
    private const int HOTKEY_ID = 1; // �ȼ� ID
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

        // ���ô�����ʽ���ޱ߿��ޱ�������
        int intExTemp = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, intExTemp | WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);
        SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_BORDER & ~WS_CAPTION);

        // ���ô���λ�ã����½ǣ�
        int currentX = Screen.currentResolution.width - 900;
        int currentY = Screen.currentResolution.height - 800;
        SetWindowPos(hwnd, -1, currentX, currentY, 1200, 900, SWP_SHOWWINDOW);

        // ��չ���ڵ��ͻ�������ʵ��͸��
        var margins = new MARGINS() { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        // ���ñ���͸������ɫ���ֿɴ�͸��
        SetLayeredWindowAttributes(hwnd, 0, 255, LWA_COLORKEY);

        // ��ʼ������
        InitializeTray();

        // ע��ȫ���ȼ�
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
                Text = "��������",
                Visible = true,
                Icon = LoadTrayIcon(Application.streamingAssetsPath + "/icon.png", 100, 100)
            };

            var closeMenu = new System.Windows.Forms.MenuItem("�ر�");
            var settingsMenu = new System.Windows.Forms.MenuItem("����");
            var calendarMenu = new System.Windows.Forms.MenuItem("�ճ�");
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

    #region ȫ���ȼ�ʵ��
    private void RegisterGlobalHotKey()
    {
#if UNITY_EDITOR
        // ע�� Ctrl + S �������ô���
        //if (!RegisterHotKey(hwnd, HOTKEY_ID, MOD_CONTROL, VK_G))
        //{
        //    GUIManager.Instance.ShowWindow<ChatWindow>();
        //}
#endif
    }

    // �����ȼ���Ϣ����Ҫ Unity ��Ϣѭ��֧�֣�
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
        // ����ڲ���ʾ���ô���
        SetForegroundWindow(hwnd);
#endif
    }
    #endregion

    #region �������봰������
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
            // ʧȥ����ʱ�������д���
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
        // ע��ȫ���ȼ�
        UnregisterHotKey(hwnd, HOTKEY_ID);
#endif
    }

    // �������ⲿ�����Դ����ȼ���Ϣ
    public void ProcessHotKeyMessage(int id)
    {
        OnHotKeyPressed(id);
    }
}