# WorkdayTool
單純做一些紀錄，這是在工作時，幫公司寫的WorkdayTool，包括全自動的Migration方式和全自動的把Workday上的資料落地到自己DB的方式，所有程式全部都我自己寫的，原本的git log內含有一些公司的敏感資料，所以就不copy整個git log history了，或許能給一些在大公司做超大量資料Migration或Integration的人參考。

Workday是一間大公司，我們公司買了他們的服務，因此需要把原先在Peoplesoft的一堆資料弄到Workday上面

他們的服務可以參考以下Document

[Workday Services](https://community.workday.com/sites/default/files/file-hosting/productionapi/versions/v31.2/index.html)

如果你全部大略看過，可以知道大約有幾千個API，但主管也不想我寫幾千個程式來串接(這樣工作會全部在我身上)。
所以這次Migration用另一套方式來做，我只要維護這個Tool就好，domain那些甚至不用瞭解。

主要的架構大概會長的像這樣
![架構圖](10.jpg "架構圖")
我們在TempDB內會產生API專用的Table，PeoplesoftDB的資料會先弄到TempDB內

使用流程大概就是先產生TempTable→打Stored procedure塞值到TempTable→使用完全Reflection將TempTable的資料塞進C# Object→最後打進Workday並產生Report

![Alt text](1.jpg "Optional title")
![Alt text](2.jpg "Optional title")

Workday與我們其他系統的Integration是先用Workday去做Custom Report，在全自動抓下來。

![Alt text](3.jpg "Optional title")
![Alt text](4.jpg "Optional title")
![Alt text](5.jpg "Optional title")
![Alt text](6.jpg "Optional title")
![Alt text](7.jpg "Optional title")
![Alt text](8.jpg "Optional title")