# CommAdapter Project #
* * *

通訊測試SerialPort, UDP, TCP(未實作)

By neilpipi1985

## 開發環境 ##

- Win 7 64bit
- Visual Studio 2017 Community
- 通訊類別庫專案(CommAdapter)(.net framework 3.5)
- UI專案(CommAdapterDemo)(.net framework 4.5.1)(WPF專案(個人不喜歡使用WinForm))(需要使用NuGet取得第三方UI套件)

## 使用說明 ##

- 啟動CommAdapterDemo/bin/CommAdapterDemo.exe
- 按下Excute按鈕
- 選擇通訊模式, IP或是COM Port, Port或是Baud Rate
- 按下Confirm
- 連線完成即可進行收發資料

- 可以同時開啟兩個CommAdapterDemo程式測試傳訊息



## 問題說明 ##

- TCP尚未實作及測試
- 同一個TCP/UDP Port同一個時間只允許一個物件使用
