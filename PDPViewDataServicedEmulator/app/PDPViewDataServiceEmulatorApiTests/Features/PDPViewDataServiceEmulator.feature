Feature: PDP View Data Service Emulator Tests

API Tests for PDP View Data Service Emulator.  To Test on Local use localhost in place of Azure QA Environment.

@smoke @PDPViewData
Scenario: PDP View Data Service Emulator GET Request with empty authorisation value
	Given user sends get request to 'Azure QA Environment' pdp view data service endpoint
	Then response is Unauthorized
	And response header contains value for WWW-Authenticate

@smoke @PDPViewData
Scenario: PDP View Data Service Emulator GET Request with missing authorisation header
	Given user sends get request to 'Azure QA Environment' pdp view data service endpoint with missing authorisation header
	Then response is Unauthorized
	And response header contains value for WWW-Authenticate