using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

public static class Injector
{
    public static void doshit(String strProcessName, String strDLLName)
    {
        Int32 ProcID = GetProcessId(strProcessName);
        if (ProcID >= 0)
        {
            IntPtr hProcess = (IntPtr)OpenProcess(0x1F0FFF, 1, ProcID);
            if (hProcess == null)
            {
                return;
            }
            else
                InjectDLL(hProcess, strDLLName);
        }
    }

    [DllImport("kernel32")]
    public static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        UIntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        out IntPtr lpThreadId
    );

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(
        UInt32 dwDesiredAccess,
        Int32 bInheritHandle,
        Int32 dwProcessId
    );

    [DllImport("kernel32.dll")]
    public static extern Int32 CloseHandle(
        IntPtr hObject
    );

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern bool VirtualFreeEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        UIntPtr dwSize,
        uint dwFreeType
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
    public static extern UIntPtr GetProcAddress(
        IntPtr hModule,
        string procName
    );

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAllocEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        uint dwSize,
        uint flAllocationType,
        uint flProtect
    );

    [DllImport("kernel32.dll")]
    static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        string lpBuffer,
        UIntPtr nSize,
        out IntPtr lpNumberOfBytesWritten
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(
        string lpModuleName
    );

    [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
    internal static extern Int32 WaitForSingleObject(
        IntPtr handle,
        Int32 milliseconds
    );

    public static Int32 GetProcessId(String proc)
    {
        try
        {
            Process[] ProcList;
            ProcList = Process.GetProcessesByName(proc);
            return ProcList[0].Id;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static void InjectDLL(IntPtr hProcess, String strDLLName)
    {
        IntPtr bytesout;
        Int32 LenWrite = strDLLName.Length + 1;
        IntPtr AllocMem =
            (IntPtr)VirtualAllocEx(hProcess, (IntPtr)null, (uint)LenWrite, 0x1000, 0x40);
        WriteProcessMemory(hProcess, AllocMem, strDLLName, (UIntPtr)LenWrite, out bytesout);
        UIntPtr Injector = (UIntPtr)GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
        if (Injector == null) return;

        IntPtr hThread = (IntPtr)CreateRemoteThread(hProcess, (IntPtr)null, 0, Injector, AllocMem, 0, out bytesout);

        if (hThread == null) return;
        int Result = WaitForSingleObject(hThread, 10 * 1000);
        if (Result == 0x00000080L || Result == 0x00000102L || Result == 0xFFFFFFFF)
        {
            if (hThread != null) CloseHandle(hThread);
            return;
        }
        Thread.Sleep(1000);
        VirtualFreeEx(hProcess, AllocMem, (UIntPtr)0, 0x8000);
        if (hThread != null) CloseHandle(hThread);
        return;
    }
}