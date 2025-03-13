using MhpdCommon.Utils;

namespace MhpdCommonTests.Utils
{
    public class JweValidatorTests
    {
        [Fact]
        public void IsJweFormatValid_ValidJwe_ReturnsTrue()
        {
            var validJwe = "eyJ0eXAiOiJKV1QiLCJraWQiOiJ1c2VyLWFjY291bnQtc2lnbmF0dXJlLWtleSIsImVuYyI6IkExMjhDQkMtSFMyNTYiLCJjdHkiOiJKV1QiLCJhbGciOiJSU0EtT0FFUC0yNTYifQ."
                           + "e8fhuMztCdiet-Q6uW34w-RmlHXbVxLkDwmoDCvO8Y-Fqh_DIvUltm7lRgzugZevFa8VPPOlKuFRH6iBXwFNvtXlyHdYLg1Z2nT-3YV-ylvXiTRn71SesgV_clxHehQv7083zKavWJGxkV4L02maxJC-h3QYuH3KRouf3qfCHxpaLVNRTWxQZXdSArap9Sd5DGAfXWYEy-UmHvdXZ5XLsY_1VQhx4cqwwJKyJXrLCne76tU92ZT_bcreQp2u1gccjtLWOWVMp05ESM-dFp0i5pp3D1YG4FZjUr8K92fRhl683rf9ugxSIl-WqTZ2LdK6XHVFnUJxSiNJoPHedhKr2A."
                           + "Y4adof4jIngLUFuQ5CeKhw."
                           + "kplzS7OrJcbzVRI__xEhZZxzXmKRRAH4QhkXlScKmHVa1nq0S8G1yyXRE0XWGclIQma0jcqxOHKve23MdExps5K9dKoUdCbGqOR8SVjJed9UvRysudwyTLThQ9xBRrBMwnx8gGSiNyvEij4jxWR_v2vEEtn_JmnuYTQh8eTZwJUpHj3hk_yH3eRYD3LvDrO5wtMIWVDodFI3ZEFlxk9Ja5TddIru8b_eBgaDdRsEnhW-6Zx51quYkTTf28So_8sRZS2rHxQHJlwDuNPjGbYyiLsJBTKe0e0WCoWLSyKbaNvTjx7Mxl8SHw8Fo3oYls-IqVGftl6EdBvGB-D1ImChlU5f-Ht6n9tt-lqsAJz5Vqli9ARcrTfFOXwSuEWOM1CDqjKnN4AEXd5N0X4ZR4iYt5RwLS4_o6d2m-aA3gQW2gPqj1yywcE6hvX6DBjo9Xm9n4k9pYmfJ7NQsdhQmFpluq6K3OX9BfO5nBpmeocHEcP0QfdLaeAg4cXgBh1OkGEjLXgMOzx-ElWfVnRYFyoRRDAYqBDtZNK3OuMK4rL54enKlc928tIt-Fv7IjfmhR7s."
                           + "yAKO3wvIMrrlNE1_ln_Zng";
            
            Assert.True(JweValidator.IsJweFormatValid(validJwe));
        }

        [Fact]
        public void IsJweFormatValid_InvalidJwe_WrongPartCount_ReturnsFalse()
        {
            var invalidJwe = "eyJ0eXAiOiJKV1QiLCJraWQiOiJ1c2VyLWFjY291bnQtc2lnbmF0dXJlLWtleSIsImVuYyI6IkExMjhDQkMtSFMyNTYiLCJjdHkiOiJKV1QiLCJhbGciOiJSU0EtT0FFUC0yNTYifQ."
                             + "e8fhuMztCdiet-Q6uW34w-RmlHXbVxLkDwmoDCvO8Y-Fqh_DIvUltm7lRgzugZevFa8VPPOlKuFRH6iBXwFNvtXlyHdYLg1Z2nT-3YV-ylvXiTRn71SesgV_clxHehQv7083zKavWJGxkV4L02maxJC-h3QYuH3KRouf3qfCHxpaLVNRTWxQZXdSArap9Sd5DGAfXWYEy-UmHvdXZ5XLsY_1VQhx4cqwwJKyJXrLCne76tU92ZT_bcreQp2u1gccjtLWOWVMp05ESM-dFp0i5pp3D1YG4FZjUr8K92fRhl683rf9ugxSIl-WqTZ2LdK6XHVFnUJxSiNJoPHedhKr2A";
            
            Assert.False(JweValidator.IsJweFormatValid(invalidJwe));
        }

        [Fact]
        public void IsJweFormatValid_InvalidJwe_Null_ReturnsFalse()
        {
            Assert.False(JweValidator.IsJweFormatValid(null));
        }

        [Fact]
        public void IsJweFormatValid_InvalidJwe_EmptyString_ReturnsFalse()
        {
            Assert.False(JweValidator.IsJweFormatValid(""));
        }

        [Fact]
        public void IsJweFormatValid_InvalidJwe_InvalidCharacters_ReturnsFalse()
        {
            var invalidJwe = "eyJ0eXAiOiJKV1QiLCJraWQiOiJ1c2VyLWFjY291bnQtc2lnbmF0dXJlLWtleSIsImVuYyI6IkExMjhDQkMtSFMyNTYiLCJjdHkiOiJKV1QiLCJhbGciOiJSU0EtT0FFUC0yNTYifQ."
                             + "e8fhuMztCdiet-Q6uW34w-RmlHXbVxLkDwmoDCvO8Y-Fqh_DIvUltm7lRgzugZevFa8VPPOlKuFRH6iBXwFNvtXlyHdYLg1Z2nT-3YV-ylvXiTRn71SesgV_clxHehQv7083zKavWJGxkV4L02maxJC-h3QYuH3KRouf3qfCHxpaLVNRTWxQZXdSArap9Sd5DGAfXWYEy-UmHvdXZ5XLsY_1VQhx4cqwwJKyJXrLCne76tU92ZT_bcreQp2u1gccjtLWOWVMp05ESM-dFp0i5pp3D1YG4FZjUr8K92fRhl683rf9ugxSIl-WqTZ2LdK6XHVFnUJxSiNJoPHedhKr2A."
                             + "Y4adof4jIngLUFuQ5CeKhw."
                             + "invalid_base64=="
                             + "yAKO3wvIMrrlNE1_ln_Zng";
            
            Assert.False(JweValidator.IsJweFormatValid(invalidJwe));
        }
    }
}