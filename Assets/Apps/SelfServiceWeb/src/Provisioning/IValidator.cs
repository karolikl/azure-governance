using SelfServiceWeb.Domain;
using SelfServiceWeb.ViewModels;
using System.Collections.Generic;

namespace SelfServiceWeb.Provisioning
{
    public interface IValidator
    {
        List<ExistingEnvironment> IsValid(ProvisioningViewModel model);
    }
}
