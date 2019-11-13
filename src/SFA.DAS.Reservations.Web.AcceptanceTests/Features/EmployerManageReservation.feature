Feature: EmployerManageReservation
	In order to manage reservations
	As an employer
	I want to be able to view the reservations I have made


Scenario: Reservations showing on page
	Given I am a non levy employer
	And I have 1 Pending reservation
	And I have 1 Confirmed reservation
	And I have 1 Expired reservation
	When I view the manage reservation screen
	Then 3 reservations are displayed

Scenario: Confirmed Reservation can not be assigned an apprentice
	Given I am a non levy employer
	And I have 1 Confirmed reservation
	When I view the manage reservation screen
	Then I am not able to add an apprentice

Scenario: Expired Reservation can not be assigned an apprentice
	Given I am a non levy employer
	And I have 1 Expired reservation
	When I view the manage reservation screen
	Then I am not able to add an apprentice

Scenario: Pending Reservation can be assigned an apprentice
	Given I am a non levy employer
	And I have 1 Pending reservation
	When I view the manage reservation screen
	Then I am able to add an apprentice

Scenario: Confirmed Reservation can not be deleted
	Given I am a non levy employer
	And I have 1 Confirmed reservation
	When I view the manage reservation screen
	Then I am not able to delete the reservation

Scenario: Expired Reservation can not be deleted
	Given I am a non levy employer
	And I have 1 Expired reservation
	When I view the manage reservation screen
	Then I am not able to delete the reservation

Scenario: Pending Reservation can be deleted
	Given I am a non levy employer
	And I have 1 Pending reservation
	When I view the manage reservation screen
	Then I am able to delete the reservation