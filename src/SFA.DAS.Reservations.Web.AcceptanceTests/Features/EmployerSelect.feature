Feature: EmployerSelect
	In order to select reservations
	As an employer
	I want to be able to view or create the reservations when needed

Scenario: Reservations available to select
	Given I am a non levy employer
	And I have 1 Pending reservation	
	When I view the select reservation screen
	Then 1 reservations are selectable

Scenario: Reservations not available to select
	Given I am a non levy employer	
	When I view the select reservation screen
	Then I am redirected to the create reservation page

Scenario: Employer comes through via transfer journey an ignores selection
	Given I am a non levy employer
	And I have a transfer receiver	
	When I view the select reservation screen
	Then I am redirected to the add apprentice page

Scenario: Employer is a levy payer and bypasses select journey
	Given I am a levy employer	
	When I view the select reservation screen
	Then I am redirected to the add apprentice page