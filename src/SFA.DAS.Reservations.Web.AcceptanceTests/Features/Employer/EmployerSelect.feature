Feature: EmployerSelect
	In order to add an apprentice to an existing cohort
	As an employer
	I want to be able to select existing or create reservations to assign to an apprentice

Scenario: Reservations available to select
	Given I am a non levy employer
	And an employer web app is running
	And I have 1 Pending reservation	
	When I view the select reservation screen
	Then 1 reservations are selectable

Scenario: Reservations not available to select
	Given I am a non levy employer
	And an employer web app is running
	When I view the select reservation screen
	Then I am redirected to the create reservation page

Scenario: Employer comes through via transfer journey and ignores selection
	Given I am a non levy employer
	And an employer web app is running
	And I have a transfer receiver	
	When I view the select reservation screen
	Then I am redirected to the add apprentice page

Scenario: Employer is a levy payer and bypasses select journey
	Given I am a levy employer
	And an employer web app is running
	When I view the select reservation screen
	Then I am redirected to the add apprentice page

Scenario: Employer is levy with no cohort and can select a reservation
	Given I am a levy employer
	And an employer web app is running
	And I have 1 Pending reservation	
	And I have no cohort reference
	When I view the select reservation screen
	Then I am redirected to the add apprentice page with no cohort ref

Scenario: Employer comes through via transfer journey with no cohort ref and ignores selection
	Given I am a non levy employer
	And an employer web app is running
	And I have a transfer receiver	
	And I have no cohort reference
	When I view the select reservation screen
	Then I am redirected to the add apprentice page with no cohort ref
