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

    let check = function | true -> Some () | _ -> None
    let relpath path = Path.Combine(__SOURCE_DIRECTORY__, path)

[<AutoOpen>]
module GatewayBase =

    type FilePath = string

    type IHaveAPath = 
        abstract path : string
        abstract relpath : string

    type root = 
        { name : string 
          root : FilePath
          assets : FilePath list 
          antiassets : FilePath list }
        interface IHaveAPath with
            member x.path = x.root
            member x.relpath = relpath x.root

    type site = 
        { root : FilePath }
        interface IHaveAPath with
            member x.path = x.root
            member x.relpath = relpath x.root

    let gateways = 
        Directory.GetDirectories(relpath "") 
        |> Seq.map Path.GetFileNameWithoutExtension 
        |> Seq.where (fun g -> not (String.IsNullOrWhiteSpace(g) || g.StartsWith(".") || g.StartsWith("_")))  
        |> Seq.map (fun g -> { site.root = g } )
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



module Gateways =

    type SocialMedia = 
        | Twitter of string 
        | Facebook of string 
        | GooglePlus of string

    type Source = Source of string

    type Gateway = 
        { Title : string 
          Sections : string list
          Design : string
          Social : SocialMedia list 
          Root : Source 
          SourceRoot : Source
          ContentSources : Source list }

    let EmptyGateway = 
        { Title = "[Empty]"
          Sections = [ ]
          Design = "none"
          Social = [ ]
          Root = Source "www.example.com/new"
          SourceRoot = Source "www.example.com/source"
          ContentSources = [ ] }


    module private Register = 

        let Eutro = { EmptyGateway with Title = "Eutro" }

        let Iforsk = { EmptyGateway with Title = "Eutro" }
        let Forforskning = { EmptyGateway with Title = "Eutro" }
        let Forresearch = { EmptyGateway with Title = "Eutro" }

        let Helsereg = { EmptyGateway with Title = "Eutro" }
        let Healthreg = { EmptyGateway with Title = "Eutro" }

        let Healthreg = { EmptyGateway with Title = "Eutro" }

        let all = [ Eutro ]


    let register = [ Register.all ]



module ContentCurator =

    let pathto (source:'a when 'a :> IHaveAPath) asset = relpath source.path + "/" + asset
    let commonpath name = relpath "common/" + name
    let gatewaypath gateway name = relpath
    let stagenpush repo = 
        Git.Staging.StageAll(repo)
        Git.Commit.Commit repo "Gateway content population"
        Git.Branches.pushBranch repo "origin" "gh-pages"
    let publish (g:site) =
        let repo = pathto g ""
        stagenpush repo
        printf "updated %s\r\n, repo in %s\r\n" g.root repo
     
    module internal Asset =

        let ensure (g:site, ass:FilePath) = 

            let cname g ass = File.WriteAllLines((pathto g ass), [ g.root + ".no" ])
            let mkdir g ass = Directory.CreateDirectory(pathto g ass) |> ignore
            let pass  g ass =
                let correctedText = File.ReadAllText(pathto shared ass).Replace(@"{%% gateway.name %%}", g.root)
                File.WriteAllText((pathto g ass), correctedText)

            let (|Cname|_|) (g, ass) = check (ass = "CNAME")
            let (|IsDir|_|) (g, ass) = check ((ass:FilePath).EndsWith("/"))

            function
            | Cname -> cname g ass
            | IsDir -> mkdir g ass
            | _ ->       pass  g ass

        let delete (g:site, antiass:FilePath) = File.Delete(pathto g antiass)

    let populategateway (g:site) =

        let withgateway assets = assets |> List.map (fun a -> g,a)
        let ensure assets = assets |> withgateway |> List.map Asset.ensure |> ignore
        let delete assets = assets |> withgateway |> List.map Asset.delete |> ignore

        shared.assets     |> ensure
        shared.antiassets |> delete
        publish g

    let populate = gateways |> List.map populategateway
