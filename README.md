# IdentityServer: Cache Improvements (Redis/KeyDB)
This project demonstrages how to implement the Redis Cache and improve it to be more performant on Microsoft SQL Server.   

This references a modified version of Duende IdentityServer that modifies the `ICache` interface. This modified code is located in the git-submodule but it is the `main-cache` branch.   

[https://github.com/mrjamiebowman/IdentityServer/tree/main-cache](https://github.com/mrjamiebowman/IdentityServer/tree/main-cache)   

Main Diff: ICache.cs, DefaultCache.cs, CachingResourceStore.cs
[https://github.com/DuendeSoftware/IdentityServer/compare/main...mrjamiebowman:IdentityServer:main-cache#diff-b7b9e013788429a969d9b1732dc92be3215bd86d82697304cbd87f73f826ec1f](https://github.com/DuendeSoftware/IdentityServer/compare/main...mrjamiebowman:IdentityServer:main-cache#diff-b7b9e013788429a969d9b1732dc92be3215bd86d82697304cbd87f73f826ec1f)   

src/IdentityServer/Services/ICache.cs   
src/IdentityServer/Services/Default/DefaultCache.cs   
src/IdentityServer/Stores/Caching/CachingResourceStore.cs   

## Git Clone / Submodules
`git clone --recurse-submodules git@github.com:mrjamiebowman/IdentityServer-Cache-Improvements.git`   
`git submodule update --init --recursive`   


## Packages
StackExchange.Redis   
Microsoft.Extensions.Caching.StackExchangeRedis   
OpenTelemetry   

## KeyDB (Redis Drop-in Replacement)
KeyDB is substantially faster than Redis.   

[https://github.com/Snapchat/KeyDB](https://github.com/Snapchat/KeyDB)   

```console
helm repo add enapter https://enapter.github.io/charts/
helm install keydb enapter/keydb
```
