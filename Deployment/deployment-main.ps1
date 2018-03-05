<#PSScriptInfo
    .VERSION 1.0
    .AUTHOR dapazd_msft
    .COMPANYNAME Microsoft Corporation
    .COPYRIGHT 
    .TAGS Governance Azure Deployment
    .RELEASENOTES
#>

#Requires -Module Azure
#requires -version 2
<#
.SYNOPSIS
  Provisions all Azure resources for Azure Governance Model.

.DESCRIPTION
  This script creates a new RG and populates it with all necessary components that make this model.

.PARAMETER GovResourceGroupName
 Specify name of a Resource Group in Azure that will host all components.

.PARAMETER Location
 Specify a location for RG and all resources (components).

.PARAMETER SubscriptionId
 Specify Azure subscription identifier.

.INPUTS
  None


.OUTPUTS

.NOTES
  Version:        1.0
  Author:         David Pazdera
  Last Updated:   05.03.2018
  
.EXAMPLE
  Example usage: .\deployment-main.ps1 -GovResourceGroupName azure-governance-rg -SubscriptionId eefd4836-a496-477e-a6db-46e7c17923ac

.LINK
  https://github.com/karolikl/azure-governance/blob/master/README.md

.NOTES

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
    [string]$GovResourceGroupName,
    [parameter(Mandatory = $true)]
    [string]$Location,
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId
 )  
#endregion Parameters

#region Declaration
Write-Host "Logging in...";
Login-AzureRmAccount
Write-Host "Selecting subscription '$subscriptionId'";
Set-AzureRmContext -SubscriptionId $SubscriptionId

$templateFilePath = 'azuredeploymain.json'
$parametersFilePath = 'azuredeploymain.parameters.json'

#endregion Declaration

#region Execution

# Step 1: Create a Resource Group
Get-AzureRmResourceGroup -Name $GovResourceGroupName -ErrorVariable notPresent -ErrorAction SilentlyContinue
if ($notPresent)
{
    Write-Host "Creating a new Resource Group - '$GovResourceGroupName' for Governance";
    New-AzureRmResourceGroup -Name $GovResourceGroupName -Location $Location
}
else
{
    Write-Error -Message "Resource group '$GovResourceGroupName' already exists, select a different one!"
    Exit
}

# Step 2: Provision all components with a set of ARM templates
Write-Host "Starting deployment...";
if(Test-Path $parametersFilePath) {
    New-AzureRmResourceGroupDeployment -ResourceGroupName $GovResourceGroupName -TemplateFile $templateFilePath -TemplateParameterFile $parametersFilePath;
} else {
    New-AzureRmResourceGroupDeployment -ResourceGroupName $GovResourceGroupName -TemplateFile $templateFilePath;
}

# Step 3: Populate Automation Account with assets and runbooks



# Step 4: Populate Azure Storage with content




#endregion Execution