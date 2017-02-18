(**
# Memoize
 *)
let memoizeBy (f: 'a -> 'b) (g: 'a -> 'c) =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<_, _>(HashIdentity.Structural)
    fun x ->
        cache.GetOrAdd(Some (g x), lazy (f x)).Force()
        
let inline memoize f = memoizeBy f id
let inline memoizeConst f =
    let g = memoize (fun _ -> f())
    fun () -> g 0

