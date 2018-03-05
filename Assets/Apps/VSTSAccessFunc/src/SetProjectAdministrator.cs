using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.Licensing;
using Microsoft.VisualStudio.Services.Licensing.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CorpDevVSTSRbac
{
    public static class SetProjectAdministrator
    {
        public class Project
        {
            public string ProjectId { get; set; }
            public string Owner { get; set; }
        }

        [FunctionName("SetProjectAdministrator")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            Project project = await req.Content.ReadAsAsync<Project>();
            log.Info("C# HTTP trigger function processed a request.");

            TypeDescriptor.AddAttributes(typeof(IdentityDescriptor), new TypeConverterAttribute(typeof(IdentityDescriptorConverter).FullName));
            TypeDescriptor.AddAttributes(typeof(SubjectDescriptor), new TypeConverterAttribute(typeof(SubjectDescriptorConverter).FullName));

            Guid userId = await AddMemberToAccount(project, log);
            if (userId == Guid.Empty)
                return req.CreateResponse(HttpStatusCode.InternalServerError);

            log.Info($"UserId: {userId}");
            bool success = await AssignProjectAdminstratorRights(project, userId, log);
            return success ? req.CreateResponse(HttpStatusCode.OK) : req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        private async static Task<bool> AssignProjectAdminstratorRights(Project project, Guid userId, TraceWriter log)
        {
            string url = ConfigurationManager.AppSettings.Get("MemberEntitlementUrl");
            if (string.IsNullOrWhiteSpace(url))
            {
                log.Error("MemberEntitlementUrl appsetting missing. Format: https://{instance}/_apis/memberentitlements/{memberIdentifier}?api-version={version}");
                return false;
            }

            string accountName = ConfigurationManager.AppSettings.Get("VSTSAccountName");
            string username = ConfigurationManager.AppSettings.Get("VSTSUserName");
            string pattoken = ConfigurationManager.AppSettings.Get("VSTSPat");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pattoken) || string.IsNullOrWhiteSpace(accountName))
            {
                log.Error("AppSettings missing for VSTSUserName, VSTSPat or VSTSAccountName");
                return false;
            }

            url = url.Replace("{accountName}", accountName);
            url = url.Replace("{memberIdentifier}", userId.ToString());

            var client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json-patch+json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{pattoken}")));

            log.Info($"Creating a new member entitlement for user: {userId} in project:{project.ProjectId}");
            MemberEntitlement entitlement = new MemberEntitlement(project.ProjectId);
            StringContent content = new StringContent(JsonConvert.SerializeObject(entitlement.Entitlements).ToLower(), Encoding.UTF8, "application/json-patch+json");
            HttpResponseMessage response = await client.PatchAsync(url, content);
            response.EnsureSuccessStatusCode();

            var memberEntitlementResponse = await response.Content.ReadAsAsync<MemberEntitlementResponse>();

            log.Info($"Finished creating a new member entitlement. IsSuccess: {memberEntitlementResponse.isSuccess}");
            if (!memberEntitlementResponse.isSuccess)
            {
                foreach (var operationResults in memberEntitlementResponse.operationResults)
                {
                    foreach (var error in operationResults.errors)
                    {
                        log.Error($"Error! Key: {error.key}. Value: {error.value}");
                    }
                }
                return false;
            }

            return true;
        }

        private async static Task<Guid> AddMemberToAccount(Project project, TraceWriter log)
        {
            string username = ConfigurationManager.AppSettings.Get("VSTSUserName");
            string pattoken = ConfigurationManager.AppSettings.Get("VSTSPat");
            string accountName = ConfigurationManager.AppSettings.Get("VSTSAccountName");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pattoken) || string.IsNullOrWhiteSpace(accountName))
            {
                log.Error("AppSettings missing for VSTSUserName, VSTSPat or VSTSAccountName");
                return Guid.Empty;
            }

            try
            {
                VssBasicCredential credentials = new VssBasicCredential(username, pattoken);
                VssConnection connection = new VssConnection(new Uri($"https://{accountName}.vssps.visualstudio.com/"), credentials);

                var identityClient = connection.GetClient<IdentityHttpClient>();
                var licensingClient = connection.GetClient<LicensingHttpClient>();
                var userIdentity = identityClient.ReadIdentitiesAsync(IdentitySearchFilter.AccountName, project.Owner).Result.FirstOrDefault();

                // If the identity is null, this is a user that has not yet been added to the account.
                // We'll need to add the user as a "bind pending" - meaning that the email address of the identity is 
                // recorded so that the user can log into the account, but the rest of the details of the identity 
                // won't be filled in until first login.
                if (userIdentity != null)
                {
                    log.Info($"User:{project.Owner} already exists in VSTS account with name: {accountName}");
                    return userIdentity.Id;
                }

                log.Info("Creating a new identity and adding it to the collection's licensed users group.");
                var collectionScope = identityClient.GetScopeAsync(accountName).Result;

                // First get the descriptor for the licensed users group, which is a well known (built in) group.
                var licensedUsersGroupDescriptor = new IdentityDescriptor(IdentityConstants.TeamFoundationType,
                                                                          GroupWellKnownSidConstants.LicensedUsersGroupSid);

                // Now convert that into the licensed users group descriptor into a collection scope identifier.
                var identifier = string.Concat(SidIdentityHelper.GetDomainSid(collectionScope.Id),
                                               SidIdentityHelper.WellKnownSidType,
                                               licensedUsersGroupDescriptor.Identifier.Substring(SidIdentityHelper.WellKnownSidPrefix.Length));

                // Here we take the string representation and create the strongly-type descriptor
                var collectionLicensedUsersGroupDescriptor = new IdentityDescriptor(IdentityConstants.TeamFoundationType,
                                                                                    identifier);

                // Get the domain from the user that runs this code. This domain will then be used to construct
                // the bind-pending identity. The domain is either going to be "Windows Live ID" or the Azure 
                // Active Directory (AAD) unique identifier, depending on whether the account is connected to
                // an AAD tenant. Then we'll format this as a UPN string.
                var currUserIdentity = connection.AuthorizedIdentity.Descriptor;
                if (!currUserIdentity.Identifier.Contains('\\'))
                {
                    log.Error("Could not find directory for user");
                    return Guid.Empty;
                }

                // The identifier is domain\userEmailAddress, which is used by AAD-backed accounts.
                // We'll extract the domain from the admin user.
                var directory = currUserIdentity.Identifier.Split(new char[] { '\\' })[0];
                var upnIdentity = string.Format("upn:{0}\\{1}", directory, project.Owner);

                // Next we'll create the identity descriptor for a new "bind pending" user identity.
                var newUserDesciptor = new IdentityDescriptor(IdentityConstants.BindPendingIdentityType,
                                                              upnIdentity);

                // We are ready to actually create the "bind pending" identity entry. First we have to add the
                // identity to the collection's licensed users group. Then we'll retrieve the Identity object
                // for this newly-added user. Without being added to the licensed users group, the identity 
                // can't exist in the account.
                bool result = identityClient.AddMemberToGroupAsync(collectionLicensedUsersGroupDescriptor,
                                                                   newUserDesciptor).Result;
                userIdentity = identityClient.ReadIdentitiesAsync(IdentitySearchFilter.AccountName,
                                                                  project.Owner).Result.FirstOrDefault();

                log.Info("Assigning license to user.");
                Microsoft.VisualStudio.Services.Licensing.License licence = GetLicense();
                var entitlement = licensingClient.AssignEntitlementAsync(userIdentity.Id, licence, false).Result;
                log.Info($"Added {project.Owner} as a user with license:{licence.ToString()} to account with name:{accountName}");
                return userIdentity.Id;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return Guid.Empty;
            }
        }

        private static Microsoft.VisualStudio.Services.Licensing.License GetLicense()
        {
            Microsoft.VisualStudio.Services.Licensing.License license = AccountLicense.Stakeholder;
            string licenseType = ConfigurationManager.AppSettings.Get("License");

            switch (licenseType)
            {
                case "Basic":
                    license = AccountLicense.Express;
                    break;
                case "Professional":
                    license = AccountLicense.Professional;
                    break;
                case "Advanced":
                    license = AccountLicense.Advanced;
                    break;
                case "Msdn": // When the user logs in, the system will determine the actual MSDN benefits for the user.
                    license = MsdnLicense.Eligible;
                    break;
                case "Stakeholder":
                    license = AccountLicense.Stakeholder;
                    break;
            }
            return license;
        }
    }
}
