namespace WebApp_OpenIDConnect_DotNet.Services.Arm
{
    /// <summary>
    /// Result of Arm List tenant. We are only interested in Arm tenants
    /// </summary>
    class ArmResult
    {
        public ArmTenant[] value { get; set; }
    }

    /// <summary>
    /// In a Tenant, we are only interested in the tenant ID
    /// </summary>
    class ArmTenant
    {
        public string tenantId { get; set; }
    }
}
