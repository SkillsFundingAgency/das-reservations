Feature: Index
	In order to create a reservation
	As an employer
	I want to view information about reservations before starting the process

@pressStart
Scenario: press start button
	Given The employer is viewing the employer Index page
	When The employer presses start
	Then Navigate the employer to the page where they select the legal entity

@pressLink
Scenario: Given the employer is viewing 
	Given The employer is viewing the employer Index page
	When The employer presses the apprenticeship training link
	Then The employer is redirected to Find Apprenticeship Training via a new tab