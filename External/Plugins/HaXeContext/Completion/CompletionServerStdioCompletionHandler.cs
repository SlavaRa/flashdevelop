using System.Diagnostics;
using PluginCore;
using ProjectManager.Projects.Haxe;
using PluginCore.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HaXeContext
{
    public class CompletionServerStdioCompletionHandler : ICompletionServerCompletionHandler
    {
        public event FallbackNeededHandler FallbackNeeded;

        readonly Process haxeProcess;
        bool listening;
        bool failure;

        public CompletionServerStdioCompletionHandler(Process haxeProcess)
        {
            this.haxeProcess = haxeProcess;
        }

        public bool IsRunning()
        {
            try { return !haxeProcess.HasExited; } 
            catch { return false; }
        }

        ~CompletionServerStdioCompletionHandler() => Stop();

        public string GetCompletion(string[] args) => GetCompletion(args, null);

        public string GetCompletion(string[] args, string fileContent)
        {
            if (args == null || haxeProcess == null) return string.Empty;
            if (!IsRunning()) StartServer();
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("--cwd \"" + ((HaxeProject) PluginBase.CurrentProject).Directory + "\"");
                foreach (var it in args)
                    sb.AppendLine(it);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    sb.Append("\x01");
                    sb.Append(fileContent);
                }
                else sb.Append("\0");
                var data = sb.ToString();
                var bytes = Encoding.UTF8.GetBytes(data);
                var writer = new BinaryWriter(haxeProcess.StandardInput.BaseStream);
                writer.Write(BitConverter.GetBytes(bytes.Length));
                writer.Write(bytes);
                writer.Flush();
                return "";
            }
            catch(Exception ex)
            {
                TraceManager.AddAsync(ex.Message);
                if (!failure) FallbackNeeded?.Invoke(false);
                failure = true;
                return string.Empty;
            }
        }

        public void StartServer()
        {
            if (haxeProcess == null || IsRunning()) return;
            haxeProcess.StartInfo.RedirectStandardInput = true;
            haxeProcess.Start();
            if (listening) return;
            listening = true;
            haxeProcess.BeginOutputReadLine();
            haxeProcess.BeginErrorReadLine();
            haxeProcess.OutputDataReceived += haxeProcess_OutputDataReceived;
            haxeProcess.ErrorDataReceived += haxeProcess_ErrorDataReceived;
            haxeProcess.WaitForExit();
        }

        void haxeProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            TraceManager.AddAsync(e.Data, 2);
        }

        void haxeProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null || !Regex.IsMatch(e.Data, "Error.*--wait")) return;
            if (!failure) FallbackNeeded?.Invoke(true);
            failure = true;
        }

        public void Stop()
        {
            if (IsRunning()) haxeProcess.Kill();
        }
    }
}
