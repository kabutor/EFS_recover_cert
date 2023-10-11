# EFS recover cert
You have a yellow icon on some of your files, and can't open them, somehow you enabled file/folder encryption on Windows and now you can't read your own files, main reason is that someone deleted a certificate on the cert manager and now all your info is lost. Also the opposite, you encypted some files, so no one can read them but you, now you want all the info to be unrecoveable, just delete the certificate and data is lost forever.

Well, I have some news, when you go to the certmgr.msc and delete a certificate, Windows does not really delete it, you can still recover it, as long as the user profile is still intact. Let's dive in

# Theory
When you ask windows to encrypt a folder/files, it creates a certificate, this certificate consists on two parts a public certificate and a private key. To know the fingerprint id of the certificate used to encrypt a file you can use the cipher.exe tool that is already in windows with the /c parameter:
![image](https://github.com/kabutor/EFS_recover_cert/assets/43006263/8f9f0393-de72-4f74-a5ba-f2996e169377)

## Public certificate
The public cert is sitting on your roaming profile %user%\AppData\Roaming\Microsoft\SystemCertificates\My\Certificates\ with the same name as the fingerprint id
![image](https://github.com/kabutor/EFS_recover_cert/assets/43006263/c681e8c9-7f5b-4f18-a874-bcdbcf465903)

The moment you delete certificate, that file is gone.
## Private Key
This is faster, because the private key is encripted using DPAPI on the %user%\AppData\Roaming\Microsoft\Crypto\RSA\<SID>\ folder and when you delete the certificate the private key is not deleted, that is right. Should it be gone once I delete my certificate? - Yes -  Why is not deleted? No idea.

# Recover
You have to recover the public and the private parts, and then later stick them together
## Public certificate
The file with the public certificate is gone, but windows still keeps a record in the registry (why?), as binary blob in 
HKEY_CURRENT_USER\SOFTWARE\Microsoft\SystemCertificates\TrustedPeople\Certificates\<thumbprint_id>

![image](https://github.com/kabutor/EFS_recover_cert/assets/43006263/967d8cd3-8858-4c40-90ac-466b9749d649)

To export that blob as a file you can use the reg_blob.exe, you can download it from this repository, (also provided the source code as reg_blob.cs in case you want to compile it) with the fingerprint id as a parameter. You get the same file as you have in the Microsoft\SystemCertificates\My\Certificates\
'''
reg_blob.exe 3F0899CD824828B156114C0DF4CC7E21BA5E0C7C
'''

TODO : explain how to use binwalk to strip the header from the blob

## Private Key
The key was not deleted, but 

dpapi::capi /in:"C:\temp\Crypto\S-1-5-21-809848743-1371230335-2595545360-1001\566b9cb8d95dc0c0cfe359c9ffeaf252_2c04acab-f7da-4489-b9da-08efb0051201"

dpapi::masterkey /in:"C:\temp\Crypto\Protect\S-1-5-21-809848743-1371230335-2595545360-1001\bb1af8be-89cb-435f-80ed-1379bee29c53" /password:USERPASSWORD
  sha1: f6d3e18299cc3502af58cbc01e1eea09e1d41972

mimikatz # dpapi::capi /in:"C:\temp\Crypto\S-1-5-21-809848743-1371230335-2595545360-1001\566b9cb8d95dc0c0cfe359c9ffeaf252_2c04acab-f7da-4489-b9da-08efb0051201" /masterkey:f6d3e18299cc3502af58cbc01e1eea09e1d41972

 
