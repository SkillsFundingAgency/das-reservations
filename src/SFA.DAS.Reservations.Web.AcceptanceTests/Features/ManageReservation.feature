Feature: ManageReservation
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
