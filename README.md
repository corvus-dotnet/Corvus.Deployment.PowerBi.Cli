# Corvus.Deployment.PowerBi.Cli

This tool was inspired by the excellent [TabularEditor](https://github.com/TabularEditor/TabularEditor) and uses some derived code to bring a subset of its devops features to a lightweight, cross-platform CLI tool - packaged as a .NET Global Tool.

You can use Tabular Editor to develop your tabular models, taking advantage of its support for storing the model definition as a set of files that is source control friendly.  Then use this tool in your CI/CD pipeline to build a deployable artefact, from that set of files, and subsequently deploy it to a Power BI Premium workspace.

## Installation

Ensure you have the .NET 5.0 SDK installed, then run:
```
dotnet tool install -g corvus.deployment.powerbi.cli
```

## Basic Usage

### Build Scenario
```
convert
  Converts model to .bim format.

Usage:
  pbi [options] model convert <model-file-path> <bim-output-file-path>

Arguments:
  <model-file-path>       Path to the Tabular Editor 'database.json' file.
  <bim-output-file-path>  Path to the .bim output file.

Options:
  -?, -h, --help  Show help and usage information
```

```
pbi model convert C:\temp\model\database.json C:\temp\.output\model.bim
```

### Deployment Scenario
```
deploy
  Deploys a .bim formatted model to a Power BI Premium workspace.

Usage:
  pbi [options] model deploy <bim-file-path> <workspace-name> <dataset-name>

Arguments:
  <bim-file-path>   Path to the .bim file.
  <workspace-name>  Existing Power BI Premium workspace name.
  <dataset-name>    Power BI dataset name.

Options:
  -t, --tenant-id <tenant-id>  Power BI tenant ID.
  -?, -h, --help               Show help and usage information
```

```
pbi model deploy C:\temp\.output\model.bim <existing-workspace-name> <dataset-name>
```

## Deployment Automation

For automation scenarios, service principal authentication can be enabled by setting the following environment variables:

```
AZURE_CLIENT_ID
AZURE_CLIENT_SECRET
AZURE_TENANT_ID
```

>NOTE: Without these environment variables, interactive authentication will be triggered.