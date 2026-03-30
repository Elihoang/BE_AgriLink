Get-ChildItem -Path "AgriLink_DH.Core\Repositories\*.cs" | ForEach-Object {
    if ($_.Name -ne "BaseRepository.cs" -and $_.Name -ne "UnitOfWork.cs") {
        $content = Get-Content $_.FullName -Raw
        $content = $content -replace "namespace AgriLink_DH\.Core\.Repositories", "namespace AgriLink_DH.Infrastructure.Repositories"
        $content = $content -replace "using AgriLink_DH\.Core\.Configurations", "using AgriLink_DH.Infrastructure.Data"
        $target = "AgriLink_DH.Infrastructure\Repositories\$($_.Name)"
        $content | Set-Content $target
    }
}
