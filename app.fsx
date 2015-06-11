// Step 0. Boilerplate to get the paket.exe tool
 
open System
open System.IO
 
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let dst = ".paket\paket.exe"
if not (File.Exists dst) then
    let url = "https://github.com/fsprojects/Paket/releases/download/0.31.5/paket.exe"
    use wc = new Net.WebClient()
    let tmp = Path.GetTempFileName()
    wc.DownloadFile(url, tmp)
    Directory.CreateDirectory(".paket") |> ignore
    File.Move(tmp, dst)
 
// Step 1. Resolve and install the packages
 
#r ".paket\paket.exe"
 
Paket.Dependencies.Install """
source https://nuget.org/api/v2
nuget FSharp.Data
"""

// Step 2. Use the packages
 
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
 
//let ctxt = FSharp.Data.WorldBankData.GetDataContext()
//let data = ctxt.Countries.Algeria.Indicators.``GDP (current US$)``
open FSharp.Data

let baseUrl = "https://api.stackexchange.com/2.2/"

[<Literal>]
let tagsSampleJasonSchema = 
    """
    {
      "items": [
        {
          "has_synonyms": true,
          "is_moderator_only": false,
          "is_required": false,
          "count": 870388,
          "name": "java"
        }
        ],
      "has_more": true,
      "quota_max": 10000,
      "quota_remaining": 9992
    }
    """
type StackExTags = JsonProvider<tagsSampleJasonSchema>
let api_method ="tags"
let pagesize = 50;
let result = Http.RequestString( 
                    (baseUrl + api_method),
                    httpMethod="GET",
                    query=["pagesize", pagesize.ToString(); "order", "desc"; "sort", "popular"; "site", "stackoverflow"],
                    headers = ["Accept", "application/json"])

let tagsInfo = StackExTags.Parse(result).Items
tagsInfo
|> Array.iteri(fun i t -> printfn "[%03d] %s %09d" i t.Name t.Count)

//let dat = tagsInfo |> Array.map(fun t-> t.Name, t.Count) |> Array.toSeq
//Chart.Column dat
let data_items = 
    tagsInfo |> Array.map(fun t-> sprintf "['%s',%d]" t.Name t.Count)
    
let html_template = File.ReadAllText("barchart.html")
let html = html_template
                .Replace("@@CHART-TITLE", "StackOverflow Tag Popularity")
                .Replace("@@CHART-SUBTITLE", "")
                .Replace("@@DATA-HEADER","['Tag','Popularity'],")
                .Replace("@@DATA-ITEMS",String.Join(",", data_items))
                .Replace("@@CHART-SERIES-BINDING","0:{axis: 'popularity'}")
                .Replace("@@CHART-X-AXIS-LABELS","popularity:{label: 'question count'}")
                .Replace("@@CHART-HEIGHT",(pagesize * 20).ToString())
let tmp = Path.ChangeExtension(Path.GetTempFileName(),".html")
File.WriteAllText(tmp,html)
System.Diagnostics.Process.Start(tmp)
//  return! ctx |> OK(html.Replace("#1", news).Replace("#2", weather)) }
