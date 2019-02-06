namespace Shared

type Image = Name of int
type Patch = Name of int
type ImagePatch = Image * Patch
type Neighbor = Image * float
type TextAttribute = Text of string
type AttributeCorrelation = {Attribute : TextAttribute; Correlation : float}
type AttributeCandidate = {Id : int; Representatives : ImagePatch list}
type AttributeExpansion = {Candidate : AttributeCandidate; Neighbors : Neighbor list}

type Rename = {NewName : string; Id : int}

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type ICounterApi =
    { rename : int -> Async<Rename>}
