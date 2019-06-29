# Surging文档贡献指南

## docfx的安装与使用
### 1. 打开`powershell`,使用如下命令安装[Chocolatey](https://chocolatey.org/install)
```shell
Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
```

### 2. 安装`docfx`工具
```shell
choco install docfx -y
```
> **Notes**
> - 安装完`docfx`之后需要重启`powershell`,才会有`docfx`命令

### 3. `docfx`的使用
```shell
# 构建文档
docfx build

# 构建文档同时本地预览
docfx build -s

# 本地预览文档
docfx serve
```
> **Notes**
> - 更多关于`docfx`工具使用教程的请[参考](https://dotnet.github.io/docfx/tutorial/docfx.exe_user_manual.html)

## 如何成为开发者文档贡献者
我们欢迎所有有能力的开发者为surging社区做出自己的贡献,为surging的发展贡献自己的力量。您可以通过qq群或是issues向作者申请成为文档作者。我们会将文档的贡献者显示在开发者文档相关页面,对所有做出贡献的开发者表示感谢。

## 编写开发者文档

### 1. 新增文档目录
在[`./docs/toc.yml`](./toc.yml)新增文档目录，并在首页[`./docs/index.md`](./index.md)中创建相关的链接。

### 2. 编写开发者文档
使用`markdown`编写开发者文档,将文档存放到相关模块的目录下。关于文档写作规范请[参考](https://github.com/ruanyf/document-style-guide)。

### 3. 提交并发起PR
发起pr后,经作者审核通过,将会合并到主分支。

## 投稿
欢迎开发者向社区投稿关于surging或是微服务相关的博客,您可以选择同时发表到其他平台,但是必须是原创的。请将相关的博文存放在`./docs/blogs`目录下,并向社区提交`PR`。
