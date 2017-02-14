#load "paket-files/include-scripts/net46/include.main.group.fsx"
#r "packages/FAKE/tools/FakeLib.dll"

#load "config.fsx"

open FSharp.Literate
open System.IO
open Fue.Data
open Fue.Compiler
open Fake

let run() =
    let source = __SOURCE_DIRECTORY__

    let saveOutput path text = File.WriteAllText(source @@ "output" @@ path, text)

    let categories = DirectoryInfo(source @@ "content").GetDirectories() |> Seq.map (fun d -> d.Name) |> Seq.toList

    let getPosts cat = 
        DirectoryInfo(source @@ "content" @@ cat).GetFiles()
        |> Seq.sortByDescending(fun f -> f.CreationTime)
        |> Seq.choose (fun (f:FileInfo) -> 
            match f.Extension with 
            | ".fsx" -> Literate.ParseScriptFile(f.FullName) |> Some
            | ".md" -> Literate.ParseMarkdownFile(f.FullName) |> Some
            | _ -> printfn "Ignore %s %s" f.FullName f.Extension; None)
        |> Seq.map Literate.WriteHtml
        |> Seq.toArray

    let rec fue path =
        init
        |> add "site.title" Config.siteTitle
        |> add "site.tagline" Config.siteTagLine
        |> add "site.description" Config.siteDescription
        |> add "page.title" "Home"
        |> add "isHomePage" true
        |> add "include" fue
        |> add "categories" categories
        |> addMany (categories |> List.map (fun c -> ("posts-" + c), (getPosts c :> obj)))
        |> add "githubUrl" Config.githubUrl
        |> add "twitterUrl" Config.twitterUrl
        |> add "formatTime" (fun (format:string) -> System.DateTime.Now.ToString format)
        |> fromFile (source @@ "layouts" @@ path)

    "index" :: categories
    |> Seq.map (fun x -> x + ".html")
    |> Seq.iter (fun x -> fue x |> saveOutput x)

    CopyRecursive (source @@ "include") (source @@ "output") true

run()