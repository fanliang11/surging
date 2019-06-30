# surging打包脚本

## windows系统
Windows系统下使用powershell工具通过脚本`pack.ps1`打包Surging组件。

| 参数名 | 是否必须 | 说明 | 
|:----|:-----|:-----|
| -repo | 否 | nuget仓库地址 |
| -push | 否 | 是否将surging组件推送到nuget仓库,缺省值为`false` |
| -apikey | 否 | nuget仓库apikey,如果设置了`-push $true`,必须提供`-apikey`值 |
| -build | 否 | 是否构建surging组件包,缺省值为`true` |

## Linux系统
Liunx系统下使用`pack.sh`脚本打包Surging组件。

| 参数名 | 是否必须 | 说明 | 
|:----|:-----|:-----|
| -r 或 --repo | 否 | nuget仓库地址 |
| -p 或 --push | 否 | 是否将surging组件推送到nuget仓库 |
| ---ship-build | 否 | 是否跳过构建过程 |
| -k 或 --apikey | 否 |  nuget仓库apikey,如果设置了`-p`,必须提供`--k`值 |
| -h 或 --help | 否 | 显示帮助信息 |