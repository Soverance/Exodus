# � 2018 Soverance Studios
# Scott McCutchen
# soverance.com
# scott.mccutchen@soverance.com

param (
	[string]$hypervhost = $(throw "-host is required. You must specify a valid Hyper-V host on the network."),
	[string]$vm = $(throw "-vm is required. You must specify a valid Hyper-V guest on the network."),
	[string]$path = $(throw "-path is required. You must specify a valid backup destination path on the network.")
)

$message = "Full Backup of " + $vm + " initiated..."
Write-EventLog -LogName "Exodus Event Log" -Source "Exodus Source" -EventID 625 -EntryType Information -Message $message

# Process an export of the specified Hyper-V virtual machine
$ExportJob = Export-VM -ComputerName $hypervhost -Name $vm -Path $path -AsJob

while ($ExportJob.State -eq "Running" -or $ExportJob.State -eq "NotStarted")
{
	# log backup progress - REMOVED TO AVOID LOG CLUTTER
	#$message = $vm + " export progress: " + $ExportJob.Progress.PercentComplete + "% complete."
	#Write-EventLog -LogName "Exodus Event Log" -Source "Exodus Source" -EventID 625 -EntryType Information -Message $message
	sleep(60)
}

if ($ExportJob.State -ne "Completed")
{
	$message = $vm + " export job did not complete.  STATUS: " + $ExportJob.State
	Write-EventLog -LogName "Exodus Event Log" -Source "Exodus Source" -EventID 624 -EntryType Error -Message $message
}

if ($ExportJob.State -eq "Completed")
{
	$message = $vm + " export job has finished."
	Write-EventLog -LogName "Exodus Event Log" -Source "Exodus Source" -EventID 626 -EntryType Information -Message $message
}