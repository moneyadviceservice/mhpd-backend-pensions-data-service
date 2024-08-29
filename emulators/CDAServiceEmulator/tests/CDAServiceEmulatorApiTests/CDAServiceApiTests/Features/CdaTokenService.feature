@Ignore
Feature: CDATokenService Tests

API Tests for CDA Token Service.  To Test on Local use localhost in place of Azure QA Environment.

@smoke @regression @tokenservice
Scenario: CDA Token Service Post Request with Valid inputs
	Given user sends post request to 'localhost' cda token service endpoint
	Then response from token service is all ok with response code as 'OK'
	And response body contains access_token, token_type, upgraded, pct


@regression @tokenservice
Scenario Outline: Get Request with various invalid inputs
	Given user sends post request to 'localhost' with headers as '<X-Request-ID>' with params as '<scope>' for scope '<grant_type>' for grant type '<ticket>' for ticket '<claim_token>' for claim token '<claim_token_format>' for claim token format
	Then response from token service is all ok with response code as '<StatusCode>'

Examples:
	| X-Request-ID         | scope    | grant_type | ticket  | claim_token | claim_token_format      | StatusCode   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gt         | idno    | idno        | pension_dashboad_rqp    | OK           |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gt         |         | idno        | pension_dashboad_rqp    | BadRequest   |
	|                      | owner    | gt         | idno    | idno        | pension_dashboad_rqp    | Unauthorized |
	| b7301d11-f166-499a-9bf1-0598c2f1af52 | owner    | gt         | idno    | idno        | pension_dashboad_rqp    | OK           |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    |            | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gtxxx      | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gt         | idnoxxx | idno        | pension_dashboad_rqp    | OK           |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gt         | idno    |             | pension_dashboad_rqp    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gt         | idno    | idnoxxx     | pension_dashboad_rqp    | OK           |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    |          | gt         | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | ownerxxx | gt         | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gt         | idno    | idno        |                         | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52    | owner    | gt         | idno    | idno        | pension_dashboad_rqpxxx | BadRequest   |
		