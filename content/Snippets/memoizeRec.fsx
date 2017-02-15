(**
# Memoize recursive
*)

let memoize (f: 'a -> 'b) =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<_, _>(HashIdentity.Structural)
    fun x ->
        cache.GetOrAdd(x, lazy (f x)).Force()

// This works, but emit warning: This and other recursive references to the object(s) being defined will be checked for initialization-soundness at runtime through the use of a delayed reference.
let rec fib' = memoize <| fun n -> if n<1 then 1 else fib' (n-1) + fib' (n-2)

(**
Memoize gif by [F# Casts](https://twitter.com/FSharpCasts)
 <video controls>
    <source src="https://video.twimg.com/tweet_video/C4pJCWzUkAE-kQV.mp4" type="video/mp4">
    Your browser does not support the video tag.
</video> 
*)

//-----------------------------------
let memoizeRec f =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<_, _>(HashIdentity.Structural)
    let rec memF x = 
        cache.GetOrAdd(x, lazy (f memF x)).Force()
    memF
    
let fib = memoizeRec <| fun f n -> if n<1 then 1 else f (n-1) + f (n-2)