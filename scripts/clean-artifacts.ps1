$RepositoryRoot = Split-Path $PSScriptRoot
$ArtifactsRoot = Join-Path $RepositoryRoot 'artifacts'

Remove-Item $ArtifactsRoot -Force -Recurse -ErrorAction SilentlyContinue
Get-ChildItem $RepositoryRoot -Directory -Recurse -Filter bin | Remove-Item -Recurse
Get-ChildItem $RepositoryRoot -Directory -Recurse -Filter obj | Remove-Item -Recurse
Get-ChildItem $RepositoryRoot -Directory -Recurse -Filter app | Remove-Item -Recurse
Get-ChildItem $RepositoryRoot -Directory -Recurse -Filter nupkg | Remove-Item -Recurse
Get-ChildItem $RepositoryRoot -Directory -Recurse -Filter build | Remove-Item -Recurse
Get-ChildItem $RepositoryRoot -Directory -Recurse -Filter node_modules | Remove-Item -Recurse
