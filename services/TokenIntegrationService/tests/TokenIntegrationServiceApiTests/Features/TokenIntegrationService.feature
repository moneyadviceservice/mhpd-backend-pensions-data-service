@Ignore
Feature: Token Integration Service Tests

API Tests for Token Integration Service.  To Test on Local use localhost in place of Azure QA Environment.

@smoke @regression @tokenintegrationservice
Scenario: Token Integration Service Post Request with Valid inputs
	Given user sends post request to 'Azure QA Environment' Token Integration Service endpoint
	Then response is all ok with response code as 'OK'
	And response body contains rqp


@regression @tokenservice @tokenintegrationservice
Scenario Outline: Get Request with various invalid inputs
	Given user sends post request to 'Azure QA Environment' with body as '<rqp>' for rqp '<ticket>' for ticket '<as_uri>' for as_uri
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| rqp      | ticket  | as_uri  | StatusCode          |
	| rqpNo    | idno    | idno    | OK                  |
	|          | idno    | idno    | BadRequest          |
	| rqpNoxxx | idno    | idno    | OK                  |
	| rqpNo    |         | idno    | BadRequest          |
	| rqpNo    | idnoxxx | idno    | OK                  |
	| rqpNo    | idno    | idnoxxx | InternalServerError |
	| rqpNoxxx | idno    |         | BadRequest          |