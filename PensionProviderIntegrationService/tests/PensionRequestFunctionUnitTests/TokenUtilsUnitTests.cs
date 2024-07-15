using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PensionRequestFunction.TokenUtils;

namespace PensionRequestFunctionUnitTests
{
    public class TokenUtilsUnitTests
    {
        public TokenUtilsUnitTests ()
        {

        }

        [Fact]
        public void WhenRetrievedClaimCalled_AndKnownClaimPresent_ItShouldReturnClaimValue ()
        {

            var expectedViewData = "{\r\n\t\"arrangements\": [\r\n\t\t{\r\n\t\t\t\"pensionProviderSchemeName\": \"My Company Direct Contribution Scheme\",\r\n\t\t\t\"alternateSchemeName\": {\r\n\t\t\t\t\"name\": \"Converted from My Old Direct Contribution Scheme\",\r\n\t\t\t\t\"alternateNameType\": \"FOR\"\r\n\t\t\t},\r\n\t\t\t\"possibleMatch\": true,\r\n\t\t\t\"possibleMatchReference\": \"Q12345\",\r\n\t\t\t\"pensionAdministrator\": {\r\n\t\t\t\t\"name\": \"Pension Company 1\",\r\n\t\t\t\t\"contactMethods\": [\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": false,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"email\": \"example@examplemyline.com\"\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t},\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\t\"preferred\": true,\r\n\t\t\t\t\t\t\"contactMethodDetails\": {\r\n\t\t\t\t\t\t\t\"number\": \"+123 1111111111\",\r\n\t\t\t\t\t\t\t\"usage\": [\r\n\t\t\t\t\t\t\t\t\"A\",\r\n\t\t\t\t\t\t\t\t\"M\"\r\n\t\t\t\t\t\t\t]\r\n\t\t\t\t\t\t}\r\n\t\t\t\t\t}\r\n\t\t\t\t]\r\n\t\t\t}\r\n\t\t}\r\n\t]\r\n}";

            var viewDataToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIwNjk5MzY2LCJleHAiOjE3MjA3MDI5NjYsImp0aSI6IjZiNTQ1NjllLTNmN2UtNGRiMy1hNDhkLTk4YjEwMmFhMWZlMyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMDY5OTM2Nn0.NPa_p2hETl_CoIz-R2MFrWjAC2vFgwb5aUHFBooPTiA4jDOzdMEk9ZvS2KogL8TVW36OeqiJYtcMunP5yRkFykjlvr06W1L1SoJFxv__pOSNzjP5nziOVyHYEiYvIVkrm3Yv-96KlFe_wLsZ7p1ibsTI8krPF_rIt0xjvU555_sFaebCMCsaxRujzo7Gp9Ef55AxizpXi-osddzMYfbcLfe9DpNzl7Ij4pGQriEAR9R2ZZCUCwQd0n8DV9ZN4hIsOVCzHsY16b-7bnciy8GScsKLLcFeuN4LQba4uOW1AqlLXFv-8_YrJofAzIH8eej6rB4t8KBB9cvTxrRvikFHxA";
            TokenDecoder tokenUtils = new TokenDecoder();
            var viewDataValue = tokenUtils.RetrieveClaim(viewDataToken, "ViewData", out string viewData);

            Assert.Equal(expectedViewData, viewDataValue!.ToString());
        }

        [Fact]
        public void WhenRetrievedClaimCalled_AndClaimNotPresent_ItShouldReturnNull()
        {
            var viewDataToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIwNjk5MzY2LCJleHAiOjE3MjA3MDI5NjYsImp0aSI6IjZiNTQ1NjllLTNmN2UtNGRiMy1hNDhkLTk4YjEwMmFhMWZlMyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMDY5OTM2Nn0.NPa_p2hETl_CoIz-R2MFrWjAC2vFgwb5aUHFBooPTiA4jDOzdMEk9ZvS2KogL8TVW36OeqiJYtcMunP5yRkFykjlvr06W1L1SoJFxv__pOSNzjP5nziOVyHYEiYvIVkrm3Yv-96KlFe_wLsZ7p1ibsTI8krPF_rIt0xjvU555_sFaebCMCsaxRujzo7Gp9Ef55AxizpXi-osddzMYfbcLfe9DpNzl7Ij4pGQriEAR9R2ZZCUCwQd0n8DV9ZN4hIsOVCzHsY16b-7bnciy8GScsKLLcFeuN4LQba4uOW1AqlLXFv-8_YrJofAzIH8eej6rB4t8KBB9cvTxrRvikFHxA";
            TokenDecoder tokenUtils = new TokenDecoder();
            var viewDataValue = tokenUtils.RetrieveClaim(viewDataToken, "BlaBla", out string viewData);

            Assert.Null(viewDataValue);
        }

