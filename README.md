# QShare

Local network file sharing tool built with .NET Core

> Receiver doesn't have to install qshare or any other 3rd party application (just a web browser).

## Features
+ Receive files via web browser
+ QR Code scanning (coming soon)
+ Upload files via browser (coming soon)

## Requirements
+ .NET 6 and later

## Quickstart

### Installing

#### 1. Clone the repo
```bash
git clone https://github.com/AhmedKabbary/QShare QShare
cd QShare
```

#### 2. Pack the project
```bash
cd src
dotnet pack
```

#### 3. Install the tool
```bash
dotnet tool install --global --add-source ./nupkg Qabbary.QShare
```

### Updating

> Change the current working directory to the cloned repo directory before updating.

#### 1. Pull the latest changes
```bash
git pull
```

#### 2. Pack the project
```bash
cd src
dotnet pack
```

#### 3. Update the tool
```bash
dotnet tool update --global --add-source ./nupkg Qabbary.QShare
```

### Uninstalling

```bash
dotnet tool uninstall --global Qabbary.QShare
```

> Don't forget to remove the cloned repo directory.

## Usage

+ Connect the other device to the same local network.
+ Execute the qshare command with the appropriate arguments.
+ Open the web browser on the other device and enter the url appeared in the terminal.

> Soon you will be able to scan a QR code instead of typing the url manually.

### Arguments
| Argument | Description |
| -------- | ----------- |
| ```-?```, ```-h```, ```--help``` | Show help and usage information |
| ```-i```, ```--include``` | Specifies files to include in sharing |
| ```-e```, ```--exclude``` | Specifies files to exclude from sharing |
| ```-p```, ```--port``` | Specifies http server port number |
| ```--version``` | Show version information |

## Examples

Share all files in top-level directory
```bash
qshare
```

Share a single file
```bash
qshare -i book.pdf
```

Share multiple files
```bash
qshare -i book1.pdf book2.pdf
```

Share all files with extension '.pdf'
```bash
qshare -i *.pdf
```

Share all files with extension '.pdf' except 'book.pdf'
```bash
qshare -i *.pdf -e book.pdf
```

Share all files with extension '.css' in the directory 'styles/'.
```bash
qshare -i styles/*.css
```

Share all files in any subdirectory
```bash
qshare -i **/*
```

## License
QShare is licensed under the MIT license.
