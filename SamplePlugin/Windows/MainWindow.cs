using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using SamplePlugin.Win32;

namespace SamplePlugin.Windows;

public sealed class MainWindow : IDisposable
{
    public bool IsVisible { get; set; } = true;

    private readonly uint mIsAppContainer;
    private readonly uint mIsLpac; // "LessPrivilegedAppContainer" (Windows 10 RH2+)
    private readonly string? mAppContainerSid;
    
    public unsafe MainWindow()
    {
        BOOL success;
        TOKEN_APPCONTAINER_INFORMATION acInfo = default;
        
        using var processHandle = PInvoke.GetCurrentProcess_SafeHandle();
        using var processToken = new ProcessToken(processHandle);
        
        fixed (uint* pIsAppContainer = &mIsAppContainer)
        fixed (uint* pIsLpac = &mIsLpac)
        {
            PInvoke.GetTokenInformation(processToken, TOKEN_INFORMATION_CLASS.TokenIsAppContainer, pIsAppContainer,
                                        (uint)Marshal.SizeOf(mIsAppContainer), out _);
            PInvoke.GetTokenInformation(processToken, TOKEN_INFORMATION_CLASS.TokenIsLessPrivilegedAppContainer, pIsLpac,
                                        (uint)Marshal.SizeOf(mIsLpac), out _);
        }

        success = PInvoke.GetTokenInformation(processToken, TOKEN_INFORMATION_CLASS.TokenAppContainerSid, &acInfo,
                                    (uint)Marshal.SizeOf(acInfo), out _);
        if (success)
        {
            PInvoke.ConvertSidToStringSid(acInfo.TokenAppContainer, out var pAppContainerSid);
            mAppContainerSid = new string(pAppContainerSid);
            PInvoke.LocalFree((IntPtr)pAppContainerSid.Value);
        }
    }

    public void Dispose()
    {
        
    }

    public void Draw()
    {
        if (!IsVisible)
            return;

        var textIsRunningAppContainer = mIsAppContainer switch
        {
            1 => "Yes",
            _ => "No",
        };
        var textIsRunningLpac = mIsAppContainer switch
        {
            1 => "Yes",
            _ => "No",
        };
        
        ImGui.Text("TokenIsAppContainer: ");
        ImGui.SameLine();
        ImGui.Text(textIsRunningAppContainer);
        
        ImGui.Text("TokenIsLessPrivilegedAppContainer: ");
        ImGui.SameLine();
        ImGui.Text(textIsRunningLpac);
        
        ImGui.Text("AppContainer sid: ");
        ImGui.SameLine();
        ImGui.Text(string.IsNullOrWhiteSpace(mAppContainerSid) ? "None" : mAppContainerSid);

        if (ImGui.Button("Launch cmd"))
        {
            LaunchCmd();
        }
    }

    private void LaunchCmd()
    {
        Process.Start(@"C:\Windows\System32\cmd.exe");
    }
}
