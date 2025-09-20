namespace FacilityManagement.Client

module Index =

    open Elmish
    open FacilityManagement.Shared
    open SAFE

    type State = {
        Todos: RemoteData<Todo list>
        Input: string
    }

    type Message =
        | SetInput of string
        | LoadTodos of ApiCall<unit, Todo list>
        | SaveTodo of ApiCall<string, Todo list>


    module State =
        let todosApi = Api.makeProxy<ITodosApi> ()

        let init () =
            let initialModel = { Todos = NotStarted; Input = "" }
            let initialCmd = LoadTodos(Start()) |> Cmd.ofMsg

            initialModel, initialCmd

        let update message model =

            match message with
            | SetInput value -> 
                { model with Input = value }, Cmd.none

            | LoadTodos msg ->
                match msg with
                | Start() ->
                    let loadTodosCmd = Cmd.OfAsync.perform todosApi.get () (Finished >> LoadTodos)
                    { model with Todos = model.Todos.StartLoading() }, loadTodosCmd

                | Finished todos -> 
                    { model with Todos = Loaded todos }, Cmd.none

            | SaveTodo msg ->
                match msg with
                | Start todoText ->
                    let saveTodoCmd =
                        let todo = Todo.create todoText
                        Cmd.OfAsync.perform todosApi.post todo (Finished >> SaveTodo)

                    { model with Input = "" }, saveTodoCmd

                | Finished todos ->
                    { model with Todos = RemoteData.Loaded todos }, Cmd.none

    module View =
        open Feliz
        open Feliz.Shadcn

        let renderAccordion =
            Shadcn.accordion [
                accordion.type'.single
                prop.children [
                    Shadcn.accordionItem [
                        prop.value "item-1"
                        prop.children [
                            Shadcn.accordionTrigger "Is it accessible?"
                            Shadcn.accordionContent "Yes. It adheres to the WAI-ARIA design pattern."
                        ]
                    ]
                    Shadcn.accordionItem [
                        prop.value "item-2"
                        prop.children [
                            Shadcn.accordionTrigger "Is it styled?"
                            Shadcn.accordionContent
                                "Yes. It comes with default styles that matches the other components & aesthetic.."
                        ]
                    ]
                ]
            ]
        let renderTodoAction state dispatch =
            Html.div [
                prop.className "flex flex-col sm:flex-row mt-4 gap-4"
                prop.children [
                    Html.input [
                        prop.className
                            "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker text-sm sm:text-base"
                        prop.value state.Input
                        prop.placeholder "What needs to be done?"
                        prop.autoFocus true
                        prop.onChange (SetInput >> dispatch)
                        prop.onKeyPress (fun ev ->
                            if ev.key = "Enter" then
                                dispatch (SaveTodo(Start state.Input)))
                    ]
                    Html.button [
                        prop.className
                            "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed text-sm sm:text-base"
                        prop.disabled (Todo.isValid state.Input |> not)
                        prop.onClick (fun _ -> dispatch (SaveTodo(Start state.Input)))
                        prop.text "Add"
                    ]
                    Shadcn.button [
                        button.variant.default'
                        prop.disabled (Todo.isValid state.Input |> not)
                        prop.onClick (fun _ -> dispatch (SaveTodo(Start state.Input)))
                        prop.text "Add Alt"
                    ]
                ]
            ]

        let renderTodoList state dispatch =
            Html.div [
                prop.className "rounded-md p-2 sm:p-4 w-full"
                prop.children [
                    Html.ol [
                        prop.className "list-decimal ml-4 sm:ml-6"
                        prop.children [
                            match state.Todos with
                            | NotStarted -> Html.text "Not Started."

                            | Loading None -> Html.text "Loading..."

                            | Loading(Some todos) ->
                                for todo in todos do
                                    Html.li [
                                        prop.className "my-1 text-black text-base sm:text-lg break-words"
                                        prop.text todo.Description
                                    ]

                            | Loaded todos ->
                                for todo in todos do
                                    Html.li [
                                        prop.className "my-1 text-black text-base sm:text-lg break-words"
                                        prop.text todo.Description
                                    ]
                        ]
                    ]

                    renderTodoAction state dispatch
                ]
            ]

        let render state dispatch =
            Html.section [
                prop.className "h-screen w-screen relative overflow-hidden"
                prop.children [
                    // Meta viewport tag for proper mobile scaling
                    Html.meta [
                        prop.name "viewport"
                        prop.content "width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"
                    ]

                    // Background div with image and glass effect
                    Html.div [
                        prop.className
                            "absolute inset-0 bg-cover bg-center bg-fixed bg-no-repeat bg-white/20 backdrop-blur-sm"
                        prop.style [ style.backgroundImageUrl "https://unsplash.it/1200/900?random" ]
                    ]

                    // Content container (the rest of your UI)
                    Html.div [
                        prop.className "relative z-10 h-full w-full"
                        prop.children [
                            // Your existing content here
                            Html.a [
                                prop.href "https://safe-stack.github.io/"
                                prop.className
                                    "absolute block ml-4 sm:ml-12 h-10 w-10 sm:h-12 sm:w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                                prop.children [ Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ] ]
                            ]


                            Html.div [
                                prop.className "flex flex-col items-center justify-center h-full"
                                prop.children [
                                    Html.div [
                                        prop.className
                                            "bg-white/20 backdrop-blur-lg p-4 sm:p-8 rounded-xl shadow-lg border border-white/30 mx-4 sm:mx-0 max-w-full sm:max-w-2xl"
                                        prop.children [
                                            Html.h1 [
                                                prop.className "text-center text-3xl sm:text-5xl font-bold mb-3 p-2 sm:p-4"
                                                prop.text "Entheo Genesis"
                                            ]
                                            renderTodoList state dispatch
                                            renderAccordion
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]