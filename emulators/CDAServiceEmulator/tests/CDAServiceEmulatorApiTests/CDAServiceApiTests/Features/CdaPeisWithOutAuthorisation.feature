@Ignore
Feature: CDA Pei API Test Without RPT Authorization
API Tests with RPT Authorization as per policy on Service Emulator. To Test on Local use localhost in place of Azure QA Environment.

@smoke @cdapei
Scenario: Get Request without RPT & Valid inputs
	Given user sends request to 'Azure QA Environment' endpoint 'without' RPT authorization
	Then response is all ok with response code as 'Unauthorized'


@regression @cdapei
Scenario Outline: Get Request without RPT with various invalid inputs
	Given user sends request to 'Azure QA Environment' endpoint 'with' RPT authorization with request as '<X-Request-ID>' with peisid as '<PEISID>'
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| X-Request-ID                          | PEISID                                | StatusCode |
	| b7301d11-f166-499a-9bf1-0598c2f1af51x | 0cbe2fcf-4332-4018-a42b-ad2488a810b6  | BadRequest |
	| b7301d11-f166-499a-9bf1-0598c2f1af2   | 0cbe2fcf-4332-4018-a42b-ad2488a810b6  | BadRequest |
	|                                       | 0cbe2fcf-4332-4018-a42b-ad2488a810b6  | BadRequest |
	| b7301d11-f166-499a-9bf1-0598c2f1af54  |                                       | NotFound   |
	| b7301d11-f166-499a-9bf1-0598c2f1af54  | 11111111-1111-1111-1111-1111111111112 | BadRequest |
	| b7301d11-f166-499a-9bf1-0598c2f1af55  | 0cbe2fcf-4332-4018-a42b-ad2488a810    | BadRequest |
	| b7301d11-f166-499a-9bf1-0598c2f1af57  | !"£$%^&-*()-2345-6789-012345678901    | BadRequest |