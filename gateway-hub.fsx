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

    module internal Sites =

        let Eutro = { EmptyGateway with Title = "Eutro" }

        let iforsk = { EmptyGateway with Title = "iforsk" }
        let forforskning = { EmptyGateway with Title = "forforskning" }
        let forresearch = { EmptyGateway with Title = "forresearch" }

        let Helseregistre = { EmptyGateway with Title = "Helseregistre" }
        let Helsereg = { EmptyGateway with Title = "Helsereg" }
        let Healthreg = { EmptyGateway with Title = "Healthreg" }
        let variabler = { EmptyGateway with Title = "variabler" }
        let variables = { EmptyGateway with Title = "variables" }

        let Kodeverket = { EmptyGateway with Title = "Kodeverket" }
        let kodebok = { EmptyGateway with Title = "kodebok" }
        let codebook = { EmptyGateway with Title = "codebook" }
        let kvrk =
            { EmptyGateway with
                Title = "kvrk"
                Sections = [ "Diagnosis"; "Procedure"; "Prescription" ]  //todo: "news/major" "news/minor" "news/whaaaa"
                Design = "oppslag" }

        let Repository = { EmptyGateway with Title = "Repsitory" }
        let Taxonomy = { EmptyGateway with Title = "Taxonomy" }
        let Taksonomi = { EmptyGateway with Title = "Taksonomi" }
        let Ontologi = { EmptyGateway with Title = "Ontologi" }
        let Ontology = { EmptyGateway with Title = "Ontology" }

        let all =
            [ Eutro
              iforsk
              forforskning
              forresearch
              Helseregistre
              Helsereg
              Healthreg
              variabler
              variables
              Kodeverket
              kvrk
              kodebok
              codebook
              Repository
              Taxonomy
              Taksonomi
              Ontologi
              Ontology
            ]

    let sites = Sites.all



module ContentCurator =

    open Gateways

    let pathto (source:'a when 'a :> IHaveAPath) asset = relpath source.path + "/" + asset
    //let commonpath name = relpath "common/" + name
    let gatewaypath gateway name = relpath
    let stagenpush repo =
        Git.Staging.StageAll(repo)
        Git.Commit.Commit repo "Gateway content population"
        Git.Branches.pushBranch repo "origin" "gh-pages"
    let publish (g:site) =
        let repo = pathto g ""
        stagenpush repo
        printf "updated %s\r\n, repo in %s\r\n" g.root repo



    // todo: create a filler function that examines all the sites, looks at their design folders, and copied default.html and post.html (with the section name), and puts them in the root folder
    //        These get propogated to child sites if they are missing
    //        No intelligence, perfect overrides from child sites
    let empty = Set.ofList [ "About" ]
    let joinsections acc (d, gateways) = 
        let allsections = [ for g in (gateways:Gateway list) do yield! g.Sections ]
        Set.union acc (Set.ofList allsections)
    let designs = 
        Gateways.sites
        |> List.groupBy (fun g -> g.Design)
    let aggregateSections =
        designs
        |> List.fold joinsections empty
        |> Set.toList


    let ensureDirectory path = Directory.CreateDirectory(path)

    let prepDesigns =
        //let designs = aggregateDesigns

        let root = relpath shared.root + "/_layouts/"
        for design, gateways in designs do
            let designFolder = root + design.ToLower() + "/"
            ensureDirectory (designFolder) |> ignore
            File.Copy(root + "default.html", designFolder + "default.html")
            File.Copy(root + "post.html", designFolder + "post.html")



        // for each design create folder in _layout
            // for each section copy default and post
       

       // for each site....

        // create root items if they dont exist - layout files...

        // blast through the sites, check and populate 
        ()

        //|> Seq.map (fun d -> printf ". . .%s\r\n" d)
        // select a list of designs and a superset of their sections, ensure their content and then populate 
        // based on design (make folder and templates)
 
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
            | _ ->     pass  g ass

        let delete (g:site, antiass:FilePath) = File.Delete(pathto g antiass)

    let populategateway (g:site) =

        let withgateway assets = assets |> List.map (fun a -> g,a)
        let ensure assets = assets |> withgateway |> List.map Asset.ensure |> ignore
        let delete assets = assets |> withgateway |> List.map Asset.delete |> ignore

        shared.assets     |> ensure
        shared.antiassets |> delete
        publish g

    let populate = 
        // ensure designs
        gateways |> List.map populategateway


ContentCurator.prepDesigns
