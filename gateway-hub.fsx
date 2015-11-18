(*


    Gateway Hub Management


*)


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
    |> Seq.where (fun g -> not (String.IsNullOrWhiteSpace(g) || g.StartsWith(".") || g.StartsWith("_") ))  
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
          "blog/"
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
            File.Copy((pathto shared ass), (pathto g ass))
        for antiass in shared.antiassets do
            File.Delete(pathto g antiass)

        printf "%s" g.name

   
// copy files, from -> to












