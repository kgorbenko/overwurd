namespace Overwurd.Infrastructure.Tests

module Dapper =

    open System
    open Dapper

    type internal OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override this.SetValue(param, value) =
            let valueOrNull =
                match value with
                | Some x -> box x
                | None -> null

            param.Value <- valueOrNull

        override this.Parse value =
            if isNull value || value = box DBNull.Value
            then None
            else Some (value :?> 'T)

    let registerTypeHandlers () =
        SqlMapper.AddTypeHandler (OptionHandler<Guid>())
        SqlMapper.AddTypeHandler (OptionHandler<int64>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTimeOffset>())
        ()
