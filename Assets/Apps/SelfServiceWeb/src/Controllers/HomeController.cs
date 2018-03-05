using Microsoft.AspNetCore.Mvc;
using SelfServiceWeb.Domain;
using SelfServiceWeb.Provisioning;
using SelfServiceWeb.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SelfServiceWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IValidator _validator;
        private readonly IProvisioning _provisioning;

        public HomeController(IValidator validator, IProvisioning provisioning)
        {
            _validator = validator;
            _provisioning = provisioning;
        }

        public IActionResult Index()
        {
            var provisioningViewModel = new ProvisioningViewModel();
            SetUserMetadata(provisioningViewModel);

            return View(provisioningViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProvisioningViewModel provisioningViewModel)
        {
            SetUserMetadata(provisioningViewModel);

            if (string.IsNullOrWhiteSpace(provisioningViewModel.Email) || string.IsNullOrWhiteSpace(provisioningViewModel.OwnerObjectId))
            {
                provisioningViewModel.ErrorMessage = "Could not retrieve objectId or email from currently logged in user";
                return View(provisioningViewModel);
            }

            if (!ModelState.IsValid)
            {
                return View(provisioningViewModel);
            }

            List<ExistingEnvironment> existingEnvironments = _validator.IsValid(provisioningViewModel);
            if (existingEnvironments.Any())
            {
                string errorMessage = string.Empty;
                foreach (var environment in existingEnvironments)
                {
                    if (!string.IsNullOrWhiteSpace(environment.ErrorMessage))
                        errorMessage += environment.ErrorMessage;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    provisioningViewModel.ErrorMessage = errorMessage;
                    return View(provisioningViewModel);
                }
            }

            if (!provisioningViewModel.ProvisionAzure && !provisioningViewModel.ProvisionVSTS)
            {
                provisioningViewModel.ErrorMessage = "Please select a provisioning option below!";
                return View(provisioningViewModel);
            }

            var success = await _provisioning.Run(provisioningViewModel, existingEnvironments.Any());

            if (success)
            {
                return View("Success");
            }
            else
            {
                return View("Error");
            }
        }

        private void SetUserMetadata(ProvisioningViewModel provisioningViewModel)
        {
            //TODO: Remove default
            provisioningViewModel.Email = GetValueOfUserClaim("name");
            provisioningViewModel.OwnerObjectId = GetValueOfUserClaim("http://schemas.microsoft.com/identity/claims/objectidentifier");
        }

        private string GetValueOfUserClaim(string type)
        {
            if (HttpContext.User == null)
                return null;

            if (!HttpContext.User.Claims.Any())
                return null;

            return HttpContext.User.Claims.Where(c => c.Type == type)?.FirstOrDefault()?.Value;
        }
    }
}
