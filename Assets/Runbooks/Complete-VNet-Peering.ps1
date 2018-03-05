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
  Creates a VNet peering connection from HubVNet to SpokeVNet and vice versa.

.DESCRIPTION
  This script takes projectName as parameter and completes VNet peering configuration between 
  HubVNet and  SpokeVNet (provisioned in project Resource Group) by adding
  connection from HubVnet to SpokeVNet and vice versa.

.PARAMETER projectName
 Name of the project in Azure

.PARAMETER ResourceGroupName
 Specify name of the project Resource Group in Azure.

.PARAMETER SubscriptionId
 Specify Azure subscription identifier.

.INPUTS
  <Inputs if any, otherwise state None>

.NOTES
  Version:        1.1
  Author:         David Pazdera
  Last Updated:   03.01.2018
  Purpose/Change: Initial script development
  
.EXAMPLE
  Example usage: .\Complete-VNet-Peering.ps1 -projectName myProjectOne -ResourceGroupName myProjectOne-dev-rg -SubscriptionId eefd4836-a496-477e-a6db-46e7c17923ac

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
    [string]$projectName,
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

#Set Error Action to Silently Continue
$ErrorActionPreference = "SilentlyContinue"
# Set verbose output to show (for debugging) by using "continue" value or not show ("SilentlyContinue")
$VerbosePreference = "continue"

# Getting all variables populated
$HubVNetId = Get-AutomationVariable -Name 'CoreVNetId'
$SpokeVNet = Get-AzureRmVirtualNetwork -ResourceGroupName $ResourceGroupName -Name ($projectName + "-vnet")
#$SpokeVNetId = (Get-AzureRmVirtualNetwork -ResourceGroupName $ResourceGroupName -Name $SpokeVNetName).Id.ToString()
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Using $SpokeVNetId virtual network Id for SpokeVNet"
#endregion Declaration

#region Execution
# Creates a new peering connection in the SpokeVNet
Add-AzureRmVirtualNetworkPeering -Name ($projectName + "-vnet" + "-To-vn-we-core") -VirtualNetwork $SpokeVNet -RemoteVirtualNetworkId $HubVNetId -UseRemoteGateways
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Added VNet peering from CORPQA to CORP..."

# Creates a new peering connection in the HubVNet
Select-AzureRmSubscription -Subscription "CORP"
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Switching subscription to CORP"
Add-AzureRmVirtualNetworkPeering -Name ("vn-we-core-To-" + $projectName + "-vnet") -VirtualNetwork (Get-AzureRmVirtualNetwork -Name 'vn-we-core' -ResourceGroupName 'rg-core') -RemoteVirtualNetworkId $SpokeVNet.Id.ToString() -AllowGatewayTransit
Write-Verbose -Message "$(Get-Date -Format g) [NORMAL] - Added VNet peering from CORP to CORPQA..."
#endregion Execution