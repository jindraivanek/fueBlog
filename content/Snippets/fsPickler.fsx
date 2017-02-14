(** 
# FsPickler
FsPickler can be used to "serialize" functions: 
*)
open MBrace.FsPickler
open MBrace.FsPickler.Json

let s = FsPickler.CreateJsonSerializer()
let pickle = s.PickleToString (fun x -> x + 1)
let f:int->int = bs.UnPickleOfString pickle
f(1)