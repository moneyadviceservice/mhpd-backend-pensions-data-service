Feature: Token Integration Service Tests

API Tests for Token Integration Service.  To Test on Local use localhost in place of Azure QA Environment.

@smoke @regression @tokenintegrationservice @ignore
Scenario: Token Integration Service Post Request with Valid inputs	
	Given user sends post request to 'Azure QA Environment' Token Integration Service endpoint
	Then response is all ok with response code as 'OK'
	And response body contains rqp


@regression @tokenservice @tokenintegrationservice @ignore
Scenario Outline: Get Request with various invalid inputs
	Given user sends post request to 'Azure QA Environment' with headers as '<X-Request-ID>' with params as '<scope>' for scope '<grant_type>' for grant type '<ticket>' for ticket '<claim_token>' for claim token '<claim_token_format>' for claim token format
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| X-Request-ID         | scope    | grant_type | ticket  | claim_token | claim_token_format      | StatusCode   |
	| sdfasdfasdasdadsa    | owner    | gt         | idno    | idno        | pension_dashboad_rqp    | OK           |
	| sdfasdfasdasdadsb    | owner    | gt         |         | idno        | pension_dashboad_rqp    | BadRequest   |
	|                      | owner    | gt         | idno    | idno        | pension_dashboad_rqp    | Unauthorized |
	| sdfasdfasdasdadsdxxx | owner    | gt         | idno    | idno        | pension_dashboad_rqp    | OK           |
	| sdfasdfasdasdadse    | owner    |            | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| sdfasdfasdasdadsf    | owner    | gtxxx      | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| sdfasdfasdasdadsg    | owner    | gt         | idnoxxx | idno        | pension_dashboad_rqp    | OK           |
	| sdfasdfasdasdadsh    | owner    | gt         | idno    |             | pension_dashboad_rqp    | BadRequest   |
	| sdfasdfasdasdadsi    | owner    | gt         | idno    | idnoxxx     | pension_dashboad_rqp    | OK           |
	| sdfasdfasdasdadsj    |          | gt         | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| sdfasdfasdasdadsk    | ownerxxx | gt         | idno    | idno        | pension_dashboad_rqp    | BadRequest   |
	| sdfasdfasdasdadsl    | owner    | gt         | idno    | idno        |                         | BadRequest   |
	| sdfasdfasdasdadsm    | owner    | gt         | idno    | idno        | pension_dashboad_rqpxxx | BadRequest   |