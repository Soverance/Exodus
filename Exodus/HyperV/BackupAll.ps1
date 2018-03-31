# © 2018 Soverance Studios
# Scott McCutchen
# soverance.com
# scott.mccutchen@soverance.com

param (
	[string]$hypervhost = $(throw "-computer is required. You must specify a valid hostname on the network."),
	[string]$path = $(throw "-path is required. You must specify a valid destination path on the network.")
)

Write-EventLog -LogName "Exodus Event Log" -Source "Exodus Source" -EventID 625 -EntryType Information -Message "Full Backup of Hyper-V initiated..."
# Process a full export of all machines on the Hyper-V host
Get-VM -ComputerName $hypervhost | Export-VM -Path $path

