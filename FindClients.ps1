$Api = @{
    Uri             = 'https://localhost:7088/api/Users/GetToken'
    Method          = 'POST'
}

$Creds            = '{"username": "alice", "password" : "lshwlshw"}'
$Response = ""
$Response = Invoke-WebRequest @Api -Body $Creds -ContentType "application/json" -UseBasicParsing;
$Token = (($Response | Select-Object -Property Content).Content | ConvertFrom-Json)[0].token

$headers = @{
    Authorization="Bearer $Token"
}

$Api = @{
    Uri             = 'https://localhost:7088/api/Clients/FindClients'
    Method          = 'GET'
}

$Response = ""
$Response = Invoke-WebRequest @Api -Headers $headers -ContentType "application/json" -UseBasicParsing;
$Values = ($Response | Select-Object -Property Content).Content | ConvertFrom-Json #| ConvertTo-Json  -Depth 100
$Values

