namespace SelfServiceWeb.Provisioning
{
    public interface ISettings
    {
        string GetSubscriptionId(string environment);
        string GetProvisioningUrl();
        string GetProjectDbConnectionString();
    }
}
