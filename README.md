[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-c66648af7eb3fe8bc4f294546bfd86ef473780cde1dea487d3c4ff354943c9ae.svg)](https://classroom.github.com/online_ide?assignment_repo_id=9468930&assignment_repo_type=AssignmentRepo)
# Fitogram Interview Service

This service is based on our booking system and has been stripped down for the HR interview. Please dig into the service and try to understand what it does. In the interview we will have a couple of questions around areas like Coding, Distributed Systems, Testing, Performance and more. As part of the test you will have two issues to solve (see below). Before you start developing, please create a new public repository on your GitHub account and upload the code. For each of the issues below create one Pull Request so its easier for all of us to discuss your changes together.

If you run in trouble getting the project to run, just create the Pull Requests without testing, and we discuss the issues you faced together (as it would be in normal work relationships).

## Tasks to solve upfront before the interview

We expect this to take around an hour to complete. Please don't spend 2 hours or more.

ISSUE-1: It seems the tests are failing. Please find the failing tests and fix them. If necessary improve the test to have full coverage.

ISSUE-2: The product team wishes to have a field for notes on the booking (so that the customer can add some notes when making a booking). Please add the field `Notes` (string) to the model and make sure it can be added and read via the existing bookings api.

## Development

To start all dependencies run

```bash
docker-compose up -d redis rabbitmq postgres
```

Run the tests:

```bash
docker-compose up test
```
