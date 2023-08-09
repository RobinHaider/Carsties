# Auction Service Specification

## Infrastructure

- **.Net Web API**
- **Postgres DB**
- **Entity Framework ORM**
- **Service Bus - RabbitMQ**

### Nuget Packages

- AutoMapper.Extensions.Microsoft.DependancyInjection
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore.Design
- Npgsql.EntityFrameworkCore.PostgreSQL
- MassTransit.RabbitMQ

## External (User)

- **CreateAuction**: Creates an Item. Emits AuctionCreated
- **UpdateAuction**: Updates an Auction. Emits AuctionUpdated
- **DeleteAuction**: Deletes an Auction if state allows (no bids on auction, reserve not met). Emits AuctionDeleted

### Queries handled

- **GetAuctionById**: Gets an auction by an ID. Returns AuctionDto
- **GetAuctions**: Gets all auctions. Returns list of AuctionDto
- **GetAuctionsFromDate**: Gets all auctions updated from a Date

### Events emitted

- **AuctionCreated**: When the auction is created, in response to CreateAuction - Emits AuctionDto
- **AuctionUpdated**: When the auction is updated, in response to UpdateAuction - Emits AuctionDto
- **AuctionDeleted**: When the auction is deleted, in response to DeleteAuction - Emits AuctionId

### Events consumed

- **BidService.BidPlaced**: When a bid has been placed in the BidService
- **BidService.BiddingFinished**: When an auction has reached the AuctionEnd date.

## API Endpoints

- **POST** `api/auctions` Create Item Auth
- **PUT** `api/auctions/:id` Update auction Auth
- **DELETE** `api/auctions/:id` Delete auction Auth
- **GET** `api/auctions` Get auctions Anon
- **GET** `api/auctions/:id` Get auction by id Anon

## Models

### Aucton.cs

| Property Name  | Property Type | Default Value         |
|----------------|---------------|-----------------------|
| Id             | Guid          |                       |
| ReservePrice   | int           | 0                     |
| Seller         | string        | (username from claim) |
| Winner?        | string        | (username of winner)  |
| SoldAmount?    | int           |                       |
| CurrentHighBid?| int           |                       |
| CreatedAt      | DateTime      | DateTime.UtcNow       |
| UpdatedAt      | DateTime      | DateTime.UtcNow       |
| AuctionEnd     | DateTime      |                       |
| Status         | Status        | Status.Live           |
| Item           | Item          |                       |

### Item.cs

| Property Name | Property Type | Default Value |
|---------------|---------------|---------------|
| Id            | Guid          |               |
| Make          | string        |               |
| Model         | string        |               |
| Year          | int           |               |
| Color         | string        |               |
| Mileage       | int           |               |
| ImageUrl      | string        |               |
| Auction       | Auction       | (related to Auction) |
| AuctionId     | Guid          |               |

### Status.cs (enum)

- Live
- Finished
- ReserveNotMet

### DTOs

#### AuctionDto.cs

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Id             | Guid          |               |
| CreatedAt      | DateTime      |               |
| UpdatedAt      | DateTime      |               |
| AuctionEnd     | DateTime      |               |
| Seller         | string        |               |
| Winner         | string        |               |
| Make           | string        |               |
| Model          | string        |               |
| Year           | int           |               |
| Color          | string        |               |
| Mileage        | int           |               |
| ImageUrl       | string        |               |
| Status         | string        |               |
| ReservePrice   | int           |               |
| SoldAmount?    | int           |               |
| CurrentHighBid?| int           |               |

#### CreateAuctionDto.cs

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Make           | string        |               |
| Model          | string        |               |
| Color          | string        |               |
| Mileage        | int           |               |
| Year           | int           |               |
| ReservePrice   | int           |               |
| ImageUrl       | string        |               |
| AuctionEnd     | DateTime      |               |

#### UpdateAuctionDto.cs

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Make?          | string        |               |
| Model?         | string        |               |
| Color?         | string        |               |
| Mileage?       | int           |               |
| Year?          | int           |               |

### Event Emitted Types

#### AuctionCreated

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Id             | Guid          |               |
| CreatedAt      | DateTime      |               |
| UpdatedAt      | DateTime      |               |
| AuctionEnd     | DateTime      |               |
| Seller         | string        |               |
| Winner         | string        |               |
| Make           | string        |               |
| Model          | string        |               |
| Year           | int           |               |
| Color          | string        |               |
| Mileage        | int           |               |
| ImageUrl       | string        |               |
| Status         | string        |               |
| ReservePrice   | int           |               |
| SoldAmount?    | int           |               |
| CurrentHighBid?| int           |               |

#### AuctionUpdated

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Id             | string        |               |
| Make           | string        |               |
| Model          | string        |               |
| Color          | string        |               |
| Mileage        | int           |               |
| Year           | int           |               |

#### AuctionDeleted

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Id             | string        |               |

### Event Consumed Types

#### BidService.BidPlaced

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Id             | string        |               |
| Make           | string        |               |
| Model          | string        |               |
| Color          | string        |               |
| Mileage        | int           |               |
| Year           | int           |               |

## Auction Service Specification 6

| Property Name  | Property Type | Default Value |
|----------------|---------------|---------------|
| Color          | string        |               |
| Mileage        | int           |               |
| Year           | int           |               |
