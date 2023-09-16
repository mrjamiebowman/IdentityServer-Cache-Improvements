# IdentityServer: Cache Improvements (Redis)
This project demonstrages how to implement the Redis cache and improve it to be more performant. So I think...

This references a modified version of Duende IdentityServer that modifies the `ICache` interface. This modified code is located in the git-submodule but it is the `main-cache` branch.   

[https://github.com/mrjamiebowman/IdentityServer/tree/main-cache](https://github.com/mrjamiebowman/IdentityServer/tree/main-cache)   

## Packages
StackExchange.Redis   
OpenTelemetry   


## KeyDB (Redis Drop-in Replacement)
Key DB is substantially faster than Redis.   

[https://github.com/Snapchat/KeyDB](https://github.com/Snapchat/KeyDB)   

```console
helm repo add enapter https://enapter.github.io/charts/
helm install keydb enapter/keydb
```
