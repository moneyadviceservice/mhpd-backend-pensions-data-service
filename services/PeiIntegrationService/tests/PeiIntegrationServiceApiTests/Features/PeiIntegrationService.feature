@ignore
Feature: PeiIntegrationService Tests

API Tests for PEI Integration Service. To Test on Local use localhost in place of Azure QA Environment.

@smoke @pei
Scenario: Get Request with Valid inputs
	Given user sends get request to 'Azure QA Environment' peis endpoint
	Then response is all ok with response code as 'OK'
	And response header contains rpt
	And response body contains pei with description, retrievalStatus, retrievalRequestedTimestamp


@regression @pei
Scenario Outline: Get Request with various invalid inputs
	Given get request sent to 'Azure QA Environment' with headers as '<iss>' for iss '<userSessionId>' for sessionid '<rpt>' for authorisation with request body having '<requestId>' for request id '<peisId>' for request peisId
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| iss              | userSessionId                           | rpt                                                                                                                                   | requestId                               | peisId                                  | StatusCode |
	|                  | 29bf8cc0-b874-4725-bf3e-408486047acb    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af52    | cd0e4fdc-8586-4483-9899-17dd85af9072    | BadRequest |
	| https://maps.com |                                         | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af55    | cd0e4fdc-8586-4483-9899-17dd85af9075    | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047acfxxx | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af56    | cd0e4fdc-8586-4483-9899-17dd85af9076    | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047ag     | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af56    | cd0e4fdc-8586-4483-9899-17dd85af9076    | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047aci    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyH  | b7301d11-f166-499a-9bf1-0598c2f1af59    | cd0e4fdc-8586-4483-9899-17dd85af9079    | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047acj    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af60xxx | cd0e4fdc-8586-4483-9899-17dd85af9080    | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047ack    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI |                                         | cd0e4fdc-8586-4483-9899-17dd85af9081    | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047acl    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af62    |                                         | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047acm    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af63    | cd0e4fdc-8586-4483-9899-17dd85af9083xxx | BadRequest |
	| https://maps.com | 29bf8cc0-b874-4725-bf3e-408486047acm    | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI | b7301d11-f166-499a-9bf1-0598c2f1af63    | cd0e4fdc-8586-4483-9899-17dd85af984     | BadRequest |

@regression @pei
Scenario Outline: Get Request with various valid inputs
	Given get request sent to 'Azure QA Environment' with headers as '<iss>' for iss '<userSessionId>' for sessionid '<rpt>' for authorisation with request body having '<requestId>' for request id '<peisId>' for request peisId
	Then response is all ok with response code as '<StatusCode>'
	And response header contains rpt
	And response body contains pei with description, retrievalStatus, retrievalRequestedTimestamp

Examples:
	| iss                 | userSessionId                        | rpt                                                                                                                                      | requestId                            | peisId                               | StatusCode |
	| https://maps.com    | 29bf8cc0-b874-4725-bf3e-408486047aca | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | b7301d11-f166-499a-9bf1-0598c2f1af51 | cd0e4fdc-8586-4483-9899-17dd85af9071 | OK         |
	| https://maps.comxxx | 29bf8cc0-b874-4725-bf3e-408486047acc | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | b7301d11-f166-499a-9bf1-0598c2f1af53 | cd0e4fdc-8586-4483-9899-17dd85af9073 | OK         |
	| http://maps.co.uk   | 29bf8cc0-b874-4725-bf3e-408486047acd | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | b7301d11-f166-499a-9bf1-0598c2f1af54 | cd0e4fdc-8586-4483-9899-17dd85af9074 | OK         |
	| https://maps.com    | 6ee2ec99-70a6-4781-b2d2-da2cc75fd177 |                                                                                                                                          | b7301d11-f166-499a-9bf1-0598c2f1af52 | cd0e4fdc-8586-4483-9899-17dd85af9078 | OK         |
	| https://maps.com    | 6ee2ec99-70a6-4781-b2d2-da2cc75fd177 | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHIxxx | b7301d11-f166-499a-9bf1-0598c2f1af52 | cd0e4fdc-8586-4483-9899-17dd85af9078 | OK         |

@regression @pei
Scenario: Get Request with missing iss
	Given user sends get request to 'Azure QA Environment' peis endpoint with missing iss
	Then response is all ok with response code as 'BadRequest'

@regression @pei
Scenario: Get Request with missing userSessionId
	Given user sends get request to 'Azure QA Environment' peis endpoint with missing userSessionId
	Then response is all ok with response code as 'BadRequest'

@smoke @pei
Scenario: Get Request with missing rpt
	Given user sends get request to 'Azure QA Environment' peis endpoint with missing rpt
	Then response is all ok with response code as 'OK'

@regression @pei
Scenario: Get Request with missing requestId
	Given user sends get request to 'Azure QA Environment' peis endpoint with missing requestId
	Then response is all ok with response code as 'BadRequest'

@regression @pei
Scenario: Get Request with missing peisId
	Given user sends get request to 'Azure QA Environment' peis endpoint with missing peisId
	Then response is all ok with response code as 'BadRequest'