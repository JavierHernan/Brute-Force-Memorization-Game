# Ports & Protocols Memorization Game
# Pure PowerShell - no extra packages required

$Protocols = @(
    @{ Abbr = "HTTP";  Full = "HyperText Transfer Protocol"; Ports = @(80) },
    @{ Abbr = "HTTPS"; Full = "HyperText Transfer Protocol Secure"; Ports = @(443) },
    @{ Abbr = "SMTP";  Full = "Simple Mail Transfer Protocol"; Ports = @(25) },
    @{ Abbr = "SMTPS"; Full = "Simple Mail Transfer Protocol Secure"; Ports = @(465, 587) },
    @{ Abbr = "POP3";  Full = "Post Office Protocol version 3"; Ports = @(110) },
    @{ Abbr = "POP3S"; Full = "Post Office Protocol version 3 Secure"; Ports = @(995) },
    @{ Abbr = "IMAP";  Full = "Internet Message Access Protocol"; Ports = @(143) },
    @{ Abbr = "IMAPS"; Full = "Internet Message Access Protocol Secure"; Ports = @(993) },
    @{ Abbr = "SSH";   Full = "Secure Shell"; Ports = @(22) },
    @{ Abbr = "SFTP";  Full = "SSH File Transfer Protocol"; Ports = @(22) },
    @{ Abbr = "Telnet";Full = "Telecommunication Network"; Ports = @(23) },
    @{ Abbr = "RDP";   Full = "Remote Desktop Protocol"; Ports = @(3389) },
    @{ Abbr = "FTP";   Full = "File Transfer Protocol"; Ports = @(20, 21) },
    @{ Abbr = "TFTP";  Full = "Trivial File Transfer Protocol"; Ports = @(69) },
    @{ Abbr = "DNS";   Full = "Domain Name System"; Ports = @(53) },
    @{ Abbr = "DHCP";  Full = "Dynamic Host Configuration Protocol"; Ports = @(67, 68) },
    @{ Abbr = "LDAP";  Full = "Lightweight Directory Access Protocol"; Ports = @(389) },
    @{ Abbr = "LDAPS"; Full = "Lightweight Directory Access Protocol Secure"; Ports = @(636) },
    @{ Abbr = "SQL Service (MSSQL)"; Full = "Microsoft SQL Server"; Ports = @(1433) },
    @{ Abbr = "SQL Service (MySQL)"; Full = "MySQL Server"; Ports = @(3306) },
    @{ Abbr = "SNMP";  Full = "Simple Network Management Protocol"; Ports = @(161, 162) },
    @{ Abbr = "SysLog";Full = "System Logging Protocol"; Ports = @(514) },
    @{ Abbr = "NTP";   Full = "Network Time Protocol"; Ports = @(123) },
    @{ Abbr = "SIP";   Full = "Session Initiation Protocol"; Ports = @(5060, 5061) },
    @{ Abbr = "H.323"; Full = $null; Ports = @(1720) },
    @{ Abbr = "SMB and CIFS"; Full = "Server Message Block / Common Internet File System"; Ports = @(445) },
    @{ Abbr = "NetBIOS and NetBT"; Full = "Network Basic Input/Output System"; Ports = @(137, 138, 139) }
)

function Shuffle($list) {
    $a = @($list)
    for ($i = $a.Count - 1; $i -gt 0; $i--) {
        $j = Get-Random -Maximum ($i + 1)
        $temp = $a[$i]
        $a[$i] = $a[$j]
        $a[$j] = $temp
    }
    return $a
}

function Normalize-Ports($input) {
    $nums = $input -split '[/\-\s,]+' |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ -match '^\d+$' } |
            ForEach-Object { [int]$_ } |
            Where-Object { $_ -gt 0 -and $_ -le 65535 } |
            Select-Object -Unique |
            Sort-Object
    return @($nums)
}

function Ports-Equal($a, $b) {
    $sortedB = @($b | Sort-Object)
    if ($a.Count -ne $sortedB.Count) { return $false }
    for ($i = 0; $i -lt $a.Count; $i++) {
        if ($a[$i] -ne $sortedB[$i]) { return $false }
    }
    return $true
}

function Canonical-Ports($ports) {
    return ($ports | Sort-Object) -join "/"
}

function Display-Name($proto) {
    if ($proto.Full) { return "$($proto.Abbr) - $($proto.Full)" }
    else { return $proto.Abbr }
}

