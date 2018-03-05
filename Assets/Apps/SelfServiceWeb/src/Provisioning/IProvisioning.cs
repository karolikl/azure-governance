using SelfServiceWeb.Domain;
using SelfServiceWeb.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SelfServiceWeb.Provisioning
{
    public interface IProvisioning
    {
        Task<bool> Run(ProvisioningViewModel provisioningViewModel, bool vnetCreatedBefore);
    }
}
