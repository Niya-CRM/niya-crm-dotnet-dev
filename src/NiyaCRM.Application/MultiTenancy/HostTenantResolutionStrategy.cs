using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NiyaCRM.Application.MultiTenancy;

public class HostTenantResolutionStrategy : ITenantResolutionStrategy
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _systemAdminHost;

    public HostTenantResolutionStrategy(IHttpContextAccessor httpContextAccessor, string systemAdminHost = "admin.niyacrm.com")
    {
        _httpContextAccessor = httpContextAccessor;
        _systemAdminHost = systemAdminHost;
    }

    public string? GetTenantIdentifier()
    {
        var host = _httpContextAccessor.HttpContext?.Request.Host.Value;
        if (string.IsNullOrEmpty(host))
            return null;
        var hostWithoutPort = host.Split(':')[0];
        if (hostWithoutPort.Equals(_systemAdminHost, StringComparison.OrdinalIgnoreCase))
            return null;
        return hostWithoutPort;
    }
}