        [Fact]
        public void WhenRetrievedClaimCalled_AndClaimIsInvakid_ItShouldReturnNull()
        {
            var viewDataToken = "XXXeyJhbGciOiJSUzI1NiIsImtpZCI6ImMwMGI0MGVhLTZkYTEtNDA4YS1hNmM5LTE3YjFmZjQ1YmI5YSIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjRicWZ3MzQ4ZjlxNDM5OGgzIiwiaWF0IjoxNzIwNjk5MzY2LCJleHAiOjE3MjA3MDI5NjYsImp0aSI6IjZiNTQ1NjllLTNmN2UtNGRiMy1hNDhkLTk4YjEwMmFhMWZlMyIsImF1ZCI6Imh0dHBzOi8vcGRwL2lnL3Rva2VuIiwiaXNzIjoiREFUQV9QUk9WSURFUl8xZmQxZGE4OC05ZmIzLTQ2MWMtYTQ4YS0zZGJhMjFiZmJhMTciLCJWaWV3RGF0YSI6IntcclxuXHRcImFycmFuZ2VtZW50c1wiOiBbXHJcblx0XHR7XHJcblx0XHRcdFwicGVuc2lvblByb3ZpZGVyU2NoZW1lTmFtZVwiOiBcIk15IENvbXBhbnkgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XCJhbHRlcm5hdGVTY2hlbWVOYW1lXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJDb252ZXJ0ZWQgZnJvbSBNeSBPbGQgRGlyZWN0IENvbnRyaWJ1dGlvbiBTY2hlbWVcIixcclxuXHRcdFx0XHRcImFsdGVybmF0ZU5hbWVUeXBlXCI6IFwiRk9SXCJcclxuXHRcdFx0fSxcclxuXHRcdFx0XCJwb3NzaWJsZU1hdGNoXCI6IHRydWUsXHJcblx0XHRcdFwicG9zc2libGVNYXRjaFJlZmVyZW5jZVwiOiBcIlExMjM0NVwiLFxyXG5cdFx0XHRcInBlbnNpb25BZG1pbmlzdHJhdG9yXCI6IHtcclxuXHRcdFx0XHRcIm5hbWVcIjogXCJQZW5zaW9uIENvbXBhbnkgMVwiLFxyXG5cdFx0XHRcdFwiY29udGFjdE1ldGhvZHNcIjogW1xyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiBmYWxzZSxcclxuXHRcdFx0XHRcdFx0XCJjb250YWN0TWV0aG9kRGV0YWlsc1wiOiB7XHJcblx0XHRcdFx0XHRcdFx0XCJlbWFpbFwiOiBcImV4YW1wbGVAZXhhbXBsZW15bGluZS5jb21cIlxyXG5cdFx0XHRcdFx0XHR9XHJcblx0XHRcdFx0XHR9LFxyXG5cdFx0XHRcdFx0e1xyXG5cdFx0XHRcdFx0XHRcInByZWZlcnJlZFwiOiB0cnVlLFxyXG5cdFx0XHRcdFx0XHRcImNvbnRhY3RNZXRob2REZXRhaWxzXCI6IHtcclxuXHRcdFx0XHRcdFx0XHRcIm51bWJlclwiOiBcIisxMjMgMTExMTExMTExMVwiLFxyXG5cdFx0XHRcdFx0XHRcdFwidXNhZ2VcIjogW1xyXG5cdFx0XHRcdFx0XHRcdFx0XCJBXCIsXHJcblx0XHRcdFx0XHRcdFx0XHRcIk1cIlxyXG5cdFx0XHRcdFx0XHRcdF1cclxuXHRcdFx0XHRcdFx0fVxyXG5cdFx0XHRcdFx0fVxyXG5cdFx0XHRcdF1cclxuXHRcdFx0fVxyXG5cdFx0fVxyXG5cdF1cclxufSIsIm5iZiI6MTcyMDY5OTM2Nn0.NPa_p2hETl_CoIz-R2MFrWjAC2vFgwb5aUHFBooPTiA4jDOzdMEk9ZvS2KogL8TVW36OeqiJYtcMunP5yRkFykjlvr06W1L1SoJFxv__pOSNzjP5nziOVyHYEiYvIVkrm3Yv-96KlFe_wLsZ7p1ibsTI8krPF_rIt0xjvU555_sFaebCMCsaxRujzo7Gp9Ef55AxizpXi-osddzMYfbcLfe9DpNzl7Ij4pGQriEAR9R2ZZCUCwQd0n8DV9ZN4hIsOVCzHsY16b-7bnciy8GScsKLLcFeuN4LQba4uOW1AqlLXFv-8_YrJofAzIH8eej6rB4t8KBB9cvTxrRvikFHxA";
            TokenDecoder tokenUtils = new TokenDecoder();
            var viewDataValue = tokenUtils.RetrieveClaim(viewDataToken, "ViewData", out string viewData);

            Assert.Null(viewDataValue);
        }

    }
}
