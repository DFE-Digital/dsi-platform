Get-ChildItem -Path templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/*.received.txt | ForEach-Object {
    $newName = $_.Name -replace '\.received\.txt$', '.verified.txt'
    $destination = Join-Path -Path $_.DirectoryName -ChildPath $newName
    Move-Item -Path $_.FullName -Destination $destination -Force
}
