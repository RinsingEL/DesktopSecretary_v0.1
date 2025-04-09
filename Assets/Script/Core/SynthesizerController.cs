using UnityEngine;
using System.Diagnostics;
using System.IO;

public class SynthesizerController : MonoBehaviour
{
    private string pythonPath = "python";
    private string scriptPath = @"E:\PythonProject\synthesizer.py";
    private string workingDir = @"E:\PythonProject";
    private string commandFile = @"E:\PythonProject\command.txt";
    private Process pythonProcess;

    void Start()
    {
        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = "cmd.exe";
        pythonProcess.StartInfo.Arguments = $"/K {pythonPath} \"{scriptPath}\"";
        pythonProcess.StartInfo.WorkingDirectory = workingDir;
        pythonProcess.StartInfo.UseShellExecute = true;  // 使用 Shell 显示窗口
        pythonProcess.Start();

    }

    public void Synthesize(string text)
    {
        // 写入合成指令到文件
        File.WriteAllText(commandFile, $"synth:{text}", System.Text.Encoding.UTF8);
    }

    void OnApplicationQuit()
    {
        // 写入关闭指令
        File.WriteAllText(commandFile, "close", System.Text.Encoding.UTF8);
        if (!pythonProcess.HasExited)
        {
            pythonProcess.WaitForExit(5000);  // 等待 5 秒
            if (!pythonProcess.HasExited)
                pythonProcess.Kill();  // 强制关闭
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Synthesize("你好呀，这里是测试语音");
        }
    }
}