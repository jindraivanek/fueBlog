#load "paket-files/include-scripts/net46/include.main.group.fsx"
#r "packages/FAKE/tools/FakeLib.dll"

#load "config.fsx"

open FSharp.Literate
open System.IO
open Fue.Data
open Fue.Compiler
open Fake

type Category =
    | Active of string
    | Inactive of string

Target "All" ( fun _ ->
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

    let rec fue page path =
        init
        |> add "site.title" Config.siteTitle
        |> add "site.tagline" Config.siteTagLine
        |> add "site.description" Config.siteDescription
        |> add "githubUrl" Config.githubUrl
        |> add "twitterUrl" Config.twitterUrl
        |> add "page.title" page
        |> add "isHomePage" (page="index")
        |> add "include" (fue page)
        |> add "categories" (categories |> Seq.map (fun c -> if c=page then Active c else Inactive c))
        |> add "getPosts" getPosts
        |> add "formatTime" (fun (format:string) -> System.DateTime.Now.ToString format)
        |> add "eq" (fun (x,y) -> x=y)
        |> fromFile (source @@ "layouts" @@ path)

    "index" :: categories
    |> Seq.map (fun x -> x, x + ".html")
    |> Seq.iter (fun (x, path) -> fue x path |> saveOutput path)

    CopyRecursive (source @@ "include") (source @@ "output") true |> ignore
)

RunTargetOrDefault "All"