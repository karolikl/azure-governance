<#PSScriptInfo
    .VERSION 1.1
    .AUTHOR dapazd_msft
    .COMPANYNAME Microsoft Corporation
    .COPYRIGHT 
    .TAGS AzureAutomation Networking Governance Azure
    .RELEASENOTES
#>

#Requires -Module Azure
#requires -version 2
<#
.SYNOPSIS
  Provisions a Spoke VNet.

.DESCRIPTION
  This script takes projectName and AddressPrefix as parameters and provisions a "Spoke" VNet.
  It uses Azure RM template as an input.

.PARAMETER ProjectName
 Name of the project in Azure

 .PARAMETER AddressPrefix
  Network address prefix (from database)

.PARAMETER ResourceGroupName
 Specify name of the project Resource Group in Azure.

.PARAMETER SubscriptionId
 Specify Azure subscription identifier.

.INPUTS
  Variables from Azure Automation account (GovernanceAutomation):
  ProvisionVNetTemplateUri

  Dependency on existing JSON file - Azure Resource Manager Template:
  https://{StorageAccountName}.blob.core.windows.net/governancetemplates/azuredeploy.json

.NOTES
  Version:        1.1
  Author:         David Pazdera
  Last Updated:   03.01.2018
  Purpose/Change: Initial script development
  
.EXAMPLE
  Example usage: .\Provision-VNet.ps1 -ProjectName myProjectOne -AddressPrefix '10.1.0.0/16' -ResourceGroupName myProjectOne-dev-rg -SubscriptionId eefd4836-a496-477e-a6db-46e7c17923ac

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
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectName,
    [Parameter(Mandatory = $true)]
    [string]$AddressPrefix,
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId
 )  
#endregion Parameters

#region Declaration
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Getting Automation Connection and logging in to Azure" -Verbose
$Conn = Get-AutomationConnection -Name AzureRunAsConnection
Add-AzureRMAccount -ServicePrincipal -Tenant $Conn.TenantID -ApplicationId $Conn.ApplicationID -CertificateThumbprint $Conn.CertificateThumbprint
Set-AzureRmContext -SubscriptionId $SubscriptionId
#$deploymentTemplateUri = Get-AutomationVariable -Name ProvisionVNetTemplateUri
$templateParams = @{ProjectName=$ProjectName; AddressPrefix=$AddressPrefix}
#Set Error Action to Silently Continue
$ErrorActionPreference = "SilentlyContinue"
# Set verbose output to show (for debugging) by using "continue" value or not show ("SilentlyContinue")
$VerbosePreference = "continue"
#endregion Declaration

#region Execution
# Executes a new ARM deployment
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Template provisioning..."
New-AzureRmResourceGroupDeployment -ResourceGroupName $ResourceGroupName -Mode Incremental -TemplateUri (Get-AutomationVariable -Name ProvisionVNetTemplateUri) -TemplateParameterObject $templateParams
#endregion Execution