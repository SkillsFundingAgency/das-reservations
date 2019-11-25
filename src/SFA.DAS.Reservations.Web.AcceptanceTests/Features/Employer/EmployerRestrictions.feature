Feature: EmployerRestrictions
	In order to know when I cannot reserve funding
	As an employer
	I want to be notified of any restrictions
	
Scenario: Restriction in place shown to user
	Given I am a non levy employer
	And there are funding restrictions in place
	When I start the reservation journey
	Then I am shown that there are restrictions in place
	
Scenario: User is shown upcoming restriction and can dimiss
	Given I am a non levy employer
	And there are upcoming funding restrictions in place
	When I start the reservation journey
	Then I am shown that there are upcoming restrictions in place
	And I am able to dismiss them
	