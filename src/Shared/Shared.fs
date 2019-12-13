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
    |AutoOffered
    |OfferedButBlacklisted
    |OfferedButNotWhitelisted
type AttributeCandidate = {Id : int; Representatives : ImagePatch list; Status : AttributeStatus}
type InitialDisplay = {Candidates : AttributeCandidate list}
type RejectionOfAttribute = {Subject: AttributeCandidate; Reason : string; Username : string}
type AttributeExpansion = {Candidate : AttributeCandidate; Neighbors : Neighbor list; IgnoredCategories : string list}
type AcceptedAttribute = {Candidate : AttributeCandidate; AcceptedMatches : Neighbor list; NewName : string; IgnoredCategories : string list; Quality : string; Username : string}
type RelationalResults = {ObjectsWithAttributes : (Image * TextAttribute list) list}
type AutoOfferedAttribute = {OldId : int; Name : string; NewImage : Image; OriginalTreshold : float; DistanceToAttribute : float; OriginalWhitelist : string; OriginalBlacklist : string; Status : AttributeStatus}
type DynamicDbProposals = {ProductAttributePairs : AutoOfferedAttribute list}
type OfferReactionStatus = AcceptingOffer | RejectingOffer
type OfferReaction = {OldId : int; NewImage : Image; Username : string; Reaction : OfferReactionStatus; DistanceToAttribute : float }
type OfferFreshData =  {OldId : int; NewImage : Image;Status : AttributeStatus} 

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
        loadDynamicDb : unit -> Async<DynamicDbProposals>;
        reactOnOffer : OfferReaction -> Async<OfferFreshData>;
    }
