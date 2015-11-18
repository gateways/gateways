(*


    Gateway Hub Management


*)

open System
open System.IO


let relpath path = Path.Combine(__SOURCE_DIRECTORY__, path)

type FilePath = string

type INamed = 
  abstract Name : string

type gateway = 
    { name : string }
    interface INamed with
        member x.Name = x.name

type gatewayroot = 
    { name : string 
      assets : FilePath list 
      antiassets : FilePath list }
    interface INamed with
        member x.Name = x.name

let gateways = 
    Directory.GetDirectories(relpath "") 
    |> Seq.map Path.GetFileNameWithoutExtension 
    |> Seq.where (fun g -> not (String.IsNullOrWhiteSpace(g) || g.StartsWith(".") || g.StartsWith("_") || g = "common" ))  
    |> Seq.map (fun g -> { gateway.name = g } )
    |> Seq.toList


let common = 
    { name = "common"
      assets = 
        [ "_layouts/"
          "_layouts/default.html" 
          "_layouts/post.html"
          "_posts/" 
          "CNAME"
          "_config.yml"
          "index.md"
          "params.json" ] 
      antiassets = [ "index.html" ] }

let pathto (source:'a when 'a :> INamed) asset =
    relpath source.Name + "/" + asset

let commonpath name = relpath "common/" + name
let gatewaypath gateway name = relpath


let populate =
    for g in gateways do
        for ass in common.assets do
            File.Copy((pathto common ass), (pathto g ass))
        for antiass in common.antiassets do
            File.Delete(pathto g antiass)

        printf "%s" g.name

   
// copy files, from -> to












