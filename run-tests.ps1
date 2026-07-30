$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $root
try {
  $env:PYTHONPATH = $root
  python -m unittest discover -s tests
} finally {
  Pop-Location
}
