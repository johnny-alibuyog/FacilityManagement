module FacilityManagement.Client.Test

open Fable.Mocha
open FacilityManagement.Shared
open FacilityManagement.Client.Index
open SAFE

let client =
    testList "Client" [
        testCase "Added todo" (fun _ ->
            let newTodo = Todo.create "new todo"
            let state, _ = State.init ()
            let state, _ = State.update (SaveTodo(Finished [ newTodo ])) state

            Expect.equal
                (state.Todos |> RemoteData.map _.Length |> RemoteData.defaultValue 0)
                1
                "There should be 1 todo"

            Expect.equal
                (state.Todos
                |> RemoteData.map List.head
                |> RemoteData.defaultValue (Todo.create ""))
                newTodo
                "Todo should equal new todo"
        )
    ]

let all =
    testList "All" [
        #if FABLE_COMPILER // This preprocessor directive makes editor happy
        Tests.shared
        #endif
        client
    ]

[<EntryPoint>]
let main _ = Mocha.runTests all