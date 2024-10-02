using System;
using System.IO;
using System.Text;

public class IniFile
{
    private string _filePath;

    public IniFile(string filePath)
    {
        _filePath = filePath;
    }

    public string ReadValue(string section, string key)
    {
        var buffer = new StringBuilder(255);
        GetPrivateProfileString(section, key, "", buffer, buffer.Capacity, _filePath);
        return buffer.ToString();
    }

    public void WriteValue(string section, string key, string value)
    {
        WritePrivateProfileString(section, key, value, _filePath);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);
}
