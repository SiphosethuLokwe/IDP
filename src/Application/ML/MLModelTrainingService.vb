Imports Microsoft.ML
Imports Microsoft.ML.Data
Imports System
Imports System.IO
Imports System.Threading.Tasks
Imports IDP.Domain.Entities

Namespace IDP.Application.ML

    ''' <summary>
    ''' ML.NET model training service
    ''' Trains a binary classification model to predict if two learners are duplicates
    ''' Uses: FastTree (decision tree ensemble) - excellent for tabular data
    ''' </summary>
    Public Class MLModelTrainingService

        Private ReadOnly _mlContext As MLContext
        Private ReadOnly _modelPath As String
        Private _trainedModel As ITransformer

        Public Sub New(Optional modelPath As String = "MLModels/DuplicateDetectionModel.zip")
            _mlContext = New MLContext(seed:=0) ' Seed for reproducibility
            _modelPath = modelPath
            
            ' Create directory if it doesn't exist
            Dim directory = System.IO.Path.GetDirectoryName(_modelPath)
            If Not String.IsNullOrEmpty(directory) Then
                If Not System.IO.Directory.Exists(directory) Then
                    System.IO.Directory.CreateDirectory(directory)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Trains the ML model using historical duplicate data
        ''' Training data should include both confirmed duplicates and confirmed non-duplicates
        ''' </summary>
        Public Function TrainModel(trainingData As List(Of LearnerComparisonInput)) As ModelMetrics
            Console.WriteLine($"Training ML model with {trainingData.Count} samples...")
            
            ' Load data into IDataView
            Dim data = _mlContext.Data.LoadFromEnumerable(trainingData)
            
            ' Split data: 80% training, 20% testing
            Dim trainTestSplit = _mlContext.Data.TrainTestSplit(data, testFraction:=0.2)
            
            ' Define the training pipeline
            Dim pipeline = _mlContext.Transforms.Concatenate("Features",
                    "IdNumberSimilarity",
                    "FirstNameSimilarity",
                    "LastNameSimilarity",
                    "PhoneticNameMatch",
                    "DobMatch",
                    "PhoneSimilarity",
                    "EmailSimilarity",
                    "SameSetaCode",
                    "HasActiveContract",
                    "ContractOverlap") _
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName:="IsDuplicate",
                    featureColumnName:="Features",
                    numberOfLeaves:=20,
                    numberOfTrees:=100,
                    minimumExampleCountPerLeaf:=10,
                    learningRate:=0.2))

            ' Train the model
            Console.WriteLine("Training in progress...")
            _trainedModel = pipeline.Fit(trainTestSplit.TrainSet)
            
            ' Evaluate the model on test set
            Dim predictions = _trainedModel.Transform(trainTestSplit.TestSet)
            Dim metrics = _mlContext.BinaryClassification.Evaluate(predictions, labelColumnName:="IsDuplicate")
            
            ' Save the model
            _mlContext.Model.Save(_trainedModel, data.Schema, _modelPath)
            Console.WriteLine($"Model saved to: {_modelPath}")
            
            ' Return metrics
            Dim modelMetrics = New ModelMetrics() With {
                .Accuracy = metrics.Accuracy,
                .Precision = metrics.PositivePrecision,
                .Recall = metrics.PositiveRecall,
                .F1Score = metrics.F1Score,
                .AucRoc = metrics.AreaUnderRocCurve,
                .PositivePrecision = metrics.PositivePrecision,
                .PositiveRecall = metrics.PositiveRecall,
                .NegativePrecision = metrics.NegativePrecision,
                .NegativeRecall = metrics.NegativeRecall,
                .ConfusionMatrix = metrics.ConfusionMatrix.GetFormattedConfusionTable(),
                .TrainingDate = DateTime.UtcNow,
                .DatasetSize = trainingData.Count
            }
            
            PrintMetrics(modelMetrics)
            
            Return modelMetrics
        End Function

        ''' <summary>
        ''' Generates synthetic training data for initial model training
        ''' In production, replace this with real historical duplicate/non-duplicate data
        ''' </summary>
        Public Function GenerateSyntheticTrainingData() As List(Of LearnerComparisonInput)
            Dim data = New List(Of LearnerComparisonInput)()
            Dim rand = New Random(42)
            
            ' Generate 1000 samples of duplicates (positive examples)
            For i = 1 To 1000
                data.Add(New LearnerComparisonInput() With {
                    .IdNumberSimilarity = CSng(0.9 + rand.NextDouble() * 0.1), ' 0.9-1.0
                    .FirstNameSimilarity = CSng(0.85 + rand.NextDouble() * 0.15),
                    .LastNameSimilarity = CSng(0.9 + rand.NextDouble() * 0.1),
                    .PhoneticNameMatch = CSng(0.9 + rand.NextDouble() * 0.1),
                    .DobMatch = 1.0F,
                    .PhoneSimilarity = CSng(0.7 + rand.NextDouble() * 0.3),
                    .EmailSimilarity = CSng(0.8 + rand.NextDouble() * 0.2),
                    .SameSetaCode = If(rand.NextDouble() > 0.3, 1.0F, 0.0F), ' 70% same SETA
                    .HasActiveContract = If(rand.NextDouble() > 0.5, 1.0F, 0.0F),
                    .ContractOverlap = If(rand.NextDouble() > 0.4, 1.0F, 0.0F),
                    .IsDuplicate = True
                })
            Next
            
            ' Generate 1000 samples of non-duplicates (negative examples)
            For i = 1 To 1000
                data.Add(New LearnerComparisonInput() With {
                    .IdNumberSimilarity = CSng(rand.NextDouble() * 0.5), ' 0.0-0.5
                    .FirstNameSimilarity = CSng(rand.NextDouble() * 0.6),
                    .LastNameSimilarity = CSng(rand.NextDouble() * 0.6),
                    .PhoneticNameMatch = CSng(rand.NextDouble() * 0.5),
                    .DobMatch = If(rand.NextDouble() > 0.8, 1.0F, 0.0F), ' Rarely match
                    .PhoneSimilarity = CSng(rand.NextDouble() * 0.4),
                    .EmailSimilarity = CSng(rand.NextDouble() * 0.4),
                    .SameSetaCode = If(rand.NextDouble() > 0.7, 1.0F, 0.0F), ' 30% same SETA
                    .HasActiveContract = If(rand.NextDouble() > 0.5, 1.0F, 0.0F),
                    .ContractOverlap = 0.0F,
                    .IsDuplicate = False
                })
            Next
            
            ' Shuffle the data
            Return data.OrderBy(Function(x) rand.Next()).ToList()
        End Function

        ''' <summary>
        ''' Loads a previously trained model from disk
        ''' </summary>
        Public Function LoadModel() As Boolean
            Try
                If Not File.Exists(_modelPath) Then
                    Console.WriteLine($"Model file not found: {_modelPath}")
                    Return False
                End If
                
                _trainedModel = _mlContext.Model.Load(_modelPath, Nothing)
                Console.WriteLine($"Model loaded from: {_modelPath}")
                Return True
            Catch ex As Exception
                Console.WriteLine($"Error loading model: {ex.Message}")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Gets the trained model for prediction
        ''' </summary>
        Public Function GetTrainedModel() As ITransformer
            Return _trainedModel
        End Function

        Private Sub PrintMetrics(metrics As ModelMetrics)
            Console.WriteLine("===============================================")
            Console.WriteLine("ML MODEL TRAINING METRICS")
            Console.WriteLine("===============================================")
            Console.WriteLine($"Accuracy:          {metrics.Accuracy:P2}")
            Console.WriteLine($"Precision:         {metrics.Precision:P2}")
            Console.WriteLine($"Recall:            {metrics.Recall:P2}")
            Console.WriteLine($"F1 Score:          {metrics.F1Score:P2}")
            Console.WriteLine($"AUC-ROC:           {metrics.AucRoc:P2}")
            Console.WriteLine($"Dataset Size:      {metrics.DatasetSize} samples")
            Console.WriteLine($"Training Date:     {metrics.TrainingDate:yyyy-MM-dd HH:mm:ss}")
            Console.WriteLine("===============================================")
            Console.WriteLine()
            Console.WriteLine("CONFUSION MATRIX:")
            Console.WriteLine(metrics.ConfusionMatrix)
            Console.WriteLine("===============================================")
        End Sub

        ''' <summary>
        ''' Retrains model with new confirmed duplicate/non-duplicate examples
        ''' This enables continuous learning from admin decisions
        ''' </summary>
        Public Function RetrainModelWithNewData(
            existingData As List(Of LearnerComparisonInput),
            newConfirmedDuplicates As List(Of LearnerComparisonInput),
            newConfirmedNonDuplicates As List(Of LearnerComparisonInput)) As ModelMetrics
            
            ' Combine all data
            Dim allData = New List(Of LearnerComparisonInput)()
            allData.AddRange(existingData)
            allData.AddRange(newConfirmedDuplicates)
            allData.AddRange(newConfirmedNonDuplicates)
            
            Console.WriteLine($"Retraining model with {newConfirmedDuplicates.Count} new duplicates and {newConfirmedNonDuplicates.Count} new non-duplicates")
            
            Return TrainModel(allData)
        End Function

    End Class

End Namespace
