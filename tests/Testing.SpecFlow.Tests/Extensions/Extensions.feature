Feature: Extensions
In order to improve productivity
As a developer
I want to use some extensions

    @object-resolver
    Scenario Outline: Create class instance, with object property
        When I create instance from
          | Id | Value   |
          | 1  | <value> |
        Then value should be of type <type>

        Examples:
          | value | type    |
          | 1.25  | Double  |
          | 10    | Double  |
          | true  | Boolean |
          | other | String  |

    @object-resolver
    Scenario: Create class set, with object property
        When I create set from
          | Id | Value |
          | 1  | 1.25  |
          | 2  | other |
        Then value should not be null
