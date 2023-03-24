$Api = @{
    Uri             = 'https://localhost:7088/api/Users/GetToken'
    Method          = 'POST'
}

$Creds            = '{"username": "bob", "password" : "lshwlshw"}'
$Response = ""
$Response = Invoke-WebRequest @Api -Body $Creds -ContentType "application/json" -UseBasicParsing;
$Token = (($Response | Select-Object -Property Content).Content | ConvertFrom-Json)[0].token

$headers = @{
    Authorization="Bearer $Token"
}

$Api = @{
    Uri             = 'https://localhost:7088/api/Clients/DeleteClient/Федор Двинятин'
    Method          = 'DELETE'
}

$Response = ""
$Response = Invoke-WebRequest @Api -Headers $headers -ContentType "application/json; charset=UTF-8"
$Values = ($Response | Select-Object -Property Content).Content | ConvertFrom-Json #| ConvertTo-Json  -Depth 100
$Values