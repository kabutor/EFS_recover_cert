
// Compile this: 
// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:reg_blob.exe reg_blob.cs

using System;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;



public class Program
{
    public static void Main(string[] args)
    {
        byte[] bytes;
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\SystemCertificates\\TrustedPeople\\Certificates\\" + args[0] ))
          {
            if (key != null)
            {
                        Object o = key.GetValue("Blob");
                             if (o != null)
                               {
                                BinaryFormatter bf = new BinaryFormatter();
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    bf.Serialize(ms, o);
                                    bytes =  ms.ToArray();
                                    File.WriteAllBytes(args[0],bytes);
                                }
                            
                             // Console.WriteLine( Encoding.Unicode.GetString(bytes[63:80], 0, 10) );
                             }
             }
             Console.WriteLine(key);
          }

        
    }  
}
