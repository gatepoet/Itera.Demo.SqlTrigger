ALTER TRIGGER OnFishInserted ON [Fish] FOR INSERT
AS
BEGIN 
    BEGIN TRANSACTION;
        DECLARE @ch UNIQUEIDENTIFIER
        DECLARE @messageBody NVARCHAR(MAX);

        BEGIN DIALOG CONVERSATION @ch
                FROM SERVICE [InitiatorService]
                TO SERVICE 'TargetService'
                ON CONTRACT [http://blog.maskalik.com/Contract]
                WITH ENCRYPTION = OFF;

        -- Construct the request message
        SET @messageBody = (SELECT [Id], [TypeId], [Count] FROM INSERTED FOR XML AUTO, ELEMENTS);

        -- Send the message to the TargetService
        ;SEND ON CONVERSATION @ch
        MESSAGE TYPE [http://blog.maskalik.com/RequestMessage] (@messageBody);
    COMMIT;
END 
GO