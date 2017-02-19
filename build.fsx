#load "paket-files/include-scripts/net46/include.main.group.fsx"
#r "packages/FAKE/tools/FakeLib.dll"

#load "config.fsx"

open FSharp.Literate
open System.IO
open Fue.Data
open Fue.Compiler
open Fake

type Category = {
    Name: string
    Active: bool
}

type Post = {
    Filename: string
    Html: string
}
let source = __SOURCE_DIRECTORY__
let outputPath = source @@ Config.outputPath

Target "Build" ( fun _ ->
    CreateDir outputPath

    let saveOutput path text = 
        CreateDir ((fileInfo(outputPath @@ path).Directory.FullName))
        printfn "Creating %s." path
        File.WriteAllText(outputPath @@ path, text)

    let categories = DirectoryInfo(source @@ "content").GetDirectories() |> Seq.map (fun d -> d.Name) |> Seq.toList

    let getContent path =
        let f = FileInfo(path)
        let create x =
            { Filename = f.Name.Remove(f.Name.Length - f.Extension.Length)
              Html = Literate.WriteHtml x }
        match f.Extension with 
        | ".fsx" -> Literate.ParseScriptFile(f.FullName) |> create |> Some
        | ".md" -> Literate.ParseMarkdownFile(f.FullName) |> create |> Some
        | _ -> printfn "Ignore %s %s" f.FullName f.Extension; None
    
    let getPosts cat = 
        DirectoryInfo(source @@ "content" @@ cat).GetFiles()
        |> Seq.sortByDescending(fun f -> f.CreationTime)
        |> Seq.choose (fun (f:FileInfo) -> getContent (f.FullName))
        |> Seq.toArray

    let rec fue extraF page layoutPath =
        init
        |> add "site.title" Config.siteTitle
        |> add "site.tagline" Config.siteTagLine
        |> add "site.description" Config.siteDescription
        |> add "githubUrl" Config.githubUrl
        |> add "twitterUrl" Config.twitterUrl
        |> add "page.title" page
        |> add "isHomePage" (page="index")
        |> add "include" (fue extraF page)
        |> add "categories" (categories |> Seq.map (fun c -> {Name = c; Active = c=page}))
        |> add "getPosts" getPosts
        |> add "formatTime" (fun (format:string) -> System.DateTime.Now.ToString format)
        |> add "eq" (fun (x,y) -> x=y)
        |> extraF
        |> fromFile (source @@ "layouts" @@ layoutPath)

    "index" :: categories
    |> Seq.map (fun x -> x, x + ".html")
    |> Seq.iter (fun (x, path) -> fue id x path |> saveOutput path)

    

    categories
    |> Seq.iter (fun c -> getPosts c |> Seq.iter (fun p -> fue (add "content" p.Html) p.Filename "post.html" |> saveOutput (c + "_" + p.Filename + ".html")))

    CopyRecursive (source @@ "include") outputPath true |> ignore
)

Target "Deploy" (fun _ ->
    CreateDir (source @@ "deploy")
    Zip outputPath (source @@ "deploy" @@ "output.zip") (!! (outputPath @@ "**/*") --(outputPath @@ ".git/**"))
)

Target "All" id

"Build" ==> "All"
"Deploy" ==> "All"

RunTargetOrDefault "All"