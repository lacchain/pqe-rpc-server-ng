
# IronBridge.API.NetCore

.NET Core version of the IronBridge API



### Building

#### From Ben...
Dockerfile:
```
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS="https://+:443;http://+:80"?
ENV ASPNETCORE_HTTPS_PORT=443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["IBCQC_NetCore/IBCQC_NetCore.csproj", "IBCQC_NetCore/"]
RUN dotnet restore "IBCQC_NetCore/IBCQC_NetCore.csproj"
COPY . .
WORKDIR "/src/IBCQC_NetCore"
RUN dotnet build "IBCQC_NetCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IBCQC_NetCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_Kestrel__Certificates__Default__Password="REDACTED"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="client.ironbridgeapi.com.pfx"
COPY IBCQC_NetCore/client.ironbridgeapi.com.pfx .

ENTRYPOINT ["dotnet", "IBCQC_NetCore.dll"]
```
Run command:
```shell
docker run -it --privileged -p 443:443 -p 80:80 -e ASPNETCORE_Kestrel__Certificates__Default__Password="REDACTED" -e ASPNETCORE_Kestrel__Certificates__Default__Path="client.ironbridgeapi.com.pfx" 4ba53115705b
```
i.e.
```shell
docker run -it 
           --privileged 
           -p 443:443 
           -p 80:80 
           -e ASPNETCORE_Kestrel__Certificates__Default__Password="REDACTED" 
           -e ASPNETCORE_Kestrel__Certificates__Default__Path="client.ironbridgeapi.com.pfx" 
           4ba53115705b
```
But it seems that all config items (except for -it --privileged) are in his Dockerfile (see above)

#### To Build...
Guesswork based on openssl-pqe-engine README.md...
```shell
cd /work/dev/pqe-rpc-server-ng/
docker build -f Dockerfile . -t pqe_rpc_server_ng
```
And adjusted to use Ben's Docker file (see above)...
```shell
docker build -f Dockerfile.ben . -t pqe_rpc_server_ng
```

#### To Run...
```shell
docker run -v `pwd`/output:/build/packages/ --rm pqe_rpc_server_ng
```
or...
```shell
docker run -it --privileged -v `pwd`/output:/build/packages/ --rm pqe_rpc_server_ng
```
