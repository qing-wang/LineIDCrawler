# 現在開始規劃一個 LineIDAnalyzer 的程式，它的組態是
    * 使用 C# 程式語言
    * 使用 Win Form GUI 框架
    * 運行在 .NET 8.0 之上
    * 使用 Visual Studio 2026 方案及專案檔案

# 它的主要用途是串接 ChatGPT 的 LLM API 然後透過演算法來分析出使用者所輸入的一段文字是否含有該使用者提供的 Line ID
    * 有的話，把該 Line ID 萃取出來

# Main Form
    * 將 Form1 更名為 LineIDAnalyzerUI，包括 .cs 檔名及類別名稱。表單的標題改為 LineIDAnalyzer。

# Business Logic
    * 主要的分析能力都放在 LineIDAnalyzer 類別裡，以便日後讓其他專案直接以 Class Library 的方式來引用。

# 主表單待分析文字
    * 在主表單上加一個 TextBox 元件，讓使用者輸入待分析之文字。

# 主表單按鈕群
    * 在主表單的最頂端部份，設計為 "主表單按鈕群"，用來擺放觸發主要功能的按鈕們。
        * 分析
            + 進行所輸入之文字是否含 LineID 的分析
        * 設定
            + 設定 ChatGPT API Key

# Console Log
    * 幫主表單加一個 TextBox 元件，名字叫做 tbConsoleLog，用來顯示系統運行日誌，同時間要能顯示多行內容，並且在內容太多時可以往回捲動。
    * 其中的內容不可以修改。位置就放在主表單的按鈕群下方。
    * 日誌輸入訊息請使用正體中文。
    * 區分日誌的等級，分為資訊、錯誤、除錯三種。
    * 日誌的輸出，除了輸出到 tbConsoleLog 之後，也輸出到執行目錄下名為 logs 的目錄裡的日誌檔案。每個日誌檔案用 [yyyyMMdd].log 的格式來命名。    
    * 把 tbConsoleLog 依等級用不同顏色顯示（錯誤紅、除錯灰、資訊正常）
    * 日誌輸出訊息請使用正體中文。

# 使用 logger 來記錄系統日誌
    * 使用 daily roller appender，除了 Console 之外，也同時輸出到每天滾動的日誌檔案去
    * 日誌檔案置於執行路徑的 logs 目錄下
    * 捕捉到的 Exception 也要輸出到日誌去
    * 日誌記錄要有時間點
    * 所有動作記錄必要的日誌

# SQlite
    * 使用 SQLite 來做為本地端的資料庫，SQLite 部份使用 Microsoft.Data.Sqlite。
    * 建立一個叫做 DatabaseManager 的類別來提供所有對資料庫存取的 methods。
    * 記得將 ExecuteReader 的使用改為具體的 using 區塊以確保 reader 在變更 CommandText 前關閉

# 有任何我遺漏、你覺得需要加上的功能，再跟我建議。另外，有任何沒考慮周到的地方，也請你補充。            


# 現在開始設計從一段文字中分析寫該段文字的人是否有在該文字中留下 Line 的 ID，以及如果有的話，他留下的 Line ID 為何。
    * 針對這個演算方法，你有什麼建議或看法？