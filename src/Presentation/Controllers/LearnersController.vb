Imports System
Imports System.Threading.Tasks
Imports Microsoft.AspNetCore.Mvc
Imports IDP.Application.DTOs
Imports IDP.Application.Services

Namespace Controllers
    <ApiController>
    <Route("api/[controller]")>
    Public Class LearnersController
        Inherits ControllerBase
        
        Private ReadOnly _learnerService As LearnerService
        
        Public Sub New(learnerService As LearnerService)
            _learnerService = learnerService
        End Sub
        
        <HttpGet>
        Public Async Function GetAll() As Task(Of ActionResult(Of List(Of LearnerDto)))
            Try
                Dim learners = Await _learnerService.GetAllAsync()
                Return Ok(learners)
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpGet("{id}")>
        Public Async Function GetById(id As Guid) As Task(Of ActionResult(Of LearnerDto))
            Try
                Dim learner = Await _learnerService.GetByIdAsync(id)
                If learner Is Nothing Then
                    Return NotFound()
                End If
                Return Ok(learner)
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpPost>
        Public Async Function Create(<FromBody> dto As CreateLearnerDto) As Task(Of ActionResult(Of LearnerDto))
            Try
                If Not ModelState.IsValid Then
                    Return BadRequest(ModelState)
                End If
                
                Dim created = Await _learnerService.CreateAsync(dto)
                Return CreatedAtAction(NameOf(GetById), New With {.id = created.Id}, created)
            Catch ex As InvalidOperationException
                Return BadRequest(New With {.message = ex.Message})
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpPut("{id}")>
        Public Async Function Update(id As Guid, <FromBody> dto As UpdateLearnerDto) As Task(Of ActionResult(Of LearnerDto))
            Try
                If Not ModelState.IsValid Then
                    Return BadRequest(ModelState)
                End If
                
                Dim updated = Await _learnerService.UpdateAsync(id, dto)
                Return Ok(updated)
            Catch ex As InvalidOperationException
                Return NotFound(New With {.message = ex.Message})
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpDelete("{id}")>
        Public Async Function Delete(id As Guid) As Task(Of ActionResult)
            Try
                Dim result = Await _learnerService.DeleteAsync(id)
                If Not result Then
                    Return NotFound()
                End If
                Return NoContent()
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
    End Class
End Namespace
