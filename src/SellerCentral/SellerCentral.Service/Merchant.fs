[<RequireQualifiedAccess>]
module Merchant

open System

type CompanyName = string
type DisplayName = string
type Reason = string

type Command =
    | Apply of CompanyName * DisplayName
    | Onboard
    | DenyApplication of Reason

type Event =
    | Applied of CompanyName * DisplayName * DateTime
    | Onboarded of DateTime
    | ApplicationDenied of Reason * DateTime

type DenialInfo = { denialDate: DateTime; reason: Reason}

type ApprovalInfo = { approvalDate: DateTime }

type ApplicationState =
    | WaitingForApproval
    | Denied of DenialInfo
    | Approved of ApprovalInfo

type State = {
    applicationState: ApplicationState
    companyName: CompanyName;
    displayName: DisplayName;
    applicationDate: DateTime;
}
with static member Zero = { applicationState=WaitingForApproval; companyName=String.Empty; displayName=String.Empty; applicationDate=DateTime.MinValue }

let apply item = function
    | Applied(companyName, displayName, datetime) -> { item with companyName=companyName; displayName=displayName; applicationDate=datetime }
    | Onboarded approvalDate                      -> { item with applicationState=Approved({ approvalDate=approvalDate }) }
    | ApplicationDenied(reason, datetime)         -> { item with applicationState=Denied({ reason=reason; denialDate=datetime}) }

open Validator

module private Assert =
    let validApplication companyName displayName = notNullOrEmptyString ["Invalid display name"] companyName <* notNullOrEmptyString ["Invalid company name"] displayName
    let isNotOnboarded state = validator (fun m -> match m.applicationState with 
                                                   | Approved _ -> false
                                                   | _          -> true) ["The merchant is already onboarded."] state

let exec state = function
    | Apply (companyName, displayName) -> Assert.validApplication companyName displayName <?> Applied(companyName, displayName, DateTime.UtcNow)
    | Onboard                          -> Assert.isNotOnboarded state                     <?> Onboarded(DateTime.UtcNow)
    | DenyApplication (reason)         -> Assert.isNotOnboarded state                     <?> ApplicationDenied(reason, DateTime.UtcNow)
