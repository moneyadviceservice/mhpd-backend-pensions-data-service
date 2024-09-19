@Ignore
Feature: Cda Emulator Holder Name Configurations
API Tests for CDA Service Emulator for holder name configurations.  To Test on Local use localhost in place of Azure QA Environment.

@smoke @holdernameconfigurations
Scenario: Get Request with Valid inputs
	Given user sends GET request to 'Azure QA Environment' endpoint for holder name configurations of cda service emulator
	Then response is all ok with response code as 'OK'
	And response body contains holder_configurations with expected values

@regression @holdernameconfigurations
Scenario Outline: Get Request with various invalid inputs
	Given user sends Get request to 'Azure QA Environment' endpoint as per '<X-Request-ID>'
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| X-Request-ID                            | StatusCode |
	| b7301d11-f166-499a-9bf1-0598c2f1af54    | OK         |
	|                                         | BadRequest |
	| b7301d11-f166-499a-9bf1-0598c2f1af54xxx | OK         |

@regression @holdernameconfigurations
Scenario: Get Request with missing inputs
	Given user sends Get request to 'Azure QA Environment' endpoint with missing X-Request-ID
	Then response is all ok with response code as 'BadRequest'