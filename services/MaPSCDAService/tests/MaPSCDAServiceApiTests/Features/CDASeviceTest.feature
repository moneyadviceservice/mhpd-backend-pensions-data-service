@ignore
Feature: CDA Service Tests

API Tests for CDA Service Service.  To Test on Local use localhost in place of Azure QA Environment.

@smoke @regression @cdaservice
Scenario: CDA Service Post Request with Valid inputs
	Given user sends post request to 'Azure QA Environment' CDA Service endpoint
	Then response is all ok with response code as 'OK'
	And response body contains rqp


@regression @tokenservice @tokenintegrationservice
Scenario Outline: Get Request with various invalid inputs
	Given user sends post request to 'Azure QA Environment' with body as '<iss>' for iss '<userSessionId>' for userSessionId
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| iss    | userSessionId | StatusCode |
	| iss    | idno          | OK         |
	|        | idno          | BadRequest |
	| issxxx | idno          | OK         |
	| issNo  |               | BadRequest |
	| issNo  | idnoxxx       | OK         |