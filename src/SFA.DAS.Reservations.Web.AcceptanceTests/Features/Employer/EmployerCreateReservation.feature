Feature: Employer Create Reservation
	In order reserve funding for non levy employers
	As an employer
	I want to be able to create a reservation


Scenario: Create non levy reservation as employer
	Given I am a non levy employer
	And an employer web app is running
	And I have chosen a legal entity
	And I have chosen a course
	And I have a reservation start date of September
	When I review my reservation and confirm
	Then The reservation is created

Scenario: Create non levy reservation as employer but do not confirm
	Given I am a non levy employer
	And an employer web app is running
	And I have chosen a legal entity
	And I have chosen a course
	And I have a reservation start date of September
	When I review my reservation and I do not confirm
	Then The reservation is not created
	And redirected to employer dashboard

Scenario: Reservation limit reached for non levy employer
	Given I am a non levy employer
	And an employer web app is running
	And I have reached my reservation limit
	When I start the reservation journey
	Then I am shown a message saying I have reached my reservation limit

Scenario: Reservation limit reached for non levy employer when starting reservation
	Given I am a non levy employer
	And an employer web app is running
	And I have reached my reservation limit
	And I have chosen a legal entity
	Then The reservation is not created
	Then I am shown a message saying I have reached my reservation limit

Scenario: Course search validation no training selected
	Given I am a non levy employer
	And an employer web app is running
	And I have chosen a legal entity
	When I do not select any training
	Then I am shown a validation message on the SelectCourse page

Scenario: Training date validation no date selected
	Given I am a non levy employer
	And an employer web app is running
	And I have chosen a legal entity
	And I have chosen a course
	When I do not choose a start date
	Then I am shown a validation message on the ApprenticeshipTraining page
