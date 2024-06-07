Feature: CDA Pei API Test Without RPT Authorization
API Tests with RPT Authorization as per policy on Service Emulator. To Test on Local use localhost in place of Azure QA Environment.

@smoke @cdapei
Scenario: Get Request without RPT & Valid inputs	
	Given user sends request to 'Azure QA Environment' endpoint 'without' RPT authorization
	Then response is all ok with response code as 'Unauthorized'


@regression @cdapei
Scenario Outline: Get Request with various invalid inputs	
	Given user sends request to 'Azure QA Environment' endpoint 'without' RPT authorization as per '<scope>' with request as '<X-Request-ID>' and version as '<X-Version>' with guid as '<GUID>'
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| scope          | X-Request-ID        | X-Version | GUID                                 | StatusCode   |
	| uma_protection | 1111-2222-3333-4444 | 1.0       | 0cbe2fcf-4332-4018-a42b-ad2488a810b6 | Unauthorized |
	| owner          | 2222-2222-3333-4444 | 1.0       | 0cbe2fcf-4332-4018-a42b-ad2488a810b6 | Unauthorized |
	| Invalid        | 3333-2222-3333-4444 | 1.0       | 0cbe2fcf-4332-4018-a42b-ad2488a810b6 | Unauthorized |
	|                | 0000-2222-3333-4444 | 1.0       | 11111111-1111-1111-1111-111111111111 | Unauthorized |
	| uma_protection | 4444-2222-3333-4444 | 1.0       | 0cbe2fcf-4332-4018-a42b-ad2488a810b6 | Unauthorized |
	| owner          | 5555-2222-3333-4444 | 1.0       | 11111111-1111-1111-1111-111111111111 | Unauthorized |
	| uma_protection | 6666-2222-3333-4444 | 1.0       | !"£$%^&-*()-2345-6789-012345678901   | Unauthorized |
	| owner          | 7777-2222-3333-4444 | 1         | 0cbe2fcf-4332-4018-a42b-ad2488a810b6 | Unauthorized |
	| uma_protection | 8888-2222-3333-4444 | 1.0       |                                      | NotFound     |
	| owner          | 9999-2222-3333-4444 |           | 0cbe2fcf-4332-4018-a42b-ad2488a810b6 | Unauthorized |