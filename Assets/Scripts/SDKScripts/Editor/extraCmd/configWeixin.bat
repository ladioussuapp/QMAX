cd /d %~dp0

set enName=%1%
set channelId=%2%
set packageName=%3%
set versionCode=%4%
set versionName=%5%
set appkeys=%6%
set sdklib=%7%
rem channelid = -1时 没有子渠道
echo enName=%enName%    
echo channelId=%channelId%    
echo packageName=%packageName%
echo versionCode=%versionCode%
echo versionName=%versionName%
echo appkeys=%appkeys%
echo sdklib=%sdklib%
echo -----------mkdir path---------


rem GameMaincpp.java 参数替换
set f="..\src\com\yasdksdemo\GameMaincpp.java"
set src=platformVar
set dst=%appkeys%

java -jar .\ReplaceJar_fat.jar %f% %src% %dst%
rem PackagePlaceholder
java -jar .\ReplaceJar_fat.jar ..\AndroidManifest.xml PackagePlaceholder %packageName%

if %enname%==baiduyidong copy /Y .\PlatformSDKApplication.java ..\src\com\yasdksdemo\PlatformSDKApplication.java
