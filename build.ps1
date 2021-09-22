[CmdletBinding()]
param (
    [Parameter(Position=0)]
    [string[]] $Tasks = @("."),

    [Parameter()]
    [string] $Configuration = "Release",

    [Parameter()]
    [string] $BuildRepositoryUri = "",

    [Parameter()]
    [string] $SourcesDir = $PWD,

    [Parameter()]
    [string] $CoverageDir = "_codeCoverage",

    [Parameter()]
    [string] $TestReportTypes = "Cobertura",

    [Parameter()]
    [string] $PackagesDir = "_packages",

    [Parameter()]
    [string] $LogLevel = "minimal",

    [Parameter()]
    [switch] $Clean
)

$ErrorActionPreference = 'Stop'
$InformationPreference = $InformationAction ? $InformationAction : 'Continue'

$here = Split-Path -Parent $PSCommandPath

#region InvokeBuild setup
if (!(Get-Module -ListAvailable InvokeBuild)) {
    Install-Module InvokeBuild -RequiredVersion 5.7.1 -Scope CurrentUser -Force -Repository PSGallery
}
Import-Module InvokeBuild
# This handles calling the build engine when this file is run like a normal PowerShell script
# (i.e. avoids the need to have another script to setup the InvokeBuild environment and issue the 'Invoke-Build' command )
if ($MyInvocation.ScriptName -notlike '*Invoke-Build.ps1') {
    try {
        Invoke-Build $Tasks $MyInvocation.MyCommand.Path @PSBoundParameters
    }
    catch {
        $_.ScriptStackTrace
        throw
    }
    return
}
#endregion

# Import shared tasks and initialise build framework
if (!(Get-Module -ListAvailable Endjin.RecommendedPractices.Build)) {
    Write-Information "Installing 'Endjin.RecommendedPractices.Build' module..."
    Install-Module Endjin.RecommendedPractices.Build -RequiredVersion 0.1.0-beta0001 -AllowPrerelease -Scope CurrentUser -Force -Repository PSGallery
}
Import-Module Endjin.RecommendedPractices.Build -Force
. Endjin.RecommendedPractices.Build.tasks


# build variables
$SolutionToBuild = "Solutions/Corvus.Deployment.PowerBi.Cli.sln"
$SkipTests = $true
$CleanBuild = $true

# Synopsis: Build, Test and Package
task . FullBuild

# extensibility tasks
task PreBuild {}
task PostBuild {}
task PreTest {}
task PostTest {}
task PreTestReport {}
task PostTestReport {}
task PrePackage {}
task PostPackage {}