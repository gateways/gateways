(*


    Gateway Hub Management


*)

open System
open System.IO


let relpath path = Path.Combine(__SOURCE_DIRECTORY__, path)

type File = string

type gateway = { name : string }

type gatewayroot = 
    { name : string 
      assets : File list 
      antiassets : File list }

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






   
//Seq.map (fun g -> { name = g; assets = [] } )
// copy files, from -> to












