<#PSScriptInfo
    .VERSION 1.1
    .AUTHOR dapazd_msft
    .COMPANYNAME Microsoft Corporation
    .COPYRIGHT 
    .TAGS AzureAutomation Policies Governance Azure
    .RELEASENOTES
#>

#Requires -Module Azure
#requires -version 2
<#
.SYNOPSIS
  Assign defined policies to a "project" Resource Group.

.DESCRIPTION
  This script takes projectName as a parameter and assigns three specific Azure Policies to respective project RG.

.PARAMETER ResourceGroupName
 Specify name of the project Resource Group in Azure.

.PARAMETER SubscriptionId
 Specify Azure subscription identifier.

.INPUTS
  Variables from Azure Automation account (GovernanceAutomation):
  AllowedVMSKUsURL
  TagsEnforcementPolicyDefinitionId
  DisallowPIPPolicyDefinitionId
  AllowedVMsPolicyDefinitionId

  Dependency on existing JSON file with Azure VM SKUs for AllowedVMsPolicyDefinitionId:
  https://{storageAccountName}.blob.core.windows.net/governancetemplates/allowedVMSKUs.json

.OUTPUTS

.NOTES
  Version:        1.1
  Author:         David Pazdera
  Last Updated:   03.01.2018
  Purpose/Change: Initial script development
  
.EXAMPLE
  Example usage: .\Assign-Policies.ps1 -ResourceGroupName myProjectOne-rg -SubscriptionId eefd4836-a496-477e-a6db-46e7c17923ac

.LINK
  <Links to further documentation>

.NOTES
  This script is intended to run as an Azure Automation runbook.

# ---Disclaimer---
# This Sample Code is provided for the purpose of illustration only and is not intended to be used 
# in a production environment.  THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT 
# WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
# OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.  We grant You a nonexclusive, 
# royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code 
# form of the Sample Code, provided that You agree: (i) to not use Our name, logo, or trademarks to market 
# Your software product in which the Sample Code is embedded; (ii) to include a valid copyright notice 
# on Your software product in which the Sample Code is embedded; and (iii) to indemnify, hold harmless, 
# and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, 
# that arise or result from the use or distribution of the Sample Code.
# Please note: None of the conditions outlined in the disclaimer above will supersede the terms and 
# conditions contained within the Premier Customer Services Description.

#>

#region Parameters
Param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId
   )
#endregion Parameters

#region Declaration
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Getting Automation Connection and logging in to Azure"
$Conn = Get-AutomationConnection -Name AzureRunAsConnection
Add-AzureRMAccount -ServicePrincipal -Tenant $Conn.TenantID -ApplicationId $Conn.ApplicationID -CertificateThumbprint $Conn.CertificateThumbprint
Set-AzureRmContext -SubscriptionId $SubscriptionId

# Set Error Action to Silently Continue
#$ErrorActionPreference = "SilentlyContinue"
# Set verbose output to show (for debugging) by using "continue" value or not show ("SilentlyContinue")
$VerbosePreference = "Continue"

# Getting parameters
$rg = Get-AzureRmResourceGroup -Name $ResourceGroupName
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Using $rg.Name Resource Group"
$policyParams = @{TAGVALUE_1=$rg.Tags.projectname; TAGVALUE_2=$rg.Tags.owner}
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Using $policyParams.projectName as projectName and $policyParameters.Owner as Owner"
$allowedVMsRequestURI = Get-AutomationVariable -Name 'AllowedVMSKUsURL'
$allowedVMs = (Invoke-RestMethod -Uri $allowedVMsRequestURI).listOfAllowedSKUs
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - List of allowed VMs: $allowedVMs"
#endregion Declaration

#region Execution
# Assignment of 'Resource Tags Enforcement' policy
$definitioninit1 = Get-AzureRmPolicySetDefinition -Id (Get-AutomationVariable -Name 'TagsEnforcementPolicyDefinitionId')
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Loading $definitioninit1.Name initiative definition"
New-AzureRmPolicyAssignment -Name "Resource Tags Enforcement" -Scope $rg.ResourceId -PolicySetDefinition $definitioninit1 -PolicyParameterObject $policyParams
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Adding Resource Tags Enforcement policy assignment for $rg.ResourceId Resource Group"

# Assignment of 'Disallow Public IP' policy
$definition1 = Get-AzureRmPolicyDefinition -Id (Get-AutomationVariable -Name 'DisallowPIPPolicyDefinitionId')
New-AzureRMPolicyAssignment -Name "Disallow Public IP" -Scope $rg.ResourceId -PolicyDefinition $definition1
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Adding Disallow Public IP policy assignment for $rg.ResourceId Resource Group"

# Assignment of 'Allow VM SKUs' policy
#$definition2 = Get-AzureRmPolicyDefinition -Id '/providers/Microsoft.Authorization/policyDefinitions/cccc23c7-8427-4f53-ad12-b6a63eb452b3'
$definition2 = Get-AzureRmPolicyDefinition -Id (Get-AutomationVariable -Name 'AllowedVMsPolicyDefinitionId')
New-AzureRMPolicyAssignment -Name "Allow VM SKUs" -Scope $rg.ResourceId -PolicyDefinition $definition2 -PolicyParameterObject @{"listOfAllowedSKUs"=$allowedVMs}
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Adding Allow VM SKUs policy assignment for $rg.ResourceId Resource Group."
#endregion Execution