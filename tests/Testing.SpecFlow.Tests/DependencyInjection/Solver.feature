Feature: Solver
In order to test correctly
As a developer
I want to use same providers

@hooks-dependency-solver
Scenario: Merge hooks
    Given a sentence "Hi"
    When I solve hooks
    Then the result should be "Hi"
