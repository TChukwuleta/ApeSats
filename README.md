# ApeSats
ApeSats is a bidding platform that allows users to pay for digital arts.
This service is written in C# programming language.

### A bidding platform that allows users to pay for digital art built with Bitcoin, Lightning (LND) and C#



## How It Works

A user registers/logs into the platform (Apesats), and the user is able to see all digital arts and the respective information associated with the digital art (which includes pricing and bid time, amongst others).


### User-As-Buyer
A user sees the list of arts available on the platform, and join bidding for that . A user can only bid more than the last bid or more than or equal to the default bid set by the user as long as no new bid has been made. A user cannot bid for a lesser value. All these should happen within the timeframe that the seller puts for bid timing. 
Once a user indicates interest in an item, the price he is bidding for is locked separately from his available balance, should in case he wins the bid, so that we don’t have any problem with insufficient funds. Also, the higher his bids increase, the more the money is deducted from his available balance.
If he loses the bid, the money is unlocked and is now available for the user to spend on the next bid.
However, if he wins the bid, an invoice address is generated; afterwards, payment is going to be made from the locked funds to the system’s node using the invoice (QR code or plain payment request). Once all these are confirmed, ownership is then transferred to the buyer.

- User-as-a-seller:
As a user, you also have the privilege to upload art on the platform, user can also set values for the art, such as prices, bid time, etc.
Once the bid time elapsed and there is a buy, the seller’s balance is increased by the value of the sold art.
If the time elapses and there was no buyer, the user can either rebid or stock it up in his draft.
At the end of the day, the user can request a payment from the system based on the user’s available balance.

- System:
The system is the only one running a node. The system generates an invoice for the user and receives payments on behalf of the sellers. However, the sellers can track their income and also their expenses, and when the seller (user) wishes to withdraw his or her funds, all he or she needs to do is to send the system an invoice so that there can be a payment.

## Requirements

-         .Net core 6 and above
-          Visual Studio
-          A running Bitcoin core node on a signet or testnet network.
-          A running lightning Node (LND)
-          OR Docker and Polar installed.
-          MySQL Database & SSMS installed
-          LNURL proto file


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

##### USERS
 - POST /api/User/create = Registers a user on the system
 - POST /api/User/login = User login
 - POST /api/User/getusersbyid/{userid} = Returns a single user given an Id
 - POST /api/User/getall/{skip}/{take}/{email} = Returns a list of users avaialble on the system


##### ARTS
 - POST /api/Art/uploadart = Upload a art to the system
 - POST /api/Art/updateart = Update properties of the art before publishing
 - POST /api/Art/publishart = Make art open to the public for bidding
 - POST /api/Art/bidforart = Allows other users to bid for published art
 - POST /api/Art/generateinvoiceforart = Generates invoice for winning bid
 - POST /api/Art/invoicelistener = Listen for invoice when generated invoice has been paid
 - GET /api/Art/gettransactionsbyid/{skip}/{take}/{userid} = Returns a list of users transactions
 - GET /api/Art/getallarts/{skip}/{take}/{userid} = Upload a art to the system
 - GET /api/Art/uploadart/{skip}/{take}/{userid} = Upload a art to the system
 - GET /api/Art/uploadart/{skip}/{take}/{userid} = Upload a art to the system
 - GET /api/Art/getbyid/{id}/{userid} = Returns a transaction by a given ID


##### TRANSACTION
- GET /api/Transaction/gettransactionsbyid/{skip}/{take}/{userid} = Returns a list of all transactions done by user
- GET /api/Transaction/getbyid/{id}/{userid} = Retrieve a particular transacion by id
- GET /api/Transaction/getallcredit/{skip}/{take}/{userid} = Returns a list of all credit transactions on the system
- GET /api/Transaction/getcredittransactions/{skip}/{take}/{accountnumber}/{userid} = Returns a list of credit transactions done by a particular user
- GET /api/Transaction/getalldebit/{skip}/{take}/{userid} = Returns a list of all debit transactions on the system
- GET /api/Transaction/getdebittransactions/{skip}/{take}/{accountnumber}/{userid} = Returns a list of debit transactions done by a particular user
