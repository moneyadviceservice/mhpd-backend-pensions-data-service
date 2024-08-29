# Install-Module -Name Az.Accounts -Force -Scope CurrentUser
# Install the Az.Accounts module if it's not already installed
if (-not (Get-Module -Name Az.Accounts)) {
    Install-Module -Name Az.Accounts -Force -Scope CurrentUser -ErrorAction Stop
}

#param(
   $cdaServiceEmulatorResourseGroup
   $cdaComponent
   $ipRulesObject
#)

$results = $ipRulesObject | ConvertFrom-Json -AsHashtable;
foreach ($ipRule in $results) {
   if($ipRule.Action -eq "Add")
   {
      try {
            Add-AzWebAppAccessRestrictionRule -ResourceGroupName $cdaServiceEmulatorResourseGroup -WebAppName $cdaComponent -Name $ipRule.Name -Priority 200 -Action Allow -IpAddress $ipRule.IpAddress.ToString()
            Write-Output "Added $($ipRule.name)"
       } catch {
           Write-Output "Failed to add $($ipRule.name). Error: $_"
       }
   }
   if($ipRule.Action -eq "Remove")
   {
      try {
            Remove-AzWebAppAccessRestrictionRule -ResourceGroupName $cdaServiceEmulatorResourseGroup -WebAppName $cdaComponent -Name $ipRule.Name
            Write-Output "Removed $($ipRule.name)"
       } catch {
           Write-Output "Failed to remove $($ipRule.name). Error: $_"
       }
   }
}
