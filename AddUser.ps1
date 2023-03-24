$Api = @{
    Uri             = 'https://localhost:7088/api/Users/AddUser'
    Method          = 'POST'
}

$Creds            = '{"username": "Bob", "password" : "lshwlshw", "email" : "barfoo@foo"}'
$Response = ""
$Response = Invoke-WebRequest @Api -Body $Creds -ContentType "application/json" -UseBasicParsing;