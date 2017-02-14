(**
##Introduction
[F#](http://fsharp.org/) is a mature, open source, cross-platform, functional-first programming language. It empowers users and organizations to tackle complex computing problems with simple, maintainable and robust code.
*)

let safeTake n s = 
    let x = ref n
    Seq.takeWhile (fun _ -> x := !x - 1; !x >= 0) s