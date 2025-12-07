Imports Microsoft.AspNetCore.Mvc
Imports IDP.Application.Interfaces
Imports IDP.Application.Services
Imports IDP.Domain.Entities
Imports System.Threading.Tasks

Namespace Controllers
    <ApiController>
    <Route("api/[controller]")>
    Public Class SetasController
        Inherits ControllerBase
        
        Private ReadOnly _setaRepository As ISetaRepository
        
        Public Sub New(setaRepository As ISetaRepository)
            _setaRepository = setaRepository
        End Sub
        
        ''' <summary>
        ''' Get all SETAs
        ''' </summary>
        <HttpGet>
        Public Async Function GetAll() As Task(Of ActionResult(Of List(Of Seta)))
            Dim setas = Await _setaRepository.GetAllAsync()
            Return Ok(setas)
        End Function
        
        ''' <summary>
        ''' Get all active SETAs
        ''' </summary>
        <HttpGet("active")>
        Public Async Function GetActive() As Task(Of ActionResult(Of List(Of Seta)))
            Dim setas = Await _setaRepository.GetActiveSetasAsync()
            Return Ok(setas)
        End Function
        
        ''' <summary>
        ''' Get SETA by ID
        ''' </summary>
        <HttpGet("{id}")>
        Public Async Function GetById(id As Guid) As Task(Of ActionResult(Of Seta))
            Dim seta = Await _setaRepository.GetByIdAsync(id)
            
            If seta Is Nothing Then
                Return NotFound($"SETA with ID {id} not found")
            End If
            
            Return Ok(seta)
        End Function
        
        ''' <summary>
        ''' Get SETA by code (e.g., BANKSETA, CETA)
        ''' </summary>
        <HttpGet("code/{setaCode}")>
        Public Async Function GetByCode(setaCode As String) As Task(Of ActionResult(Of Seta))
            Dim seta = Await _setaRepository.GetBySetaCodeAsync(setaCode.ToUpper())
            
            If seta Is Nothing Then
                Return NotFound($"SETA with code {setaCode} not found")
            End If
            
            Return Ok(seta)
        End Function
        
        ''' <summary>
        ''' Create new SETA
        ''' </summary>
        <HttpPost>
        Public Async Function Create(<FromBody> seta As Seta) As Task(Of ActionResult(Of Seta))
            Try
                Dim createdSeta = Await _setaRepository.AddAsync(seta)
                Return CreatedAtAction(NameOf(GetById), New With {.id = createdSeta.Id}, createdSeta)
            Catch ex As Exception
                Return BadRequest($"Error creating SETA: {ex.Message}")
            End Try
        End Function
        
        ''' <summary>
        ''' Update SETA
        ''' </summary>
        <HttpPut("{id}")>
        Public Async Function Update(id As Guid, <FromBody> seta As Seta) As Task(Of ActionResult)
            If id <> seta.Id Then
                Return BadRequest("ID mismatch")
            End If
            
            Dim existingSeta = Await _setaRepository.GetByIdAsync(id)
            If existingSeta Is Nothing Then
                Return NotFound()
            End If
            
            Try
                Await _setaRepository.UpdateAsync(seta)
                Return NoContent()
            Catch ex As Exception
                Return BadRequest($"Error updating SETA: {ex.Message}")
            End Try
        End Function
    End Class
End Namespace
