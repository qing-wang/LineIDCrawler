# 現在開始規劃 PTTCrawler 的程式，它的組態是
    * 專案建置到 C:\Users\USER\work\LineIDCrawler\PTTCrawler 目錄下
    * 使用 C# 程式語言
    * 使用 Win Form GUI 框架
    * 運行在 .NET 8.0 之上
    * 使用 Visual Studio 2026 方案及專案檔案

# 它的主要用途是爬取一個叫做 PTT 的 BBS 系統內的貼文，主要功能如下
    * 爬蟲任務
        + 爬蟲任務列表
        + 新增爬蟲任務
        + 修改爬蟲任務
        + 放棄爬蟲任務
        + 刪除爬蟲任務
        + 執行指定的爬蟲任務
    * "爬蟲任務" 的資料會有
        + 任務名稱
        + 任務性質 (先支援一種，但日後可擴充)
            - 收集 Line ID
                . 手動輸入單一看版網址
        + 爬取貼文關鍵字
            - 不指定關鍵字，全爬
            - 含指定關鍵字之貼文才爬取
        + 爬取頁數上限
            - 可指定爬取時最多爬取的頁數上限，避免爬取太多過時的資料
            - 也可設定為不限
        + 建立時間
    * 在主介面上增加 "爬蟲任務" 的按鈕，按下後，開啟獨立的表單來提供上述的 "爬蟲任務功能"

# Main Form
    * 將 Form1 更名為 PTTCrawlerUI，包括 .cs 檔名及類別名稱。表單的標題改為 PTT Crawler。

# 主表單按鈕群
    * 在主表單的最頂端部份，設計為 "主表單按鈕群"，用來擺放觸發主要功能的按鈕們。
        * 爬蟲任務

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

# 看版貼文網頁結構
    * 看版網址 
        + 若看版 ID 為 "AllTogether" 
        + https://www.ptt.cc/bbs/AllTogether/index.html
    * 看版裡的搜尋列
        <input class="query" type="text" name="q" value="" placeholder="搜尋文章⋯">
        + 在上述元素輸入關鍵字之後，再輸入換行，便可以進行貼文的搜尋，也只會列出含指定關鍵字的貼文
    * 貼文列
        + 外容器
            <div class="r-ent">
        + 貼文推數
            <div class="nrec"> 的 inner text
        + 貼文標題外容器
			<div class="title">
            - 貼文網址
                其中的 <a> 的 href 屬性
                . 貼文 ID
                    由貼文網址來剖析
                    % 例：https://www.ptt.cc/bbs/AllTogether/M.1777589218.A.656.html
                      貼文 ID 即為 "M.1777589218.A.656"
            - 貼文標題
                其中的 <a> 的 inner text
            若外容器裡沒有 <a> 代表該文已被刪除，略過不處理
    * 隔文列分隔線
        <div class="r-list-sep">
        + 出現在隔文列分隔線之後的 "貼文列" 代表公告，略過不處理
    * 貼文內容頁: 開啟點文網址後可進到貼文內容頁
        + 貼文 ID
            由貼文內容頁的網址來剖析
            - 例：https://www.ptt.cc/bbs/AllTogether/M.1777589218.A.656.html
                貼文 ID 即為 "M.1777589218.A.656"
        + 外容器
            <div id="main-content" class="bbs-screen bbs-content">
            - 作者
                <span class="article-meta-value"> 的 inner text
                    . 格式範例如 "snksohot (Sunny)" 其中 () 裡為暱稱，() 前為作者的 PTT ID
            - 看版
                <span class="article-meta-value"> 的 inner text
            - 標題
                <span class="article-meta-value"> 的 inner text
            - 時間
                <span class="article-meta-value"> 的 inner text
                    . 格式範例如 "Sat May  2 00:40:39 2026"
            - 本文
                在外容器裡、但不在任何 <div> 或 <span> 中的文字，即為本文
    * "上頁" 鈕
        <a class="btn wide" href="/bbs/AllTogether/index4656.html">‹ 上頁</a>
        其中的 href 屬性值可能依情況而變動，勿以它做為定位。
        . 若沒有 href 屬性，代表已在第一頁
    * "下頁" 鈕
        <a class="btn wide" href="/bbs/AllTogether/index4657.html">下頁 ›</a>
        其中的 href 屬性值可能依情況而變動，勿以它做為定位。
        . 若沒有 href 屬性，代表已在最後一頁
    * "最新" 鈕
        <a class="btn wide" href="/bbs/AllTogether/index.html">最新</a>
        其中的 href 屬性值可能依情況而變動，勿以它做為定位。
        . 若沒有 href 屬性，代表已在最新一頁
    * "最舊" 鈕
        <a class="btn wide" href="/bbs/AllTogether/index1.html">最舊</a>
        其中的 href 屬性值可能依情況而變動，勿以它做為定位。
        . 若沒有 href 屬性，代表已在最舊一頁

# 建立資料庫表格記錄
    * 貼文
        + ID
        + 作者
        + 看版
        + 標題
        + 貼文時間
        + 本文
    * 爬蟲任務執行記錄
        + 爬蟲任務 ID
        + 執行開始時間
        + 執行結束時間
        + 處理過貼文數量
            - 新增貼文數量
            - 略過貼文數量
                . 已爬取過貼文或公告貼文會被略過

# 爬蟲任務的執行
    * 當在 "爬蟲任務" 表單中選擇爬蟲任務並執行時，依據下述規則進行貼文的爬取
        + 依據所執行之爬蟲任務，連往指定看版的網址
        + 若任務中有指定關鍵字，則搜尋該關鍵字後，等待貼文清單出現。若無指定關鍵字，亦即爬取看版全部內容時，則直接處理看版全部貼文清單內容
        + 依據 "看版貼文網頁結構" 從最新的貼文頁開始由上而下處理每一個貼文列
            - 判斷貼文列裡貼文網址所剖析出來的貼文 ID 是否已經爬取過
                . 若已爬取過，則略過，並記數
            - 若未爬取過的貼文列，則開啟貼文網址，開始剖析貼文內容頁
                . 依據 "看版貼文網頁結構" 的 "貼文內容頁" 描述剖析貼文內容，並且將貼文相關資訊記錄在資料庫的 "貼文" 資料中
            - 剖析完貼文內容頁之後，以瀏覽器的 "Back" 機制，回到適才的貼文清單頁
            - 開始處理同一貼文清單頁的下一個貼文列，直到最後一筆
        + 處理完貼文清單頁中的所有貼文列之後，點擊 "上頁" 鈕，繼續處理前一頁的貼文清單頁，直到 "上頁" 鈕無法點擊 (沒有 href 屬性) 為止。

# 有任何我遺漏、你覺得需要加上的功能，再跟我建議。另外，有任何沒考慮周到的地方，也請你補充。

> 使用者介面上 wording 的修改
    * "Console Log" -> "系統日誌"
    * "CollectLineId" -> 收集 Line ID
