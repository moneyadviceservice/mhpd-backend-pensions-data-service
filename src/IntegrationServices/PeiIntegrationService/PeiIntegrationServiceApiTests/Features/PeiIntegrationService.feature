Feature: PeiIntegrationService Tests

API Tests for PEI Integration Service. To Test on Local use localhost in place of Azure QA Environment.

@smoke @regression @pei
Scenario: Get Request with Valid inputs
	Given user sends get request to 'Azure QA Environment' peis endpoint
	Then response is all ok with response code as 'OK'
	And response header contains rpt
	And response body contains pei with description, retrievalStatus, retrievalRequestedTimestamp


@smoke @regression @pei
Scenario Outline: Get Request with various invalid inputs
	Given get request sent to 'Azure QA Environment' with headers as '<cdaUserGuid>' for guid '<iss>' for iss '<userSessionId>' for sessionid '<rpt>' for authorisation with params as '<scope>' request body having '<requestId>' for request id '<peiBaseUrl>' for request url
	Then response is all ok with response code as '<StatusCode>'

Examples:
	| cdaUserGuid                             | iss                 | userSessionId              | rpt                                                                                                                                      | scope          | requestId     | peiBaseUrl                                      | StatusCode          |
	| cd0e4fdc-8586-4483-9899-17dd85af9071    | https://maps.com    | askdj902139012ekasdlasda   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | OK                  |
	| cd0e4fdc-8586-4483-9899-17dd85af9072    | https://maps.com    | askdj902139012ekasdlasdb   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.netxxx | InternalServerError |
	| 11111111-1111-1111-1111-111111111173    | https://maps.com    | askdj902139012ekasdlasdc   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    |                                                 | BadRequest          |
	| 11111111-1111-1111-1111-111111111174    | https://maps.com    | askdj902139012ekasdlasdd   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoipxxx | https://cdaserviceemulator.azurewebsites.net    | OK                  |
	| cd0e4fdc-8586-4483-9899-17dd85af9075    | https://maps.com    | askdj902139012ekasdlasde   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          |               | https://cdaserviceemulator.azurewebsites.net    | BadRequest          |
	| cd0e4fdc-8586-4483-9899-17dd85af9076    | https://maps.com    | askdj902139012ekasdlasda   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    |                | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | BadRequest          |
	| cd0e4fdc-8586-4483-9899-17dd85af9077    | https://maps.com    | askdj902139012ekasdlasda   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | ownerxxx       | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | BadRequest          |
	| cd0e4fdc-8586-4483-9899-17dd85af9078    | https://maps.com    | askdj902139012ekasdlasda   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | uma_protection | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | BadRequest          |
	| 11111111-1111-1111-1111-111111111179    | https://maps.com    | askdj902139012ekasdlasdf   |                                                                                                                                          | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | Unauthorized        |
	| cd0e4fdc-8586-4483-9899-17dd85af9080    | https://maps.com    | askdj902139012ekasdlasdm   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHIxxx | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | OK                  |
	| cd0e4fdc-8586-4483-9899-17dd85af9081    | https://maps.com    |                            | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | Unauthorized        |
	| cd0e4fdc-8586-4483-9899-17dd85af9082    | https://maps.com    | askdj902139012ekasdlasdxxx | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | OK                  |
	| cd0e4fdc-8586-4483-9899-17dd85af9083    |                     | askdj902139012ekasdlasdi   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | Unauthorized        |
	| cd0e4fdc-8586-4483-9899-17dd85af9084    | https://maps.comxxx | askdj902139012ekasdlasdh   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | OK                  |
	|                                         | https://maps.com    | askdj902139012ekasdlasdj   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | Unauthorized        |
	| cd0e4fdc-8586-4483-9899-17dd85af9086xxx | https://maps.com    | askdj902139012ekasdlasdk   | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dnZWRJbkFzIjoiYWRtaW4iLCJpYXQiOjE0MjI3Nzk2Mzh9.gzSraSYS8EXBxLN_oWnFSRgCzcmJmMjLiuyu5CSpyHI    | owner          | qwertyuoip    | https://cdaserviceemulator.azurewebsites.net    | Unauthorized        |