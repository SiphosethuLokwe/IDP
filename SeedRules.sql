-- Seed DuplicationRules table with 5 default rules

-- Rule 1: Exact ID Number Match
INSERT INTO DuplicationRules (Id, RuleName, RuleDescription, RuleJson, Priority, MinConfidenceScore, IsActive, CreatedAt, UpdatedAt) 
VALUES (NEWID(), 'Exact ID Number Match', 'Detects when two learners have identical ID numbers', 
'{"WorkflowName":"ExactIdMatch","Rules":[{"RuleName":"ExactIdMatch","Expression":"idMatch == true","RuleExpressionType":"LambdaExpression"}]}', 
100, 1.0, 1, GETDATE(), GETDATE());

-- Rule 2: Exact Name and DOB Match
INSERT INTO DuplicationRules (Id, RuleName, RuleDescription, RuleJson, Priority, MinConfidenceScore, IsActive, CreatedAt, UpdatedAt) 
VALUES (NEWID(), 'Exact Name and Date of Birth Match', 'Detects when two learners have identical first name, last name, and date of birth', 
'{"WorkflowName":"NameDobMatch","Rules":[{"RuleName":"NameDobMatch","Expression":"nameMatch == true AND dobMatch == true","RuleExpressionType":"LambdaExpression"}]}', 
90, 0.95, 1, GETDATE(), GETDATE());

-- Rule 3: Name + DOB + SETA Match
INSERT INTO DuplicationRules (Id, RuleName, RuleDescription, RuleJson, Priority, MinConfidenceScore, IsActive, CreatedAt, UpdatedAt) 
VALUES (NEWID(), 'Name, DOB, and SETA Match', 'Detects learners with identical name, birth date, and SETA code', 
'{"WorkflowName":"NameDobSetaMatch","Rules":[{"RuleName":"NameDobSetaMatch","Expression":"nameMatch == true AND dobMatch == true AND setaMatch == true","RuleExpressionType":"LambdaExpression"}]}', 
85, 0.90, 1, GETDATE(), GETDATE());

-- Rule 4: Name and Phone Match  
INSERT INTO DuplicationRules (Id, RuleName, RuleDescription, RuleJson, Priority, MinConfidenceScore, IsActive, CreatedAt, UpdatedAt) 
VALUES (NEWID(), 'Name and Phone Number Match', 'Detects learners with identical names and phone numbers', 
'{"WorkflowName":"NamePhoneMatch","Rules":[{"RuleName":"NamePhoneMatch","Expression":"nameMatch == true AND phoneMatch == true","RuleExpressionType":"LambdaExpression"}]}', 
80, 0.85, 1, GETDATE(), GETDATE());

-- Rule 5: Email Match (Different Name)
INSERT INTO DuplicationRules (Id, RuleName, RuleDescription, RuleJson, Priority, MinConfidenceScore, IsActive, CreatedAt, UpdatedAt) 
VALUES (NEWID(), 'Email Match with Different Name', 'Detects potential duplicates where email matches but names differ (possible name change or typo)', 
'{"WorkflowName":"EmailMatch","Rules":[{"RuleName":"EmailMatch","Expression":"emailMatch == true AND nameMatch == false","RuleExpressionType":"LambdaExpression"}]}', 
70, 0.75, 1, GETDATE(), GETDATE());

-- Verify
SELECT COUNT(*) AS TotalRules FROM DuplicationRules;
SELECT RuleName, Priority, MinConfidenceScore, IsActive FROM DuplicationRules ORDER BY Priority DESC;
