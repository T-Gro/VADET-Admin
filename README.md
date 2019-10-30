## VADET-Admin

This project is the user interace for administrators of Virtual Attributes, a research prototype to mine relational attributes out of visual data and its respective deep learning descriptors.

It is a client-server we application which requires pre-processed fiels as input (see GH prroject [preprocessing scripts](https://github.com/T-Gro/Visual-Attribute-Filtering-Scripts), and uses a relational database as its output. The database itself is not part of this project, but this project contains scripts which will create the schema from scratch via standard EntityFramework migrations.


## Install pre-requisites for developers

This page is written in F# and uses the SAFE template as its primary development stack.
You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* [FAKE 5](https://fake.build/) installed as a [global tool](https://fake.build/fake-gettingstarted.html#Install-FAKE)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you an also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Database connection
The server side attempts to connect to a MS SQL SERVER database. The provider configurable, the code does not use any SQL SERVER specific features.

The connection string is read usign an environment variable that must be prefilled at the production deployment server.
The data model is created using the entity definitions in the folder [Models](https://github.com/T-Gro/VADET-Admin/tree/master/src/KnnResults.Domain/Models)
```
var conn = Environment.GetEnvironmentVariable("VADETSQL") ;
```

## Work with the application

To concurrently run the server and the client components in watch mode use the following command:

```
fake build -t Run
```


## SAFE Stack Documentation

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/docs/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
* [Fable.Remoting](https://zaid-ajaj.github.io/Fable.Remoting/)
* [Fulma](https://fulma.github.io/Fulma/)

## Client-server API

The API between client (browser) and server (.NET running F# application) is documented in the project Shared.fs.
It contains the data types and operations which are called from client to server and carry also the respective return types.
Fsharp remoting handles the serialisation of the types to JSON strings on the wire over HTTPS.

[API model](https://github.com/T-Gro/VADET-Admin/blob/master/src/Shared/Shared.fs)

## Troubleshooting

* **fake not found** - If you fail to execute `fake` from command line after installing it as a global tool, you might need to add it to your `PATH` manually: (e.g. `export PATH="$HOME/.dotnet/tools:$PATH"` on unix) - [related GitHub issue](https://github.com/dotnet/cli/issues/9321)
