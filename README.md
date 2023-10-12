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

To export that blob as a file you can use the reg_blob.exe, you can download it from this repository, (also provided the source code as reg_blob.cs in case you want to compile it) with the fingerprint id as a parameter. 
Output will be a file with the fingerprint_id.DER, that is your public certificate, also you get some text, and some garbage, the important part is the numbers in the middle.

```
reg_blob.exe 0B6E9763170C0C5FBE644162AB4EFED2AE3F171C
saving DER
A ???   ? ? ? A ? ??????????  I  l          d b 5 1 e 1 9 5 - d 8 7 9 - 4 3 4 3 - 8 a 8 6 - 8 7 1 2 0 6 5 2 e a 6 5     Microsoft Enhanced Cryptographic Provider v1.  ` A ? ??????????" A ??
HKEY_CURRENT_USER\SOFTWARE\Microsoft\SystemCertificates\TrustedPeople\Certificates\0B6E9763170C0C5FBE644162AB4EFED2AE3F171C
```
This is the Key Container of the certificate, there has to be a key on Crypto\RSA\<SID> with that same number (db51e195-d879-4343-8a86-87120652ea65 in the example, in utf-16)

## Private Key
The key was not deleted, it is sitting in the %user%\AppData\Roaming\Microsoft\Crypto\RSA\<SID>\ folder, you can do this several ways, I like using grep -H on linux, but in windows the easy way is to type all the files in the console and look at the top numbers, the one that has in the top the same id as the Key Container of the public certificate, that is the file with your Private Key.

```
C:\Users\kabutor\AppData\Roaming\Microsoft\Crypto\RSA\S-1-5-21-809848743-1371230335-2595545360-1001> type 7b7b0fed9e49b647c6e908b622e34dc4_a63a392d-e48d-4ee0-a056-8d076995042e

☻%∟☺P♠¶ⁿdb51e195-d879-4343-8a86-87120652ea65RSA ☺☺i@╒└Kúÿ~╔Kod¡╦á↨(hM↨²∟ä²É┐ }*┌├u]öB   î▌qoäq╫█£æì⌐0¶▄☼:»─√oQÆ ε»I♀ìΓ.
0=äpF┘╟:=¬Y║[R╧τZJ╚a▒≡┼4r'EûÅ≡<☼π∞░┤▌ì¿BgR▌í!â▲▄8☼Qfr
[garbage]
```

That is the file with your private key, is encrypted using DPAPI, to decrypt it, again there are several ways to do this, the easy way I think is **disable the antivirus** and use [mimikatz](https://github.com/ParrotSec/mimikatz). 
I'm not going to go deep in dpapi, you can browse my other repositories about certificates, you have to decrypt the key using mimikatz, for that you need three commands

First, the one that will tell you what masterkey is used to encrypt that password, this command will output a lot of information, the only thig we cared about is the guidMasterKey:
```
dpapi::capi /in:"C:\Users\kabutor\AppData\Roaming\Microsoft\Crypto\RSA\S-1-5-21-809848743-1371230335-2595545360-1001\7b7b0fed9e49b647c6e908b622e34dc4_a63a392d-e48d-4ee0-a056-8d076995042e"
guidMasterKey      : {bb1af8be-89cb-435f-80ed-1379bee29c53}
```
Second: Decrypt the masterkey with the user password (the login password, not the pin), this also return a lot of data, important one is the sha1 at the end.
```
dpapi::masterkey /in:"C:\Users\kabutor\AppData\Roaming\Microsoft\Protect\S-1-5-21-809848743-1371230335-2595545360-1001\bb1af8be-89cb-435f-80ed-1379bee29c53" /password:USERPASSWORD
  sha1: f6d3e18299cc3502af58cbc01e1eea09e1d41972
```

Third command: decrypt the private key, using the sha1 output as a masterkey parameter, output will be a pvk file with the private key:
```
mimikatz # dpapi::capi /in:"C:\Users\kabutor\AppData\Roaming\Microsoft\Crypto\S-1-5-21-809848743-1371230335-2595545360-1001\7b7b0fed9e49b647c6e908b622e34dc4_a63a392d-e48d-4ee0-a056-8d076995042e" /masterkey:f6d3e18299cc3502af58cbc01e1eea09e1d41972
Private export : OK - 'raw_exchange_capi_0_db51e195-d879-4343-8a86-87120652ea65.pvk'
```
## Merge bot parts in a pfx/p12 certificate
Third step, you just have to merge both files into a pfx file you can import that in windows
```
openssl.exe x509 -inform DER -outform PEM -in 0B6E9763170C0C5FBE644162AB4EFED2AE3F171C.der -out public.pem

openssl.exe rsa -inform PVK -outform PEM -in raw_exchange_capi_0_db51e195-d879-4343-8a86-87120652ea65.pvk -out private.pem
writing RSA key

openssl.exe pkcs12 -in public.pem -inkey private.pem -password pass:12345 -keyex -CSP "Microsoft Enhanced Cryptographic Provider v1.0" -export -out cert.pfx
```
This will get your cert.pfx, import it using 12345 as the password, once imported you will be able to recover your EFS encrypted files.

# Last Words

I just can't believe that something as important as encrypting your files, is not treated with the proper importance as Microsoft does here. I encrypt some files, and whatever by error or consciously, I delete the certificate, both the private and public part of the certificate should be gone forever. Microsoft treats it's users as fools, this "investigation" came after a experience I had, someone on his workstation have a folder encrypted, no one knows why and months later they just deleted all the certs in the system, they were having some issues with the Government issued certificates and just did a "delete all the certificates, reinstall them". They deleted the EFS certificate used to encrypt all the files, and they can't access all the data.

When I was inspecting the damage, and trying to recover the data, I came with a website that claims that Microsoft have a tool to recover the EFS certificate 

'''     
    If you have your original profile, you can use "reccerts" tool to retrieve the private key to recovery EFS file.
    reccerts.exe -path: "profile path" -password:<password>
    But you have to contact to Microsoft Support to get this tool. 
  '''

Thanks for nothing Microsoft.
