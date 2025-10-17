# Smart DNS Proxy VPN Client

Desktop app to connecting with SDP

## Code signing -- New method
Signing is currently included in post build events. In order for it to work first you have to import certificate into your windows certificate store.
First double click on your cert, then just follow these screenshots:
![Select current user](Certificate_import/1.png?raw=true)
![Type in password and make sure, that these 2 checkboxes from bottom are checked.](Certificate_import/2.png?raw=true)
![Leave it at auto](Certificate_import/3.png?raw=true)
Thats it!

## Code signing -- Old method

Use Developer Command Prompt for VS and signtool

### First sign the built executable app file

Explanation:

* certificate_path --- specify the path to .pfx file
* certificate_password --- password to certificte
* ${project_directory}  --- location where you have project checked out

Sign the exe file which is later taken in installer build process using following command:

```
signtool sign /f certificate_path /p certificate_password /tr http://timestamp.comodoca.com /fd SHA256 "${project_directory}\obj\Release\Smart DNS Proxy VPN Client.exe"
```

After that build the installer project using Visual Studio.

###Sign the final install file

Sign with following command:

```
signtool sign /f certificate_path /p certificate_password /tr http://timestamp.comodoca.com /fd SHA256 /d "Smart DNS Proxy VPN" "${project_directory}\VPNClientSetup\Release\Smart DNS Proxy VPN Client.msi"
```


## Built With

* [MaterialSkin](https://github.com/IgnaceMaes/MaterialSkin) - UI Skin (Material Design)
* [MetroFramework](https://github.com/dennismagno/metroframework-modern-ui) - UI framework (Metro Style)


## Authors

* **KK** - *Project Manager*
* **KJ** - *Tech Leader*
* **KO** - *Dev Knight*
* **KP** - *Dev Knight*
* **TA** - *Dev Knight*
