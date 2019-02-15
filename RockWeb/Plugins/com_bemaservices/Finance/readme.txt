TELLER IMPORT

Overview
------------------------------------------------------------------------------------------------------------
The Teller Import block provides a way of importing the gifts included in a Teller XML file. Each gift in 
the Teller file will contain the following information:

Gift Date
Batch Type
Envelope Number
Envelope Image (front/back)
Check Image (front/back)
Bank Routing Number
Bank Account Number
Total Amount
Batch/Sequence Number
Purposes (id and amount) 

Block Configuration
------------------------------------------------------------------------------------------------------------
Before importing any files, the block should be configured correctly for your environment. It includes the 
following block settings

Account Match Type: Typically the ID of each purpose on a transaction in the Teller file will coorespond
	to an account id in Rock. If that is the case, the value of this field should be set to 'Id'. If 
	however, the purpose id's are for a previous ChMS the id will need to match against the ForeignId field 
	in Rock. In this case select 'ForeignId' for this setting.

Batch Detail Page: The page that user should be redirected to after they have successfully imported a 
	Teller file so they can view the details of the batch that was updated or created.

Batch Names: When importing a Teller file, the user will have the option of importing the transactions into
	an existing 'Open' batch (all open batches will be listed in a dropdown) or to create a new batch. If 
	they opt to create a new batch, they will then have the option of selecting the name of the new batch
	from a list of pre-configured names. These names need to be configured as a new defined type with values.
	This block setting is used to specify the defined type that contains the batch name. 

In addition to the block settings, a person attribute can also be created for matching people based on an
envelope number. If a person attribute exists with the key of 'GivingEnvelopeNumber' the import will use 
that attibute to associate a transaction to an existing person.

Importing Transactions
------------------------------------------------------------------------------------------------------------
To import a Teller file, user will select the XML file and then the batch that transactions should be added
to. This can be any existing 'Open' batch or a new batch. If selecting a new batch, the batch name will be
the same as the imported file name unless a pre-configured batch name was selected, and the batch status
will be 'Pending'. The import will then create a new financial transaction for each gift in the Teller file. 

Person Matching:
When trying to associate that transaction with an existing person in Rock, the import will check the 
following...

For Checks: 
The Gift record in the Teller file contains an encrypted routing id and account number. These
values will be unencrypted (using hard-coded encryption keys) and if only one existing person in Rock has the
same Bank Account number that person will be associated to the imported transaction (the matching process 
in Rock saves this association each time a person is selected for a scanned or imported check.)

For Envelopes: 
The Gift record in the Teller file contains an envelope number. If a person attribute has been
created in Rock with a key of 'GivingEnvelopeNumber' the import will look for anyone with a value for that 
attribute with the same number. The first person found with that same value will be associated to the 
imported transaction.

Account Matching:
Each gift in the Teller file includes one or more 'purposes.' Each purpose includes an id and an amount. 
When importing the transaction, it will evaluate the id of each of the purposes and try to find an 
account in Rock that matches that Id. If the 'Account Match Type' block setting was set to 'Id' it will 
look for an account with the same Id. If the setting was set to 'ForeignId' it will look for an account
with the same ForeignId. A Transaction Detail record is then added in Rock for each purpose. If the 
import is unable to fine a matching account, it will not save the current transaction and will report
and error.

Once the import is done, a summary of the import results will be displayed and the user will be able 
to navigate to the batch that transactions were imported to and begin the matching process.