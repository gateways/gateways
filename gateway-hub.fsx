(*


    Gateway Hub Management


*)


#I "../datamunger/packages/FAKE.4.9.3/tools/"
#r "../datamunger/packages/FAKE.4.9.3/tools/FakeLib.dll"


open Fake
open System
open System.IO

[<AutoOpen>]
module Utility =

    let relpath path = Path.Combine(__SOURCE_DIRECTORY__, path)


module Gateways =

    type FilePath = string

    type IHaveAPath = 
        abstract path : string
        abstract relpath : string

    type gateway = 
        { root : FilePath }
        interface IHaveAPath with
            member x.path = x.root
            member x.relpath = relpath x.root

    type gatewayroot = 
        { name : string 
          root : FilePath
          assets : FilePath list 
          antiassets : FilePath list }
        interface IHaveAPath with
            member x.path = x.root
            member x.relpath = relpath x.root

    let gateways = 
        Directory.GetDirectories(relpath "") 
        |> Seq.map Path.GetFileNameWithoutExtension 
        |> Seq.where (fun g -> not (String.IsNullOrWhiteSpace(g) || g.StartsWith(".") || g.StartsWith("_")))  
        |> Seq.map (fun g -> { gateway.root = g } )
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




module ContentCurator =

    open Gateways

    let pathto (source:'a when 'a :> IHaveAPath) asset = relpath source.path + "/" + asset
    let commonpath name = relpath "common/" + name
    let gatewaypath gateway name = relpath

    let stagenpush repo = 
        Git.Staging.StageAll(repo)
        Git.Commit.Commit repo "Gateway content population"
        Git.Branches.pushBranch repo "origin" "gh-pages"

    let publish (g:gateway) =
        let repo = pathto g ""
        stagenpush repo
        printf "updated %s\r\n, repo in %s\r\n" g.root repo
     
    module internal Asset =

        let cname (g:gateway) (ass:FilePath) = File.WriteAllLines((pathto g ass), [ g.root + ".no" ])
        let mkdir (g:gateway) (ass:FilePath) = Directory.CreateDirectory(pathto g ass) |> ignore
        let pass  (g:gateway) (ass:FilePath) =
            let correctedText = File.ReadAllText(pathto shared ass).Replace(@"{%% gateway.name %%}", g.root)
            File.WriteAllText((pathto g ass), correctedText)

        let (|Cname|_|) (g, ass) = if ass = "CNAME" then Some ass else None
        let (|IsDir|_|) (g, ass:FilePath) = if ass.EndsWith("/") then Some ass else None

        let ensure (g:gateway, ass:FilePath) = function
            | Cname a -> cname g ass
            | IsDir a -> mkdir g ass
            | _ ->       pass  g ass

        let delete (g:gateway, antiass:FilePath) = File.Delete(pathto g antiass)

    let populategateway (g:gateway) =

        let withgateway assets = assets |> List.map (fun a -> g,a)
        let ensure assets = assets |> withgateway |> List.map Asset.ensure |> ignore
        let delete assets = assets |> withgateway |> List.map Asset.delete |> ignore

        shared.assets     |> ensure
        shared.antiassets |> delete
        publish g


    let populate = gateways |> List.map populategateway


