param(
    [switch]$Reset,
    [int]$Owners = 100,
    [int]$Props = 300,
    [int]$Images = 600,
    [int]$Traces = 3,
    [switch]$SingleMode,
    [string]$SinglePropertyName = "Propiedad Benchmark",
    [int]$SinglePropertyPrice = 9876543,
    [int]$SinglePropertyImages = 50,
    # Por defecto intenta ./.env (raíz del proyecto). Puedes sobreescribir con -EnvPath "C:\ruta\.env"
    [string]$EnvPath = "./.env"
)

function ConvertTo-JsStringLiteral {
    param([string]$Value)
    if ($null -eq $Value) { return "''" }
    $escaped = $Value.Replace('\', '\\').Replace("'", "\'")
    return "'" + $escaped + "'"
}

Write-Host "== MillionBack Seeder ==" -ForegroundColor Cyan

function Get-DotEnv {
    param([string]$Path)
    if (-not (Test-Path $Path)) { throw ".env no encontrado en $Path" }
    $dict = @{}
    Get-Content $Path | ForEach-Object {
        if ($_ -match '^[#\s]') { return }
        if ($_ -match '^(?<k>[^=]+)=(?<v>.*)$') {
            $k = $matches['k'].Trim()
            $v = $matches['v'].Trim()
            $dict[$k] = $v
        }
    }
    return $dict
}

# Resolver ruta del .env
$resolvedEnv = Resolve-Path -LiteralPath $EnvPath -ErrorAction SilentlyContinue
if ($null -eq $resolvedEnv) {
    # Intentar en el directorio padre de este script (..\.env)
    $scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
    $fallbackPath = Join-Path (Split-Path -Parent $scriptDir) '.env'
    $resolvedEnv = Resolve-Path -LiteralPath $fallbackPath -ErrorAction SilentlyContinue
}
if ($null -eq $resolvedEnv) { throw ".env no encontrado. Use -EnvPath para especificar la ruta o cree un .env en la raíz del proyecto." }

$envVars = Get-DotEnv -Path $resolvedEnv.Path
$MONGO_CONN = $envVars['MONGO_CONN']
$MONGO_DB   = $envVars['MONGO_DB']

if (-not $MONGO_CONN -or -not $MONGO_DB) {
    throw "MONGO_CONN o MONGO_DB no definidos en $($resolvedEnv.Path)"
}

# Localizar mongosh
$mongoCmd = (Get-Command mongosh -ErrorAction SilentlyContinue)
if ($null -eq $mongoCmd) {
    $commonPaths = @(
        "$Env:ProgramFiles\MongoDB\mongosh\current\bin\mongosh.exe",
        "$Env:ProgramFiles\MongoDB\Shell\mongosh.exe",
        "$Env:ProgramFiles(x86)\MongoDB\mongosh\current\bin\mongosh.exe",
        "$Env:LOCALAPPDATA\Programs\mongosh\mongosh.exe"
    )
    foreach ($p in $commonPaths) { if (Test-Path $p) { $mongoCmd = $p; break } }
}
if ($null -eq $mongoCmd) {
    throw "mongosh no está instalado o no está en PATH. Instálalo: winget install MongoDB.Shell (o descarga desde https://www.mongodb.com/try/download/shell)"
}

# Construir URI con DB (manejar query string y/o slash final correctamente)
$uri = $MONGO_CONN
$parts = $uri.Split('?', 2)
if ($parts.Length -eq 2) {
    $base = $parts[0].TrimEnd('/')
    $query = '?' + $parts[1]
    $uri = "$base/$MONGO_DB$query"
} else {
    $base = $uri.TrimEnd('/')
    $uri = "$base/$MONGO_DB"
}

$RESETVAL = if ($Reset) { 'true' } else { 'false' }

Write-Host ("Conectando a: {0}" -f $uri) -ForegroundColor Yellow
Write-Host ("RESET={0} OWNERS={1} PROPS={2} IMAGES={3} TRACES={4}" -f $RESETVAL, $Owners, $Props, $Images, $Traces) -ForegroundColor Yellow
if ($SingleMode) {
    Write-Host ("Modo propiedad única activo: Nombre='{0}' Precio={1} Imágenes={2}" -f $SinglePropertyName, $SinglePropertyPrice, $SinglePropertyImages) -ForegroundColor Yellow
}

# Ejecutar seed
$scriptPath = Join-Path $PSScriptRoot 'seed.js'
$singleModeVal = if ($SingleMode) { 'true' } else { 'false' }
$singleNameLiteral = ConvertTo-JsStringLiteral $SinglePropertyName
$evalParts = @(
    "var RESET=$RESETVAL;"
    "var OWNERS=$Owners;"
    "var PROPS=$Props;"
    "var IMAGES=$Images;"
    "var TRACES=$Traces;"
    "var SINGLE_MODE=$singleModeVal;"
    "var SINGLE_PROPERTY_NAME=$singleNameLiteral;"
    "var SINGLE_PROPERTY_PRICE=$SinglePropertyPrice;"
    "var SINGLE_PROPERTY_IMAGES=$SinglePropertyImages;"
)
$eval = ($evalParts -join ' ')
& $mongoCmd "$uri" "$scriptPath" --eval $eval

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nSeed completado ✅" -ForegroundColor Green
} else {
    Write-Host ("`nSeed falló ❌ (código {0})" -f $LASTEXITCODE) -ForegroundColor Red
    exit $LASTEXITCODE
}
