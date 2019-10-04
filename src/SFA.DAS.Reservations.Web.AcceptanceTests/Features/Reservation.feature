Feature: Reservation
	In order reserve funding for non levy employers
	As an employer
	I want to be able to create a reservation


Scenario: Create non levy reservation as employer
	Given I am a non levy employer
	And I have chosen a legal entity
	And I have chosen a course
	And I have a reservation start date of September
	When I review my reservation and confirm
	Then The reservation is created

Scenario: Create non levy reservation as employer but do not confirm
	Given I am a non levy employer
	And I have chosen a legal entity
	And I have chosen a course
	And I have a reservation start date of September
	When I review my reservation and I do not confirm
	Then The reservation is not created
	And redirected to employer dashboard
