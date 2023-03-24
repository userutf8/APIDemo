$Api = @{
    Uri             = 'https://localhost:7088/api/Users/GetToken'
    Method          = 'POST'
}

$Creds            = '{"username": "admin", "password" : "lshwlshw"}'
$Response = ""
$Response = Invoke-WebRequest @Api -Body $Creds -ContentType "application/json" -UseBasicParsing;
$Token = (($Response | Select-Object -Property Content).Content | ConvertFrom-Json)[0].token

$headers = @{
    Authorization="Bearer $Token"
}

$Api = @{
    Uri             = 'https://localhost:7088/api/Clients/UpdateClientStatus'
    Method          = 'PUT'
}

$Body            = '{"id": "ae37ba74-8ad8-4bd1-adfa-ab0fe89a5444", "status": 1}'
$Response = ""
$Response = Invoke-WebRequest @Api -Headers $headers -Body $Body -ContentType "application/json; charset=UTF-8"
$Values = ($Response | Select-Object -Property Content).Content | ConvertFrom-Json #| ConvertTo-Json  -Depth 100
$Values