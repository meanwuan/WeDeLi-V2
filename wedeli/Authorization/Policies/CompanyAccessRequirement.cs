using Microsoft.AspNetCore.Authorization;

namespace wedeli.Authorization.Policies
{
    public class CompanyAccessRequirement: IAuthorizationRequirement
    {
        public string CompanyIdRouteParam { get; }

        public CompanyAccessRequirement(string companyIdRouteParam = "companyId")
        {
            CompanyIdRouteParam = companyIdRouteParam;
        }
    }
}
