namespace Shared

open System

type Image = ImageId of string
type Patch = PatchId of string
type ImagePatch = Image * Patch
type Neighbor = Image * float
type TextAttribute = Text of string
type AttributeCorrelation = {Attribute : TextAttribute; Correlation : float}
type AttributeStatus = 
    |Offered 
    |Rejected of DateTime * string
    |Accepted of DateTime * string
type AttributeCandidate = {Id : int; Representatives : ImagePatch list; Status : AttributeStatus}
type InitialDisplay = {Candidates : Map<int,AttributeCandidate>}
type RejectionOfAttribute = {Subject: AttributeCandidate; Reason : string}
type AttributeExpansion = {Candidate : AttributeCandidate; Neighbors : Neighbor list}
type AcceptedAttribute = {Candidate : AttributeCandidate; AcceptedMatches : Neighbor list; NewName : string}
type RelationalResults = {ObjectsWithAttributes : (Image * TextAttribute list) list}


module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type ICounterApi =
    {       
        load : unit -> Async<InitialDisplay>;
        expandCandidate : AttributeCandidate -> Async<AttributeExpansion>;
        acceptNewAttribute : AcceptedAttribute -> Async<AttributeCandidate>;
        rejectOfferedAttribute : RejectionOfAttribute -> Async<AttributeCandidate>;
    }
