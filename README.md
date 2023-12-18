# .NET SDK for Lucidtech AI Services

## Documentation

[Link to docs](https://docs.lucidtech.ai/reference/dotnet/latest)


Create documents by using doxygen.
Download the latest and greatest version of [doxygen](https://github.com/doxygen/doxygen.git).
```bash
cd Lucidtech
doxygen # your docs will be put in the folder named html

```

## Installation

### NuGet

```bash
nuget install Lucidtech.Las
```

## Usage

### Preconditions

- Documents must be in upright position
- Only one receipt or invoice per document is supported
- Supported file formats are: jpeg, pdf
- Necessary keys and credentials to an endpoint on the form: "https://<your prefix>.api.lucidtech.ai/<version>".

### Quick start
See [docs](https://docs.lucidtech.ai/getting-started/dev/net).

## Contributing

### Prerequisites
Download the following packages: 
* The latest and greatest stable version of [MSBuild](https://github.com/Microsoft/msbuild)
* The latest and greatest version of [NuGet](https://github.com/NuGet/Home)
* [nunit](http://nunit.org/download/) version 3.9.0 or higher to run tests from command line
* [.NET-SDK](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/install) version 2.2.105

### Prerequisites for Arch Users
Download the following packages: 
* The latest and greatest stable version of [MSBuild](https://aur.archlinux.org/packages/mono-msbuild-git)
* The latest and greatest version of [NuGet](https://aur.archlinux.org/packages/nuget3/)
* [nunit](https://aur.archlinux.org/packages/nunit3-console/) version 3.9.0 or higher to run tests from command line
* [.NET-SDK](https://wiki.archlinux.org/title/.NET) version 7.0.113


### Build and run tests
Clone repo and install the necessary packages manually for the las-sdk-net.
```bash
git clone git@github.com:LucidtechAI/las-sdk-net.git
cd las-sdk-net
make prism-start
make test
# Build for release and make nuget package
msbuild Lucidtech/Lucidtech.csproj /t:Rebuild /p:Configuration=Release
```

Hint: Set environment variable CREDENTIALS=FROM_FILE to run a test against the real API with your default credentials.
