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
              "_layouts/index.html"
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

        let Eutro =
          { EmptyGateway with
              Title = "Eutro"
              Sections = [ "Contact"; "Services"; "Partners"; "Projects" ]
              Design = "eutro" }

        let iforsk =
          { EmptyGateway with
              Title = "iforsk"
              Sections = [ "Tools"; "Events"; "Courses"; "Resources"; "News"; ]
              Design = "iforsk" }

        let forforskning =
            { EmptyGateway with
                Title = "forforskning"
                Sections = [ "Directory"; "Resources"; ]
                Design = "catalogue" }

        let forresearch =
            { EmptyGateway with
                Title = "forresearch"
                Sections = [ "Katalog"; "Ressurs"; ]
                Design = "catalogue" }

        let Helseregistre =
            { EmptyGateway with
                Title = "Helseregistre"
                Sections = [ "Registre"; "Ressurs"; "Nyheter"; "Eventer" ]
                Design = "directory" }

        let Helsereg =
            { EmptyGateway with
                Title = "Helsereg"
                Sections = [ "Registre"; "Ressurs"; "Nyheter"; "Eventer" ]
                Design = "catalogue" }

        let Healthreg =
            { EmptyGateway with
                Title = "Healthreg"
                Sections = [ "Directory"; ]
                Design = "catalogue" }

        let variabler =
            { EmptyGateway with
                Title = "variabler"
                Sections = [ "Katalog"; "Ressurs"; ]
                Design = "catalogue" }

        let variables =
            { EmptyGateway with
                Title = "variables"
                Sections = [ "Directories"; "Resources"; ]
                Design = "catalogue" }

        let Kodeverket =
            { EmptyGateway with
                Title = "Kodeverket"
                Sections = [ "Dugnad"; "Koder"; "Nyheter"; ]
                Design = "directory" }

        let kodebok =
            { EmptyGateway with
                Title = "kodebok"
                Sections = [ "Katalog"; "Ressurs"; "Dele"; ]
                Design = "catalogue" }

        let codebook =
            { EmptyGateway with
                Title = "codebook"
                Sections = [ "Directory"; "Resources"; "Share"; ]
                Design = "catalogue" }

        let kvrk =
            { EmptyGateway with
                Title = "kvrk"
                Sections = [ "Diagnosis"; "Procedure"; "Prescription" ]
                Design = "directory" }

        let Repository =
            { EmptyGateway with
                Title = "Repsitory"
                Sections = [ "Directory"; ]
                Design = "catalogue" }

        let Taxonomy =
            { EmptyGateway with
                Title = "Taxonomy"
                Sections = [ "Directory"; ]
                Design = "catalogue" }

        let Taksonomi =
            { EmptyGateway with
                Title = "Taksonomi"
                Sections = [ "Katalog"; ]
                Design = "catalogue" }

        let Ontologi =
            { EmptyGateway with
                Title = "Ontologi"
                Sections = [ "Katalog" ]
                Design = "catalogue" }

        let Ontology =
            { EmptyGateway with
                Title = "Ontology"
                Sections = [ "Directory"; ]
                Design = "catalogue" }


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
    let gatewaypath gateway name = relpath
    let stagenpush repo =
        Git.Staging.StageAll(repo)
        Git.Commit.Commit repo "Gateway content population"
        Git.Branches.pushBranch repo "origin" "gh-pages"
    let publish (g:site) =
        let repo = pathto g ""
        stagenpush repo
        printf "updated %s\r\n, repo in %s\r\n" g.root repo


    module private StructuralValidation =

        let ensureDirectory path = Directory.CreateDirectory(path) |> ignore
        let safeCopy from to' = if not (File.Exists(to')) then File.Copy(from, to')
        let sharedroot = relpath shared.root
        let designs = Gateways.sites |> List.groupBy (fun g -> g.Design)

        let ensureLayouts () =
            let root = sharedroot + "/_layouts/"
            for design, gateways in designs do
                let designFolder = root + design.ToLower() + "/"
                ensureDirectory (designFolder)
                safeCopy (root + "default.html") (designFolder + "default.html")
                safeCopy (root + "post.html") (designFolder + "post.html")

        let pass source target section =
            let correctedText = File.ReadAllText(source).Replace(@"{%% site.section %%}", section )
            File.WriteAllText(target, correctedText)

        let ensureSections () =
            for site in Gateways.sites do
                let root = relpath site.Title + "/"
                for section in site.Sections do

                    let sectionPage = root + section.ToLower() + ".md"
                    if not (File.Exists(sectionPage)) then
                        pass (sharedroot + @"/section/section.md") sectionPage section

//                    let sectionpath = root + section.ToLower() + "/"
//                    ensureDirectory (sectionpath)
//                    //ensureDirectory (sectionpath + "_posts/")
//
//                    let sectionIndex = sectionpath +  "index.html"
//                    if not (File.Exists(sectionIndex)) then
//                        pass (sharedroot + @"/section/index.html") sectionIndex section

                        //https://github.com/raphinou/jekyll-base/blob/gh-pages/_layouts/index.html
    let ensureDesigns () =
        StructuralValidation.ensureLayouts ()
        StructuralValidation.ensureSections ()


    module internal Asset =

        let ensure (g:site, ass:FilePath) =
            let cname g ass = File.WriteAllLines((pathto g ass), [ g.root + ".no" ])
            let mkdir g ass = Directory.CreateDirectory(pathto g ass) |> ignore
            let pass  g ass =
                let correctedText = File.ReadAllText(pathto shared ass).Replace(@"{%% gateway.name %%}", g.root)
                File.WriteAllText((pathto g ass), correctedText)

            let (|Cname|_|) (g, ass) = check (ass = "CNAME")
            let (|IsDir|_|) (g, ass) = check ((ass:FilePath).EndsWith("/"))

            match g, ass with
            | Cname -> cname g ass
            | IsDir -> mkdir g ass
            | _ ->     pass  g ass

        let delete (g:site, antiass:FilePath) = File.Delete(pathto g antiass)

    let populateSite (s:site) =


        let withsite assets = assets |> List.map (fun a -> s,a)
        let delete assets = assets |> withsite |> List.map Asset.delete |> ignore
        let ensure assets = assets |> withsite |> List.map Asset.ensure |> ignore

        shared.antiassets |> delete
        shared.assets     |> ensure


    let populate () =
        ensureDesigns ()
        gateways |> List.map populateSite

    let publishAll () = gateways |> List.map publish


ContentCurator.ensureDesigns ()
ContentCurator.populate ()
ContentCurator.publishAll ()
