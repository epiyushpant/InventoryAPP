$files = Get-ChildItem -Path "Data" -Filter "*.cs"
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $content = $content -replace 'Microsoft\.Data\.SqlClient', 'Npgsql'
    $content = $content -replace 'SqlParameter', 'NpgsqlParameter'
    $content = $content -replace 'System\.Data\.SqlDbType\.Int', 'NpgsqlTypes.NpgsqlDbType.Integer'
    $content = $content -replace 'SqlDbType =', 'NpgsqlDbType ='
    Set-Content $file.FullName -Value $content
}
