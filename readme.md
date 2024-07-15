# SWIFTAPI for Processing SWIFT MT Messages
# Web API Documentation

## Overview
This API allows processing of SWIFT MT messages.
It accepts form data files restricted to .txt.
Supports partial validation for SWIFT MT799 messages.

## SWIFT Message Format
A typical SWIFT message is structured as follows:

- **{1:} Basic Header Block**
- **{2:} Application Header Block** (Optional)
- **{3:} User Header Block** (Optional)
- **{4:} Text Block** (Optional)
- **{5:} Trailer Block** (Optional)

The extracted fields for MT799 validation are from the Text Block ({4:}).

## MT799 Message Specific Fields and Validation Rules

- **Transaction Reference Number: (Field20)** 
  - Mandatory field.
  - Up to 16 characters.
  - Cannot start or end with a slash (`/`).
  - Cannot contain two consecutive slashes (`//`).

- **Related Reference: (Field21)**
  - Optional field.
  - Up to 16 characters.

- **Narrative: (Field79)** 
  - Mandatory field.
  - A brief description of the transaction.
  - Up to 35 lines, each line up to 50 characters.

*IMPORTANT* As per these docs https://www2.swift.com/knowledgecentre/publications/us7m_20230720/2.0?topic=con_sfld_Mar8YAQQEe2AI4OK6vBjrg_-2011010220fld.htm,
field :79: may be repeated.
Current Behaviour: The API behaves as normal when one :79: field is present.
If multiple :79: fields are present:
 - TEMP WORKAROUND (NEEDS REVISION): The API concatenates them into one field, separated by "||" and returns the response as normal.


## Endpoints

### Upload MT799 Message

**POST** `/api/swiftmessage/mt799`

- **Request:** 
  - FormData with a `.txt` file containing the MT799 message.

- **Responses:**
  - `201 Created` - Returns the inserted MT799 message details.
  - `400 Bad Request` - If the file is invalid or the MT799 message is invalid.
  - `500 Internal Server Error` - For unexpected errors.

### Get MT799 Message by ID

**GET** `/api/swiftmessage/mt799/{id}`

- **Request:**
  - `id` - The ID of the MT799 message to retrieve.

- **Responses:**
  - `200 OK` - Returns the MT799 message details.
  - `404 Not Found` - If no MT799 message is found for the given ID.
  - `500 Internal Server Error` - For unexpected errors.

## Architecture & Design

The API is built using the following technologies:

- **ASP.NET Core 8** - The web framework used.
- **ADO.NET** - For database operations.
- **SQLite** - The database used for storing MT799 messages.
- **NLog** - For logging.
- **Swagger** - For API documentation.

### Models

- **SwiftMessage** - Represents a generic SWIFT message.
- **MTMessage** - An abstract base class for MT messages.
- **MT799** - Represents a SWIFT MT799 message with specific fields.

### Repository

The repository layer handles database interactions for storing and retrieving messages. It includes functionality to insert and query `SwiftMessage` and `MT799` records.

### Service

The service layer contains business logic for processing Swift messages. It depends on the repository for data access and includes operations such as message parsing and validation.

### Parser

The parser component is responsible for parsing the raw Swift message text into structured data that can be processed by the API.

### Validator

The validator ensures that Swift messages conform to required formats and standards before they are processed or stored.

### Database Initialization

The database initialization component ensures that the required database schema is created and maintained. It runs as part of the application startup process. Db file will be created on the first run at root directory.

## Error Handling and Logging

The API uses structured error handling to return informative error responses. Errors are logged using NLog, and appropriate HTTP status codes are returned. Log file will be created on the first run in `\bin\Debug\net8.0\logs\` directory.


## Setup

1. **Clone the repository**: `git clone https://github.com/your-repo/swiftapi.git`
2. **Navigate to the project directory**: `cd swiftapi`
3. **Build the project**: `dotnet build`
4. **Run the project**: `dotnet run`
5. **Access the API documentation**: `https://localhost:7066/swagger/index.html`
6. **Test the API endpoints using Swagger or a REST client like Postman**.
