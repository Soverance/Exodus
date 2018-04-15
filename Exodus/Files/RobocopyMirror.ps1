# © 2018 Soverance Studios
# Scott McCutchen
# soverance.com
# scott.mccutchen@soverance.com

param (
	[string]$source = $(throw "-source is required. You must specify a valid hostname on the network."),
	[string]$destination = $(throw "-destination is required. You must specify a valid destination path on the network.")
)

Write-EventLog -LogName "Exodus Event Log" -Source "Exodus Source" -EventID 725 -EntryType Information -Message "RoboCopy mirror initiated..."

robocopy $source $destination /COPYALL /B /SEC /MIR /R:0 /W:0 /NFL /NDL
