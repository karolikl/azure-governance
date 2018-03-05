using SelfServiceWeb.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SelfServiceWeb.Provisioning
{
    public interface IWorkflow
    {
        Task<bool> Trigger(ProvisioningRequest request);
    }
}
