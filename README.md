# Corvus.Deployment.PowerBi.Cli

This tool was inspired by the excellent [TabularEditor](https://github.com/TabularEditor/TabularEditor) and uses some derived code to bring a subset of its devops features to a lightweight, cross-platform CLI tool - packaged as a .NET Global Tool.

Usage

```
model convert C:\temp\model\database.json C:\temp\.output\model.bim
```

```
model deploy C:\temp\.output\model.bim <powerbi-connection-string> <powerbi-dataset-name>
```
