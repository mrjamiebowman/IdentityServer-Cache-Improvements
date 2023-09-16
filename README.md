# IdentityServer: Cache Improvements (Redis)
This project demonstrages how to implement the Redis cache and improve it to be more performant. So I think...

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
