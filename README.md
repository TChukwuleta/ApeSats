# ApeSats
ApeSats is a bidding service that allows users to bid for and purchase digital arts.
This service is written in C# programming language.


### How It Works

Every user on the platform has the ability to either view all the arts that have already been created and exists in the service, or the upload his art and set it up for bid on the service.
When handling exchange of art ownership, the system acts as the escrow system to ensure that the seller receives his money (in sats) and the buyer receives the proposed art.


### User-As-Buyer

A user sees the list of arts available on the platform. The user is able to view information about different arts which can include the name/title of the art, a short description of the art as well as the price tag for the art. User can then go ahead and bid for the art.

A user cannot bid less than the art's base amount as defined by the seller. Also, if there is an existing bid, values of new bid cannot be less than the last existing bid. The moment a new bid is submitted, the system (escrow) returns the funds of the last bidder and only locks the funds of the current bidder. New bids can continue happenning till the expiration time of the art (sets by the seller).

Once the bid time expires, the winner of the bid is expected to redeem the art using the invoice that is provided by the system, else, after 24 hours the locked funds is used to redeem the art.


### User-As-Seller

A user not only has the ability to buy arts, a user can also sell arts by either uploading a new one and putting it up for sale, or rebiding an already purchased art. The user can also go ahead to set how much he wants his art to sell for, as long as it is within the price boudary (minimum and maximum) as defined by the system. User can also set the timeframe by which the art bid would run for. 

It is good to mention that every user has the ability to withdraw all or some of its earned sats on the platform.


## Requirements

-         .Net core 6 and above
-          Code editor (Preferably Visual studio)
-          A running lightning Node (LND)
-          OR Docker and Polar installed.
-          MySQL Database & SSMS installed
-          LNURL proto file

## Configuration

For the required environment file, please see the 'requirements.txt' file

The following are a list of currently available configuration options that needs to be setup in the appsettings.json file and a short explanation of what each does.

`AccountNumberPrefix`
Used to generate system account for every user. This signifies the first 3 digits of 10

`MinimumAmount`
Minimum amount (sats) that can be pegged as art value

`MaximumAmount`
Maximum amount (sats) that can be pegged as art value

`TokenConstants`
Handles JWT token authentication. 

`DefaultConnection`
Connection string to your sql database server

`cloudinary` (required)
All connection necessary to be able to upload photo on the system. The required keys include secred, cloudname as well as key

`Lightning` (required)
All connection necessary to connect to your bitcoin node, which includes macaroon path, ssl cert path, grpc host name


## Initializing the database

To initialize the database which would create the database file and all the 
necessary tables, open your package manager console and run the commands:

```
$ Add-Migration migration_name
$ Update-Database

```

Please ensure you already have your sql server setup.


## Running the application server

After installing the dependencies, configuring the application, initializing the database, you can start the application backend by 
running the command:

```
$ dotnet run
```


## API ROUTES

For the list of all available APIs for this project, please see the 'requirements.txt' file
