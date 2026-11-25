param([string]$BaseUrl = "http://localhost:5080")

$errorCount = 0
$suffix = Get-Date -Format "HHmmssfff"
$user1 = "sv01_$suffix"
$user2 = "sv02_$suffix"

function Invoke-Api {
    param([string]$Method, [string]$Path, [string]$Body, [string]$Token)
    $params = @{
        Uri = $BaseUrl + $Path
        Method = $Method
        ContentType = "application/json"
        UseBasicParsing = $true
        TimeoutSec = 30
    }
    if ($Body) { $params.Body = $Body }
    if ($Token) { $params.Headers = @{ Authorization = "Bearer $Token" } }
    try {
        $response = Invoke-WebRequest @params
        return @{ Code = [int]$response.StatusCode; Body = $response.Content }
    } catch {
        $response = $_.Exception.Response
        if ($null -eq $response) { return @{ Code = 0; Body = $_.Exception.Message } }
        $code = [int]$response.StatusCode
        $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
        $text = $reader.ReadToEnd()
        $reader.Close()
        return @{ Code = $code; Body = $text }
    }
}

function Check {
    param([string]$Id, [string]$Label, $Response, [int]$Expected, [string]$Contains = "")
    $ok = ($Response.Code -eq $Expected) -and (-not $Contains -or $Response.Body -like "*$Contains*")
    if ($ok) { Write-Output "PASS $Id $Label ($($Response.Code))" }
    else {
        $script:errorCount++
        Write-Output "FAIL $Id $Label expected=$Expected actual=$($Response.Code) body=$($Response.Body)"
    }
}

Write-Output "== ASM1 API smoke test =="

$r = Invoke-Api "POST" "/api/auth/register" ("{`"username`":`"$user1`",`"password`":`"123456`"}") ""
Check "TC01" "register" $r 201
$id1 = 0
if ($r.Body) { try { $id1 = ($r.Body | ConvertFrom-Json).id } catch {} }

$r = Invoke-Api "POST" "/api/auth/register" ("{`"username`":`"$user1`",`"password`":`"123456`"}") ""
Check "TC02" "duplicate username" $r 409

$r = Invoke-Api "POST" "/api/auth/register" ("{`"username`":`"${user1}_x`",`"password`":`"123`"}") ""
Check "TC03" "short password" $r 400 "Password"

$r = Invoke-Api "POST" "/api/auth/login" ("{`"username`":`"$user1`",`"password`":`"wrongpass`"}") ""
Check "TC04" "wrong password" $r 401

$r = Invoke-Api "POST" "/api/auth/login" ("{`"username`":`"$user1`",`"password`":`"123456`"}") ""
Check "TC05" "login" $r 200
$token1 = ""
if ($r.Body) { try { $token1 = ($r.Body | ConvertFrom-Json).token } catch {} }
if (-not $token1) { Write-Output "FAIL TC05 missing token"; $errorCount++ }

$r = Invoke-Api "POST" "/api/characters" "{`"name`":`"No Token`",`"class`":`"Warrior`"}" ""
Check "TC06" "no token" $r 401

$r = Invoke-Api "GET" "/swagger/v1/swagger.json" "" ""
Check "TC07" "swagger Bearer definition" $r 200 "Bearer"

$r = Invoke-Api "POST" "/api/characters" "{`"name`":`"Hiep Si Rong`",`"class`":`"Warrior`"}" $token1
Check "TC08" "create character" $r 201
$charId = 0
if ($r.Body) { try { $c = $r.Body | ConvertFrom-Json; $charId = $c.id; if ($c.level -ne 1 -or $c.hp -ne 100) { Write-Output "FAIL TC08 defaults level=$($c.level) hp=$($c.hp)"; $errorCount++ } } catch {} }

$r = Invoke-Api "POST" "/api/characters" "{`"name`":`"`",`"class`":`"Warrior`"}" $token1
Check "TC09" "empty name" $r 400 "Name"

$r = Invoke-Api "PUT" "/api/characters/$charId" "{`"name`":`"Hiep Si Rong`",`"class`":`"Paladin`",`"level`":5,`"hp`":250}" $token1
Check "TC10" "update own character" $r 200
if ($r.Body) { try { $c = $r.Body | ConvertFrom-Json; if ($c.level -ne 5 -or $c.hp -ne 250) { Write-Output "FAIL TC10 values level=$($c.level) hp=$($c.hp)"; $errorCount++ } } catch {} }

$r = Invoke-Api "PUT" "/api/characters/$charId" "{`"name`":`"Bad Level`",`"class`":`"Warrior`",`"level`":0,`"hp`":100}" $token1
Check "TC11" "level out of range" $r 400 "Level"

$r = Invoke-Api "PUT" "/api/characters/$charId" "{`"name`":`"No Hp`",`"class`":`"Warrior`",`"level`":5}" $token1
Check "EXTRA" "missing hp is rejected" $r 400 "Hp"

$r = Invoke-Api "POST" "/api/auth/register" ("{`"username`":`"$user2`",`"password`":`"123456`"}") ""
Check "SETUP" "register second user" $r 201
$r = Invoke-Api "POST" "/api/auth/login" ("{`"username`":`"$user2`",`"password`":`"123456`"}") ""
$token2 = ""
if ($r.Body) { try { $token2 = ($r.Body | ConvertFrom-Json).token } catch {} }
$r = Invoke-Api "POST" "/api/characters" "{`"name`":`"Other Player`",`"class`":`"Mage`"}" $token2
$otherCharId = 0
if ($r.Body) { try { $otherCharId = ($r.Body | ConvertFrom-Json).id } catch {} }
$r = Invoke-Api "PUT" "/api/characters/$otherCharId" "{`"name`":`"Stolen`",`"class`":`"Mage`",`"level`":5,`"hp`":250}" $token1
Check "TC12" "cannot edit other user" $r 404

$mysql = "C:\Program Files\MySQL\MySQL Server 26.10\bin\mysql.exe"
if (Test-Path $mysql) {
    $hashCount = & $mysql -u root -p123456 -N -B -e "SELECT COUNT(*) FROM game_asm1_demo.accounts WHERE username='$user1' AND PasswordHash LIKE 'AQAAAA%';" 2>$null
    if ([string]$hashCount -eq "1") { Write-Output "PASS DB01 password hashed" }
    else { Write-Output "FAIL DB01 password hash check ($hashCount)"; $errorCount++ }
    $rowCount = & $mysql -u root -p123456 -N -B -e "SELECT COUNT(*) FROM game_asm1_demo.characters WHERE name='Hiep Si Rong' AND level=5 AND hp=250;" 2>$null
    if ([string]$rowCount -ge "1") { Write-Output "PASS DB02 character updated" }
    else { Write-Output "FAIL DB02 character row check ($rowCount)"; $errorCount++ }
}

Write-Output "== RESULT errors=$errorCount =="
if ($errorCount -gt 0) { exit 1 }
exit 0
