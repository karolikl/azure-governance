using System;

namespace CorpDevVSTSRbac
{
    public class MemberEntitlementResponse
    {
        public Operationresult[] operationResults { get; set; }
        public bool isSuccess { get; set; }
        public Memberentitlement memberEntitlement { get; set; }
    }

    public class Memberentitlement
    {
        public Member member { get; set; }
        public string id { get; set; }
        public User user { get; set; }
        public Accesslevel accessLevel { get; set; }
        public DateTime lastAccessedDate { get; set; }
        public object[] projectEntitlements { get; set; }
        public Extension[] extensions { get; set; }
        public object[] groupAssignments { get; set; }
    }

    public class Member
    {
        public string subjectKind { get; set; }
        public string metaType { get; set; }
        public string domain { get; set; }
        public string principalName { get; set; }
        public string mailAddress { get; set; }
        public string origin { get; set; }
        public string originId { get; set; }
        public string displayName { get; set; }
        public _Links _links { get; set; }
        public string url { get; set; }
        public string descriptor { get; set; }
    }

    public class _Links
    {
        public Self self { get; set; }
        public Memberships memberships { get; set; }
        public Membershipstate membershipState { get; set; }
        public Storagekey storageKey { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Memberships
    {
        public string href { get; set; }
    }

    public class Membershipstate
    {
        public string href { get; set; }
    }

    public class Storagekey
    {
        public string href { get; set; }
    }

    public class User
    {
        public string subjectKind { get; set; }
        public string metaType { get; set; }
        public string domain { get; set; }
        public string principalName { get; set; }
        public string mailAddress { get; set; }
        public string origin { get; set; }
        public string originId { get; set; }
        public string displayName { get; set; }
        public _Links1 _links { get; set; }
        public string url { get; set; }
        public string descriptor { get; set; }
    }

    public class _Links1
    {
        public Self1 self { get; set; }
        public Memberships1 memberships { get; set; }
        public Membershipstate1 membershipState { get; set; }
        public Storagekey1 storageKey { get; set; }
    }

    public class Self1
    {
        public string href { get; set; }
    }

    public class Memberships1
    {
        public string href { get; set; }
    }

    public class Membershipstate1
    {
        public string href { get; set; }
    }

    public class Storagekey1
    {
        public string href { get; set; }
    }

    public class Accesslevel
    {
        public string licensingSource { get; set; }
        public string accountLicenseType { get; set; }
        public string msdnLicenseType { get; set; }
        public string licenseDisplayName { get; set; }
        public string status { get; set; }
        public string statusMessage { get; set; }
        public string assignmentSource { get; set; }
    }

    public class Extension
    {
        public string id { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string assignmentSource { get; set; }
    }

    public class Operationresult
    {
        public bool isSuccess { get; set; }
        public Error[] errors { get; set; }
        public object result { get; set; }
        public string memberId { get; set; }
    }

    public class Error
    {
        public int key { get; set; }
        public string value { get; set; }
    }

}
