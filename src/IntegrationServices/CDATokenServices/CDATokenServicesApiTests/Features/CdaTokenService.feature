Feature: CDATokenService Tests

API Tests for CDA Token Service

@smoke @regression @pei @ignore
Scenario: CDA Token Service Post Request with Valid inputs
	Given user sends post request to 'localhost' cda token service endpoint
	Then response is all ok with response code as 'OK'
	And response body contains access_token, token_type, upgraded, pct


@smoke @regression @pei @ignore
Scenario Outline: Get Request with various invalid inputs
	Given user sends post request to 'localhost'with headers as '<X-Request-ID>' with params as '<grant_type>' for grant type '<ticket>' for ticket '<claim_token_format>' for claim token format
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| X-Request-ID         | grant_type                                     | ticket                                                                                                                                                          | claim_token_format      | StatusCode   |
	| sdfasdfasdasdadsa    | urn:ietf:params:oauth:grant-type:jwt-bearer    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    | pension_dashboad_rqp    | OK           |
	|                      | urn:ietf:params:oauth:grant-type:jwt-bearer    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    | pension_dashboad_rqp    | Unauthorized |
	| sdfasdfasdasdadsbxxx | urn:ietf:params:oauth:grant-type:jwt-bearer    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    | pension_dashboad_rqp    | OK           |
	| sdfasdfasdasdadsc    |                                                | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    | pension_dashboad_rqp    | BadRequest   |
	| sdfasdfasdasdadsd    | urn:ietf:params:oauth:grant-type:jwt-bearerxxx | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    | pension_dashboad_rqp    | BadRequest   |
	| sdfasdfasdasdadse    | urn:ietf:params:oauth:grant-type:jwt-bearer    |                                                                                                                                                                 | pension_dashboad_rqp    | OK           |
	| sdfasdfasdasdadsf    | urn:ietf:params:oauth:grant-type:jwt-bearer    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.xxx | pension_dashboad_rqp    | OK           |
	| sdfasdfasdasdadsg    | urn:ietf:params:oauth:grant-type:jwt-bearer    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    |                         | OK           |
	| sdfasdfasdasdadsh    | urn:ietf:params:oauth:grant-type:jwt-bearer    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    | pension_dashboad_rqpxxx | BadRequest   |
	| sdfasdfasdasdadsa    |                                                |                                                                                                                                                                 |                         | BadRequest   |
	| sdfasdfasdasdadsa    | urn:ietf:params:oauth:grant-type:jwt-bearer    |                                                                                                                                                                 |                         | OK           |
	| sdfasdfasdasdadsa    | urn:ietf:params:oauth:grant-type:jwt-bearer    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c.    | pension_dashboad_rqp    | OK           |
