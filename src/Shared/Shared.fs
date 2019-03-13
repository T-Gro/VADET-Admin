namespace Shared

open System

type Image = ImageId of string
type Patch = PatchId of string

type ImagePatch = Image * Patch
type Neighbor = {Hit:Image;Distance:float32;Accepted:bool; Patches: Patch list; Categories: string list}
type TextAttribute = Text of string
type AttributeCorrelation = {Attribute : TextAttribute; Correlation : float}
type AttributeStatus = 
    |Offered 
    |Rejected of DateTime * string
    |Accepted of DateTime * string
type AttributeCandidate = {Id : int; Representatives : ImagePatch list; Status : AttributeStatus}
type InitialDisplay = {Candidates : AttributeCandidate list}
type RejectionOfAttribute = {Subject: AttributeCandidate; Reason : string}
type AttributeExpansion = {Candidate : AttributeCandidate; Neighbors : Neighbor list; IgnoredCategories : string list}
type AcceptedAttribute = {Candidate : AttributeCandidate; AcceptedMatches : Neighbor list; NewName : string; IgnoredCategories : string list}
type RelationalResults = {ObjectsWithAttributes : (Image * TextAttribute list) list}


module Common =
    let extractImgId (ImageId x) = x
    let extractPatchId (PatchId x) = x

module Route =
    /// Defines how routes are generated on server and mapped from client
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

    let modifier =
#if DEBUG
        ""
#else
        "/vadet-admin"
#endif
    let clientBuilder a b = modifier + builder a b

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type ICounterApi =
    {       
        load : unit -> Async<InitialDisplay>;
        expandCandidate : AttributeCandidate -> Async<AttributeExpansion>;
        acceptNewAttribute : AcceptedAttribute -> Async<AttributeCandidate>;
        rejectOfferedAttribute : RejectionOfAttribute -> Async<AttributeCandidate>;
    }
