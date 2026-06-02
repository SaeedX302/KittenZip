# RenameProject.ps1
# Automates the refactoring of NanaZip to KittenZip

$ErrorActionPreference = "Stop"

# 1. Path definitions
$repoRoot = $PSScriptRoot
$sourcePng = "C:\Users\Admin\Downloads\Documents\Untitled design (2).png"
$assetsFolder = Join-Path $repoRoot "Assets"
$targetPng = Join-Path $assetsFolder "NanaZip.png"
$targetIco = Join-Path $assetsFolder "NanaZip.ico"

Write-Output "Step 1: Copying and converting custom branding icon..."

# Check if custom PNG exists
if (Test-Path $sourcePng) {
    # Copy PNG to Assets\NanaZip.png
    Copy-Item -Path $sourcePng -Destination $targetPng -Force
    Write-Output "Copied custom PNG to $targetPng"
    
    # Convert PNG to ICO
    try {
        Add-Type -AssemblyName System.Drawing
        $bmp = New-Object System.Drawing.Bitmap($targetPng)
        $hIcon = $bmp.GetHicon()
        $icon = [System.Drawing.Icon]::FromHandle($hIcon)
        $fs = New-Object System.IO.FileStream($targetIco, [System.IO.FileMode]::Create)
        $icon.Save($fs)
        $fs.Close()
        $bmp.Dispose()
        Write-Output "Successfully converted PNG to ICO at $targetIco"
    } catch {
        Write-Warning "Could not convert PNG to ICO automatically: $_"
    }
} else {
    Write-Warning "Custom PNG logo not found at $sourcePng. Skipping icon replacement."
}

Write-Output "`nStep 2: Performing case-preserving string replacements inside files..."

# Text files search filters
$textExtensions = @(
    "*.slnx", "*.vcxproj", "*.filters", "*.props", "*.h", "*.cpp", "*.cs",
    "*.xml", "*.appxmanifest", "*.txt", "*.md", "*.cmd", "*.proj", "*.iss",
    "*.def", "*.manifest", "*.ini", "*.rc", "*.resw", "*.wapproj",
    "*.yml", "*.yaml", "*.admx", "*.adml", "*.html", "*.mjs", "*.astro",
    "*.idl", "*.xaml", "*.csproj", "*.c", "*.json", "*.js", "*.ts"
)

# Find all files matching text extensions
$filesToRefactor = Get-ChildItem -Path $repoRoot -Recurse -File | Where-Object {
    $file = $_
    $matched = $false
    foreach ($ext in $textExtensions) {
        if ($file.Name -like $ext) {
            $matched = $true
            break
        }
    }
    # Exclude files in .git folder
    $matched -and ($file.FullName -notlike "*\.git\*")
}

Write-Output "Found $($filesToRefactor.Count) files to scan for replacements."

foreach ($file in $filesToRefactor) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    if ($content -match "nanazip") {
        # Perform replacements preserving case
        $content = $content -replace "NanaZip", "KittenZip"
        $content = $content -replace "nanazip", "kittenzip"
        $content = $content -replace "NANAZIP", "KITTENZIP"
        
        [System.IO.File]::WriteAllText($file.FullName, $content)
        Write-Output "Updated content in: $($file.FullName)"
    }
}

Write-Output "`nStep 3: Renaming files on disk..."

$filesToRename = Get-ChildItem -Path $repoRoot -Recurse -File | Where-Object {
    ($_.Name -like "*nanazip*") -and ($_.FullName -notlike "*\.git\*")
}

Write-Output "Found $($filesToRename.Count) files to rename."

foreach ($file in $filesToRename) {
    $newName = $file.Name -replace "NanaZip", "KittenZip" -replace "nanazip", "kittenzip"
    $newPath = Join-Path $file.DirectoryName $newName
    if (Test-Path $newPath) {
        Remove-Item -Path $newPath -Force
    }
    Rename-Item -Path $file.FullName -NewName $newName -Force
    Write-Output "Renamed file: $($file.Name) -> $newName"
}

Write-Output "`nStep 4: Renaming directories on disk (bottom-up)..."

# Find directories containing nanazip, sorted by path length descending (bottom-up)
$dirsToRename = Get-ChildItem -Path $repoRoot -Recurse -Directory | Where-Object {
    ($_.Name -like "*nanazip*") -and ($_.FullName -notlike "*\.git\*")
} | Sort-Object { $_.FullName.Length } -Descending

Write-Output "Found $($dirsToRename.Count) directories to rename."

foreach ($dir in $dirsToRename) {
    # Check if the directory still exists (in case parent rename affected it, but bottom-up prevents this)
    if (Test-Path $dir.FullName) {
        $newName = $dir.Name -replace "NanaZip", "KittenZip" -replace "nanazip", "kittenzip"
        $newPath = Join-Path $dir.Parent.FullName $newName
        if (Test-Path $newPath) {
            Remove-Item -Path $newPath -Recurse -Force
        }
        Rename-Item -Path $dir.FullName -NewName $newName -Force
        Write-Output "Renamed directory: $($dir.Name) -> $newName"
    }
}

Write-Output "`nRefactoring complete! The project is now named KittenZip."
