# Introduction 
This function stores the project into table storage, assigning it an address prefix used for VNET peering and a spend limit. 

# Getting Started
This function works towards a storage account with three tables: 

*tblAddressPrefix*
PartitionKey: "AddressPrefix"
RowKey: int
Assigned: true/false (bool)
IPAddress: The Address Prefix to be used (string)

*tblSpendLimit*
PartitionKey: "SpendLimit"
RowKey: Possible values: "dev", "test", "staging" (string)
Limit: The cost limit for this environment (int)

*tblcorpdev*
PartitionKey: ProjectName (string)
RowKey: Possible values: "dev", "test", "staging" (string)
Owner: Email address of owner (string)
AddressPrefix: An assigned address prefix from tblAddressPrefix (string)
SpendLimit: Assigned spend limit (int)
