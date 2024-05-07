Feature: Token Integration Service Tests

API Tests for Token Integration Service.  To Test on Local use localhost in place of Azure QA Environment.

@smoke @regression @tokenintegrationservice @ignore
Scenario: Token Integration Service Post Request with Valid inputs
	Given user sends post request to 'localhost' Token Integration Service endpoint
	Then response is all ok with response code as 'OK'
	And response body contains rqp


@regression @tokenservice @tokenintegrationservice @ignore
Scenario Outline: Get Request with various invalid inputs	
	Given user sends post request to 'localhost' with headers as '<X-Request-ID>' with body as '<rqp>' for rqp '<ticket>' for ticket '<as_uri>' for as_uri
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| X-Request-ID         | rqp      | ticket  | as_uri  | StatusCode   |
	| sdfasdfasdasdadsa    | rqpNo    | idno    | idno    | OK           |
	| sdfasdfasdasdadsb    | rqpNo    | idno    | idno    | BadRequest   |
	|                      | rqpNo    | idno    | idno    | Unauthorized |
	| sdfasdfasdasdadsdxxx | rqpNo    | idno    | idno    | OK           |
	| sdfasdfasdasdadse    |          | idno    | idno    | BadRequest   |
	| sdfasdfasdasdadsf    | rqpNoxxx | idno    | idno    | BadRequest   |
	| sdfasdfasdasdadsg    | rqpNo    |         | idno    | OK           |
	| sdfasdfasdasdadsh    | rqpNo    | idnoxxx |         | BadRequest   |
	| sdfasdfasdasdadsi    | rqpNo    | idno    | idno    | OK           |
	| sdfasdfasdasdadsj    |          | idno    | idnoxxx | BadRequest   |
	| sdfasdfasdasdadsk    | rqpNoxxx | idno    |         | BadRequest   |	