function Ask-Protocol($proto, $isReview = $false) {
    $expected = @($proto.Ports | Sort-Object)
    $canon = Canonical-Ports $expected
    $name = Display-Name $proto

    Write-Host ""
    if ($isReview) {
        Write-Host "REVIEW --> Protocol: $name"
    } else {
        Write-Host "Protocol: $name"
    }
    Write-Host "Enter the port number(s) (separate multiple with /, any order is fine):"

    $answer = (Read-Host ">").Trim()
    if ($answer -eq "" -or $answer -eq "quit") { return $null }

    $userPorts = Normalize-Ports $answer

    if (Ports-Equal $userPorts $expected) {
        if ($isReview) { Write-Host "  [Correct]" }
        return $true
    }

    Write-Host ""
    Write-Host "[Incorrect] The correct answer is: $canon"

    while ($true) {
        $ack = (Read-Host "Type the correct port(s) to acknowledge").Trim()
        if ($ack -eq "" -or $ack -eq "quit") { return $null }
        if (Ports-Equal (Normalize-Ports $ack) $expected) { break }
        Write-Host "  That does not match. Please type the correct port(s)."
    }

    Read-Host "Press Enter to continue"
    return $false
}

function Do-Review($mastered) {
    if ($mastered.Count -eq 0) { return $true }

    Write-Host ""
    Write-Host "----------------------------------------"
    Write-Host "  Reviewing previously mastered protocols"
    Write-Host "  (shuffled order)"
    Write-Host "----------------------------------------"

    $reviewList = Shuffle $mastered
    foreach ($proto in $reviewList) {
        $result = Ask-Protocol $proto $true
        if ($null -eq $result) { return $false }
    }

    Write-Host ""
    Write-Host "-- Review complete --"
    return $true
}

# ─── Main Game ────────────────────────────────────────────────────────────────
Clear-Host
Write-Host "======================================================"
Write-Host "     Ports & Protocols Memorization Game"
Write-Host "     Brute-force learning through repetition"
Write-Host "======================================================"
Write-Host ""
Write-Host "- Protocols are presented one at a time in random order."
Write-Host "- Type the port(s) using / as separator (e.g. 20/21)."
Write-Host "- On a miss you must type the correct answer, then re-answer"
Write-Host "  every previously mastered protocol before retrying."
Write-Host "- Score (streak) resets on any incorrect answer."
Write-Host "- Type `"quit`" at any prompt to exit."
Write-Host ""

$remaining = @($Protocols)
$mastered  = @()
$streak    = 0
$pendingRetry = $null
$total     = $Protocols.Count

while ($mastered.Count -lt $total) {

    if ($null -ne $pendingRetry) {
        $proto = $pendingRetry
    }
    elseif ($remaining.Count -gt 0) {
        $idx = Get-Random -Maximum $remaining.Count
        $proto = $remaining[$idx]
    }
    else { break }

    if ($null -ne $pendingRetry) {
        Write-Host ""
        Write-Host ">>> Retrying the protocol you previously missed <<<"
    }

    $result = Ask-Protocol $proto

    if ($null -eq $result) {
        Write-Host ""
        Write-Host "Exiting game. Goodbye!"
        break
    }

    if ($result -eq $true) {
        if ($null -ne $pendingRetry) {
            $pendingRetry = $null
        }
        else {
            $remaining = @($remaining | Where-Object { $_ -ne $proto })
        }

        if ($mastered -notcontains $proto) {
            $mastered += $proto
        }

        $streak++
        Write-Host ""
        Write-Host "[Correct!]  Streak: $streak  |  Mastered: $($mastered.Count)/$total"
    }
    else {
        $streak = 0
        Write-Host ""
        Write-Host "Streak reset to 0.  Mastered so far: $($mastered.Count)/$total"

        $reviewOk = Do-Review $mastered
        if (-not $reviewOk) {
            Write-Host ""
            Write-Host "Exiting game. Goodbye!"
            break
        }

        $pendingRetry = $proto
        Write-Host ""
        Write-Host "Now retrying the protocol you missed..."
        Write-Host ""
    }
}

if ($mastered.Count -eq $total) {
    Write-Host ""
    Write-Host "======================================================"
    Write-Host "  Congratulations! You mastered every protocol!"
    Write-Host "======================================================"
    Write-Host "Final streak: $streak"
    Write-Host "Total protocols: $total"
}

Write-Host ""
Read-Host "Press Enter to close"

//make folder
//put code into folder Program.cs
//open cmd powershell then run this:
//dotnet new console --force
//Then run this:
//dotnet run