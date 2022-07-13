module Overwurd.Domain.Common.Utils

open System
open System.Text

let tryParseInt (value: string): int Option =
    match Int32.TryParse value with
    | true, value -> Some value
    | false, _ -> None

let parseInt (value: string): int =
    match tryParseInt value with
    | Some parsed -> parsed
    | None -> raise (InvalidOperationException $"Cannot parse '{value}' to integer")

let toUpperCase (value: string): string =
    value.ToUpperInvariant()

let toBytes (key: string) =
    Encoding.UTF8.GetBytes key