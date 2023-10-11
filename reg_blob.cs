
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
        List<byte> derFile = new List<byte>();
        List<byte> idFile = new List<byte>();

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

                                    for (int i=0;i < bytes.Length; i++)
                                        {
                                            if (bytes[i] == 0x30)
                                                {
                                                if (bytes[i+1] == 0x82)
                                                    {
                                                    Console.WriteLine("saving DER");
                                                    for (int j=0; i < bytes.Length; j++)
                                                        {
                                                            derFile.Add(bytes[i]);
                                                            i++;
                                                        }
                                                    break;
                                                    }
                                                }
                                                else
                                                {
                                                idFile.Add(bytes[i]);
                                                }
                                        }
                                    
                                    File.WriteAllBytes(args[0], derFile.ToArray());
                                }
                               Console.WriteLine(System.Text.Encoding.Unicode.GetString(idFile.ToArray()));

                             }
             }
             Console.WriteLine(key);
          }

        
    }  
}
