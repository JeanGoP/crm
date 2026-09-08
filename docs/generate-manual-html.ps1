$ErrorActionPreference = 'Stop'

$mdPath = Join-Path $PSScriptRoot 'MANUAL_DEL_SISTEMA.md'
$htmlPath = Join-Path $PSScriptRoot 'MANUAL_DEL_SISTEMA.html'
$lines = Get-Content $mdPath -Encoding UTF8
$sb = [System.Text.StringBuilder]::new()

function Enc([string]$value) {
    [System.Net.WebUtility]::HtmlEncode($value)
}

function InlineMd([string]$value) {
    $encoded = Enc $value
    $encoded = [regex]::Replace($encoded, '\*\*(.+?)\*\*', '<strong>$1</strong>')
    $encoded = [regex]::Replace($encoded, '`(.+?)`', '<code>$1</code>')
    return $encoded
}

function Close-List {
    if ($script:inList) {
        [void]$script:sb.AppendLine('</ul>')
        $script:inList = $false
    }
}

function Close-Table {
    if ($script:inTable) {
        [void]$script:sb.AppendLine('</tbody></table>')
        $script:inTable = $false
        $script:tableHeader = $false
    }
}

function Add-Paragraph([string]$value) {
    Close-List
    Close-Table
    [void]$script:sb.AppendLine('<p>' + (InlineMd $value) + '</p>')
}

$style = ':root{color-scheme:light;--ink:#111827;--muted:#526071;--line:#d8e0e8;--brand:#155e75;--paper:#fff;--bg:#f5f7fb}body{margin:0;font-family:Inter,Segoe UI,Arial,sans-serif;background:var(--bg);color:var(--ink);line-height:1.65}main{max-width:1040px;margin:0 auto;padding:42px 24px 72px}h1{font-size:38px;line-height:1.1;margin:0 0 18px;color:#0f172a}h2{font-size:26px;margin:44px 0 12px;padding-top:18px;border-top:1px solid var(--line);color:#0f172a}h3{font-size:18px;margin:24px 0 8px;color:var(--brand)}p,li{font-size:15px;color:var(--ink)}p{margin:8px 0}ul{margin:8px 0 14px 24px;padding:0}code{background:#eef5f8;color:#0f4f63;padding:2px 5px;border-radius:4px}figure{margin:18px 0 28px;border:1px solid var(--line);background:var(--paper);border-radius:10px;overflow:hidden;box-shadow:0 8px 22px rgba(15,23,42,.08)}figure img{display:block;width:100%;height:auto}figcaption{font-size:13px;color:var(--muted);padding:10px 14px;background:#fbfdff;border-top:1px solid var(--line)}table{width:100%;border-collapse:collapse;margin:14px 0 24px;background:var(--paper);border:1px solid var(--line)}th,td{text-align:left;padding:10px 12px;border-bottom:1px solid var(--line);font-size:14px;vertical-align:top}th{background:#eef5f8;color:#0f4f63}.cover{background:#fff;border:1px solid var(--line);border-radius:10px;padding:24px 28px;margin-bottom:28px;box-shadow:0 8px 22px rgba(15,23,42,.06)}@media print{body{background:white}main{max-width:none;padding:0}figure{break-inside:avoid;box-shadow:none}h2{break-after:avoid}.cover{box-shadow:none}}'

[void]$sb.AppendLine('<!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Manual de usuario - EnMarcha CRM</title><style>' + $style + '</style></head><body><main>')

$script:inList = $false
$script:inTable = $false
$script:tableHeader = $false

foreach ($line in $lines) {
    $trim = $line.TrimEnd()

    if ([string]::IsNullOrWhiteSpace($trim)) {
        Close-List
        Close-Table
        continue
    }

    if ($trim -match '^# (.+)$') {
        Close-List
        Close-Table
        [void]$sb.AppendLine('<div class="cover"><h1>' + (InlineMd $Matches[1]) + '</h1>')
        continue
    }

    if ($trim -match '^Ultima actualizacion: (.+)$') {
        [void]$sb.AppendLine('<p><strong>Ultima actualizacion:</strong> ' + (InlineMd $Matches[1]) + '</p>')
        continue
    }

    if ($trim -match '^Version del manual: (.+)$') {
        [void]$sb.AppendLine('<p><strong>Version del manual:</strong> ' + (InlineMd $Matches[1]) + '</p>')
        continue
    }

    if ($trim -match '^Sistema: (.+)$') {
        [void]$sb.AppendLine('<p><strong>Sistema:</strong> ' + (InlineMd $Matches[1]) + '</p></div>')
        continue
    }

    if ($trim -match '^## (.+)$') {
        Close-List
        Close-Table
        [void]$sb.AppendLine('<h2>' + (InlineMd $Matches[1]) + '</h2>')
        continue
    }

    if ($trim -match '^### (.+)$') {
        Close-List
        Close-Table
        [void]$sb.AppendLine('<h3>' + (InlineMd $Matches[1]) + '</h3>')
        continue
    }

    if ($trim -match '^!\[(.*?)\]\((.*?)\)$') {
        Close-List
        Close-Table
        $alt = $Matches[1]
        $rel = $Matches[2].Replace('./', '')
        $imgPath = Join-Path $PSScriptRoot $rel

        if (Test-Path $imgPath) {
            $ext = [System.IO.Path]::GetExtension($imgPath).TrimStart('.').ToLowerInvariant()
            if ($ext -eq 'jpg') { $ext = 'jpeg' }
            $b64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($imgPath))
            [void]$sb.AppendLine('<figure><img alt="' + (Enc $alt) + '" src="data:image/' + $ext + ';base64,' + $b64 + '"><figcaption>' + (Enc $alt) + '</figcaption></figure>')
        }
        continue
    }

    if ($trim -match '^- (.+)$') {
        Close-Table
        if (-not $script:inList) {
            [void]$sb.AppendLine('<ul>')
            $script:inList = $true
        }
        [void]$sb.AppendLine('<li>' + (InlineMd $Matches[1]) + '</li>')
        continue
    }

    if ($trim -match '^\|(.+)\|$') {
        Close-List
        $cells = $trim.Trim('|').Split('|') | ForEach-Object { $_.Trim() }
        $isSeparator = $true
        foreach ($cell in $cells) {
            if ($cell -notmatch '^:?-{3,}:?$') { $isSeparator = $false }
        }
        if ($isSeparator) { continue }

        if (-not $script:inTable) {
            [void]$sb.AppendLine('<table>')
            $script:inTable = $true
            $script:tableHeader = $true
        }

        if ($script:tableHeader) {
            [void]$sb.Append('<thead><tr>')
            foreach ($cell in $cells) { [void]$sb.Append('<th>' + (InlineMd $cell) + '</th>') }
            [void]$sb.AppendLine('</tr></thead><tbody>')
            $script:tableHeader = $false
        } else {
            [void]$sb.Append('<tr>')
            foreach ($cell in $cells) { [void]$sb.Append('<td>' + (InlineMd $cell) + '</td>') }
            [void]$sb.AppendLine('</tr>')
        }
        continue
    }

    Add-Paragraph $trim
}

Close-List
Close-Table
[void]$sb.AppendLine('</main></body></html>')

[IO.File]::WriteAllText($htmlPath, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Get-Item $htmlPath | Select-Object FullName,Length,LastWriteTime
