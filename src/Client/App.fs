module App

open Elmish
open Elmish.React

open Fable.Core.JsInterop
open Index

importSideEffects "./index.css"

#if DEBUG
open Elmish.HMR
#endif

Program.mkProgram State.init State.update View.render
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"

|> Program.run