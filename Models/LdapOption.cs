namespace muhaberat_evrak_yonetim.Models
{
    public class LdapOption
    {
        public string LdapServer { get; set; }
        public string LdapPortNumber { get; set; }
        public string LdapDomain { get; set; }
        public string LdapUser { get; set; }
        public string LdapPassword { get; set; }
        public string OrganizationalUnitFuzulEvUsers { get; set; }
        public string OrganizationalUnitSubeler { get; set; }
    }
}
