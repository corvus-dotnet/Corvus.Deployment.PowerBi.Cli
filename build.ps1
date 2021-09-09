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
# TODO: Get shared tasks published to PowerShell Gallery
Import-Module "C:\_DATA\code\vellum-cli\Endjin.RecommendedPractices.Build\Endjin.RecommendedPractices.Build.psd1"
. Endjin.RecommendedPractices.Build.tasks

# build variables
$SolutionToBuild = "Solutions/PowerBI.Cli.sln"
$SkipTests = $true

# Synopsis: Build, Test and Package
task . FullBuild

# extensibility targets
task PreBuild -Before Build
task PostBuild -After Build
task PreTest -Before Test
task PostTest -After Test
task PreTestReport -Before TestReport
task PostTestReport -After TestReport
task PrePackage -Before Package
task PostPackage -After Package