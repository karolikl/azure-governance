namespace CorpDevVSTSRbac
{
    public class MemberEntitlement
    {
        public Entitlement[] Entitlements { get; set; }

        public MemberEntitlement(string projectId)
        {
            Entitlements = new Entitlement[1];
            Entitlements[0] = new Entitlement(projectId);
        }
    }

    public class Entitlement
    {
        public string Op { get { return "add"; } }
        public string Path { get { return "/projectEntitlements"; } }
        public EntitlementValue Value { get; set; }

        public Entitlement(string projectId)
        {
            Value = new EntitlementValue(projectId);
        }

        public class EntitlementValue
        {
            public Projectref ProjectRef { get; set; }
            public Group Group { get; set; }

            public EntitlementValue(string projectId)
            {
                ProjectRef = new Projectref { Id = projectId };
                Group = new Group();
            }
        }

        public class Projectref
        {
            public string Id { get; set; }
        }

        public class Group
        {
            public string GroupType { get { return "projectAdministrator"; } }
        }
    }
}
