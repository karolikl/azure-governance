using System.ComponentModel.DataAnnotations;

namespace SelfServiceWeb.ViewModels
{
    public class ProvisioningViewModel
    {
        public string OwnerObjectId { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(40)]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "Only letters and numbers are allowed.")]
        public string ProjectName { get; set; }

        [Display(Name = "Would you like to provision an environment in Azure?")]
        public bool ProvisionAzure { get; set; }

        [Display(Name = "Development environment")]
        public bool ProvisionDevEnvironment { get; set; }

        [Display(Name = "Test environment")]
        public bool ProvisionTestEnvironment { get; set; }

        [Display(Name = "Staging environment")]
        public bool ProvisionStagingEnvironment { get; set; }

        [Display(Name = "Would you like to provision a project in VSTS?")]
        public bool ProvisionVSTS { get; set; }

        public string ErrorMessage { get; set; }
    }
}
