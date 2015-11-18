(*


    Gateway Hub Management


*)


#I "../datamunger/packages/FAKE.4.9.3/tools/"
#r "../datamunger/packages/FAKE.4.9.3/tools/FakeLib.dll"


open Fake
open System
open System.IO


let relpath path = Path.Combine(__SOURCE_DIRECTORY__, path)

type FilePath = string

type IHaveAPath = abstract path : string

type gateway = 
    { name : string }
    interface IHaveAPath with
        member x.path = x.name

type gatewayroot = 
    { name : string 
      root : FilePath
      assets : FilePath list 
      antiassets : FilePath list }
    interface IHaveAPath with
        member x.path = x.root

let gateways = 
    Directory.GetDirectories(relpath "") 
    |> Seq.map Path.GetFileNameWithoutExtension 
    |> Seq.where (fun g -> not (String.IsNullOrWhiteSpace(g) || g.StartsWith(".") || g.StartsWith("_")))  
    |> Seq.map (fun g -> { gateway.name = g } )
    |> Seq.toList


let shared = 
    { name = "shared"
      root = "_shared"
      assets = 
        [ "_layouts/"
          "_layouts/default.html" 
          "_layouts/post.html"
          "_posts/" 
          "_posts/2015-11-15-initial.md" 
          "blog/"
          "blog/index.html"
          "CNAME"
          "_config.yml"
          "index.md"
          "params.json" ] 
      antiassets = [ "index.html" ] }

let pathto (source:'a when 'a :> IHaveAPath) asset =
    relpath source.path + "/" + asset

let commonpath name = relpath "common/" + name
let gatewaypath gateway name = relpath

let populate =
    for g in gateways do

        for ass in shared.assets do
            if ass = "CNAME" then
                File.WriteAllLines((pathto g ass), [ g.name + ".no" ])
            else
                if ass.EndsWith("/") then
                    Directory.CreateDirectory(pathto g ass) |> ignore
                else
                    let correctedText = File.ReadAllText(pathto shared ass).Replace(@"{%% gateway.name %%}", g.name)
                    File.WriteAllText((pathto g ass), correctedText)

        for antiass in shared.antiassets do
            File.Delete(pathto g antiass)
        
        let repo = pathto g ""

        Git.Staging.StageAll(repo)
        Git.Commit.Commit repo "Gateway content population"
        Git.Branches.pushBranch repo "origin" "gh-pages"

        printf "updated %s\r\n, repo in %s\r\n" g.name repo


