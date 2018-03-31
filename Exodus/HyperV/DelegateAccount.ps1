# © 2018 Soverance Studios
# Scott McCutchen
# soverance.com
# scott.mccutchen@soverance.com

param (
	[string]$computer = $(throw "-computer is required. You must specify a valid hostname on the network."),
	[string]$path = $(throw "-path is required. You must specify a valid destination path on the network.")
)

# get the remote computer name from the specified path
$uri = New-Object System.Uri($path)

$backuphost = Get-ADComputer -Identity $uri.host
$hypervhost = Get-ADComputer -Identity $computer

# Configure Resource-based Constrained Delegation so as to allow the Hyper-V host machine to write to the network share configured on the machine which the Exodus service was installed
# this solves the kerberos "second hop" authentication problem in PowerShell
# more info here:  https://blogs.technet.microsoft.com/ashleymcglone/2016/08/30/powershell-remoting-kerberos-double-hop-solved-securely/
Set-ADComputer -Identity $backuphost -PrincipalsAllowedToDelegateToAccount $hypervhost
$message = "The principal account " + $computer + " was delegated access to the  " + $env:computername + " account."
Write-EventLog -LogName "Exodus Event Log" -Source "Exodus Source" -EventID 675 -EntryType Information -Message $message

