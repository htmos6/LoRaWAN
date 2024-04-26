# LoRaWAN-Server

## SSL Certificate Creation
- SSL Key and Password generated __easily__ using Windows Powershell in Administrator Mode.

```$cert = New-SelfSignedCertificate -DnsName "LoRaWAN" -CertStoreLocation "cert:\LocalMachine\My" Export-PfxCertificate -Cert $cert -FilePath "LoRaWAN.pfx" -Password (ConvertTo-SecureString -String "sTrongPassW1" -Force -AsPlainText)```


## Resolving Access Issues
- Access denial while reading SSL file from the client side and inability to read messages from the server.
- Run the following command in Powershell with Admin privileges:

```icacls C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys /inheritance:r /grant Administrators:F /grant:r Everyone:RW```
