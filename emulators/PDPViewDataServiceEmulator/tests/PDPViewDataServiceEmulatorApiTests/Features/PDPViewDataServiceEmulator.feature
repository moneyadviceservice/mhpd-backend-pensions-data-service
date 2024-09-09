@ignore
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

@smoke @PDPViewData
Scenario: PDP View Data Service Emulator GET Request with valid inputs
	Given user sends get request to 'Azure QA Environment' pdp view data service endpoint with valid inputs
	Then response is all ok with response code as 'OK'
	And response body contains view_data_token

@regression @PDPViewData
Scenario Outline: Get Request with various invalid inputs
	Given user sends post request to 'Azure QA Environment' pdp view data service endpoint to '<AssetGuid>' with '<scope>' as params'<X-Request-ID>' as headers '<Authorization>' as bearer token
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| AssetGuid                               | scope    | X-Request-ID                            | Authorization                                                                                                                            | StatusCode   |
	| b7301d11-f166-499a-9bf1-0598c2f1af51    | owner    | 1ba03e25-659a-43b8-ae77-b956df168969    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | OK           |
	|                                         | owner    | 1ba03e25-659a-43b8-ae77-b956df168962    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af52xxx | owner    | 1ba03e25-659a-43b8-ae77-b956df168963    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af53    |          | 1ba03e25-659a-43b8-ae77-b956df168964    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af54    | ownerxxx | 1ba03e25-659a-43b8-ae77-b956df168965    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af55    | owner    |                                         | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af56    | owner    | 1ba03e25-659a-43b8-ae77-b956df168967xxx | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | BadRequest   |
	| b7301d11-f166-499a-9bf1-0598c2f1af57    | owner    | 1ba03e25-659a-43b8-ae77-b956df168968    |                                                                                                                                          | Unauthorized |
	| b7301d11-f166-499a-9bf1-0598c2f1af58    | owner    | 1ba03e25-659a-43b8-ae77-b956df168969    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHIxxx | OK           |


	@regression @PDPViewData
Scenario: PDP View Data Service Emulator GET Request with missing AssetGuid
	Given user sends get request to 'Azure QA Environment' pdp view data service endpoint with missing asset guid
	Then response is all ok with response code as 'BadRequest'	

	@regression @PDPViewData
Scenario: PDP View Data Service Emulator GET Request with missing scope
	Given user sends get request to 'Azure QA Environment' pdp view data service endpoint with missing scope
	Then response is all ok with response code as 'BadRequest'	

	@regression @PDPViewData
Scenario: PDP View Data Service Emulator GET Request with missing x-request-id
	Given user sends get request to 'Azure QA Environment' pdp view data service endpoint with missing x-request-id
	Then response is all ok with response code as 'BadRequest'	

	@regression @PDPViewData
Scenario: PDP View Data Service Emulator GET Request with missing Authorization
	Given user sends get request to 'Azure QA Environment' pdp view data service endpoint with missing Authorization
	Then response is all ok with response code as 'Unauthorized'	