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
  Assign an owner to a Resource Group.

.DESCRIPTION
  This script takes ResourceGroupName and E-mail addres as parameters and assigns the users as owner of the respective project RG.

.PARAMETER ResourceGroupName
 Specify name of the project Resource Group in Azure.

.PARAMETER UserObjectId
 Specify owner's ObjectId in Azure AD.

.PARAMETER SubscriptionId
 Specify Azure subscription identifier.

.INPUTS
  Variables from Azure Automation account (GovernanceAutomation):
  None


.OUTPUTS

.NOTES
  Version:        1.1
  Author:         David Pazdera
  Last Updated:   03.01.2018
  
.EXAMPLE
  Example usage: .\Assign-Owner.ps1 -ResourceGroupName myProjectOne-rg -UserObjectId d0a433dc-337c-4f16-bfc6-e20a5d3abc7f -SubscriptionId eefd4836-a496-477e-a6db-46e7c17923ac

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
    [parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [string]$UserObjectId,
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId
 )  
#endregion Parameters

#region Declaration
$Conn = Get-AutomationConnection -Name AzureRunAsConnection
Add-AzureRMAccount -ServicePrincipal -Tenant $Conn.TenantID -ApplicationId $Conn.ApplicationID -CertificateThumbprint $Conn.CertificateThumbprint
Set-AzureRmContext -SubscriptionId $SubscriptionId
#$User = Get-AzureRmADUser -Mail $Email
#$User = Get-AzureADUser -Mail $Email
#endregion Declaration

#region Execution
#New-AzureRmRoleAssignment -SignInName $Email -RoleDefinitionName "Owner" -ResourceGroupName $ResourceGroupName
#New-AzureRmRoleAssignment -ObjectId $User.Id -RoleDefinitionName "Owner" -ResourceGroupName $ResourceGroupName
New-AzureRmRoleAssignment -ObjectId $UserObjectId -RoleDefinitionName "Owner" -ResourceGroupName $ResourceGroupName
#endregion Execution