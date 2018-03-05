$(document).ready(function () {
    provisionAzureChanged();
});

function provisionAzureChanged() {
    console.log("provisionAzureChanged");
    if ($('#ProvisionAzure').is(":checked")) 
        $(".noshow").show();
    else
        $(".noshow").hide();
}