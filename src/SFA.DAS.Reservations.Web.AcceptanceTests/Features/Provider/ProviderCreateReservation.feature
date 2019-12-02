Feature: Provider Create Reservation
	In order reserve funding for non levy employers
	As a provider
	I want to be able to create a reservation
	
#Background:
#	Given a provider web app is running

Scenario: Create non levy reservation as provider
	Given a provider web app is running
	When I choose an employer's account legal entity
	And I choose a course and date of July on behalf of an employer
	And I review a reservation on behalf of an employer
	Then The reservation is created on behalf of an employer

Scenario: Create non levy reservation as employer but do not confirm
	Given a provider web app is running
	When I choose an employer's account legal entity
	#And I have chosen a course
	#And I have a reservation start date of September
	#When I review my reservation and I do not confirm
	#Then The reservation is not created
	#And redirected to employer dashboard

Scenario: Reservation limit reached for non levy employer
	#Given I am a provider
	#And I have reached my reservation limit
	#When I start the reservation journey
	#Then I am shown a message saying I have reached my reservation limit

Scenario: Reservation limit reached for non levy employer when starting reservation
	#Given I am a provider
	#And I have reached my reservation limit
	#And I have chosen a legal entity
	#Then The reservation is not created
	#Then I am shown a message saying I have reached my reservation limit

Scenario: Course search validation no training selected
	#Given I am a provider
	#And I have chosen a legal entity
	#When I do not select any training
	#Then I am shown a validation message on the SelectCourse page

Scenario: Training date validation no date selected
	#Given I am a provider
	#And I have chosen a legal entity
	#And I have chosen a course
	#When I do not choose a start date
	#Then I am shown a validation message on the ApprenticeshipTraining page
