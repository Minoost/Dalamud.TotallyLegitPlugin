using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Security;
using Microsoft.Win32.SafeHandles;

namespace SamplePlugin.Win32;

internal sealed class ProcessToken : IDisposable
{
    private readonly SafeFileHandle mProcessToken;
    
    public ProcessToken(SafeHandle hProcess)
    {
        var success = PInvoke.OpenProcessToken(hProcess, TOKEN_ACCESS_MASK.TOKEN_QUERY, out mProcessToken);
        if (!success)
        {
            throw new Win32Exception();
        }
    }
    
    public void Dispose()
    {
        mProcessToken.Dispose();
    }
    
    public static implicit operator SafeFileHandle(ProcessToken token) => token.mProcessToken;
}
