# Specialized Exception Handling Middlewares

## Overview

Este projeto implementa uma estratégia de tratamento de exceções baseada em **middlewares especializados**, seguindo os princípios **SOLID**, **Clean Architecture**, **DDD** e **Arquitetura Hexagonal**.

Cada middleware tem uma **única responsabilidade** e trata um tipo específico de erro, permitindo:
- ✅ **Separação de Concerns** (cada middleware cuida de um aspecto)
- ✅ **Testabilidade** (testes isolados e focados)
- ✅ **Reusabilidade** (composição seletiva)
- ✅ **Manutenibilidade** (mudanças isoladas)
- ✅ **Logging Padronizado** (Serilog com Correlation-ID em todos os logs)

---

## Padrão de Logging

### 📋 **Características do Logging:**

1. **Framework Unificado:** Todos os middlewares usam **Serilog** diretamente (sem `ILogger<T>`)
2. **Correlation-ID:** Todos os logs incluem o Correlation-ID do header da requisição
3. **Formato Estruturado:** Logs padronizados com informações contextuais
4. **StackTrace Completo:** ⚠️ **SEMPRE** logado via Serilog para diagnóstico interno
5. **Segurança:** ❌ **NUNCA** retornar detalhes técnicos (exceções, stack traces, tipos) nas responses

### 🔒 **Política de Segurança:**

**✅ O QUE É LOGADO (Serilog - Interno):**
- Tipo completo da exceção (`exception.GetType().FullName`)
- Mensagem técnica da exceção
- **StackTrace completo** (`exception.StackTrace`)
- Correlation-ID
- Error-ID único
- Todos os detalhes técnicos necessários para debugging

**❌ O QUE NÃO É RETORNADO (Response - Externo):**
- ❌ Tipo da exceção
- ❌ StackTrace
- ❌ Mensagens técnicas de exceções de sistema
- ❌ Caminhos de arquivos
- ❌ Informações sensíveis do servidor

**✅ O QUE É RETORNADO (Response - Externo):**
- ✅ Mensagens amigáveis e genéricas
- ✅ Error-ID para rastreamento
- ✅ Status HTTP apropriado
- ✅ Mensagens de exceções de **domínio** (são controladas e seguras)
- ✅ Para JSON: linha e posição (úteis para o desenvolvedor)

### 🔍 **Headers de Correlation-ID Suportados:**

O helper `ErrorResponseHelper.GetCorrelationId()` verifica os seguintes headers (em ordem):
- `X-Correlation-ID`
- `X-Correlation-Id`
- `CorrelationId`
- `context.Items["X-Correlation-ID"]`

Se nenhum for encontrado, retorna `"N/A"`.

### 📝 **Formato Padrão de Log:**

```log
⚠️ [ERROR-abc123def456] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] Unhandled Exception | Type: System.NullReferenceException | Message: Object reference not set... | StackTrace: at MyApp.Service.DoSomething() in /app/Service.cs:line 42...
```

**Componentes:**
- `[ERROR-{ErrorId}]` - ID único do erro (12 caracteres)
- `[CorrelationId: {CorrelationId}]` - ID de correlação da requisição
- `Type: {ExceptionType}` - Tipo **COMPLETO** da exceção (FullName)
- `Message: {Message}` - Mensagem da exceção
- `StackTrace: {StackTrace}` - **StackTrace COMPLETO** (apenas no log)

---

## Middlewares Disponíveis

### 1. **GlobalExceptionMiddleware** (Catch-All Final)
**Responsabilidade:** Captura qualquer exceção não tratada pelos middlewares especializados.

**Quando usar:** Sempre como o **primeiro middleware** (mais externo) do pipeline.

**Exceções capturadas:**
- `Exception` (qualquer exceção não tratada)

**Exemplo de Log (Interno - Serilog):**
```log
⚠️ [ERROR-abc123def456] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] Unhandled Exception | Type: System.NullReferenceException | Message: Object reference not set to an instance of an object | StackTrace: at MyApp.Controllers.UsersController.Get(Int32 id) in /app/Controllers/UsersController.cs:line 42
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor...
```

**Response (Externo - Cliente):**
```json
{
  "title": "Internal Server Error",
  "errors": {
    "System": ["An unexpected error occurred. Our team has been notified and is working on a solution"],
    "Error ID": ["Contact support with the ID: abc123def456"]
  }
}
```

**⚠️ Nota de Segurança:** O tipo da exceção, mensagem técnica e StackTrace **NÃO** são retornados ao cliente.

---

### 2. **HttpProtocolExceptionMiddleware**
**Responsabilidade:** Captura erros de protocolo HTTP (headers malformados, body muito grande, etc.).

**Exceções capturadas:**
- `BadHttpRequestException` (sem `JsonException` interna)

**Exemplo de Log (Interno - Serilog):**
```log
⚠️ [ERROR-xyz789abc123] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] HTTP Protocol Exception | StatusCode: 413 | Message: Request body too large. Limit is 30MB | StackTrace: at Microsoft.AspNetCore.Server.Kestrel.Core.BadHttpRequestException...
```

**Response (Externo - Cliente):**
```json
{
  "title": "BadRequest",
  "errors": {
    "Request": ["The request could not be processed due to invalid format or content"],
    "Error ID": ["Contact support with the ID: xyz789abc123"]
  }
}
```

**⚠️ Nota de Segurança:** A mensagem técnica da exceção **NÃO** é retornada. Uma mensagem genérica é usada.

---

### 3. **JsonExceptionMiddleware**
**Responsabilidade:** Captura erros de parsing JSON no body da requisição.

**Exceções capturadas:**
- `JsonException`
- `BadHttpRequestException` com `JsonException` interna

**Exemplo de Log (Interno - Serilog):**
```log
⚠️ [ERROR-def456ghi789] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] JSON Exception | Error: JSON contains an invalid trailing comma | Line: 5 | Position: 12 | Message: ',' is an invalid start of a value. Path: $.address | LineNumber: 5 | BytePositionInLine: 12 | StackTrace: at System.Text.Json.ThrowHelper...
```

**Response (Externo - Cliente):**
```json
{
  "title": "Invalid JSON Format",
  "errors": {
    "Format": ["JSON contains an invalid trailing comma"],
    "Error ID": ["Contact support with the ID: def456ghi789"],
    "Line": ["Line 5"],
    "Position": ["Position 12"]
  }
}
```

**✅ Exceção:** Linha e posição **são retornadas** pois ajudam o desenvolvedor a localizar o erro sem expor informações do sistema.

**Mensagens amigáveis:**
- "JSON contains an invalid trailing comma"
- "JSON contains an invalid character"
- "JSON contains an unterminated string"
- "JSON is malformed"
- "JSON has too many nesting levels"
- "JSON contains an invalid property name"
- "JSON is incomplete"
- "JSON is invalid" (genérico)

---

### 4. **DomainExceptionMiddleware**
**Responsabilidade:** Captura exceções de domínio/negócio customizadas.

**Exceções capturadas:**
- `NotFoundException` → 404
- `BadRequestException` → 400
- `ConflictException` → 409

**Exemplo de Log (Interno - Serilog):**
```log
⚠️ [ERROR-jkl012mno345] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] Domain Exception | Type: MyApp.Domain.Exceptions.NotFoundException | StatusCode: 404 | Message: User with ID 123 not found | StackTrace: at MyApp.Services.UserService.GetById(Int32 id) in /app/Services/UserService.cs:line 28...
```

**Response (Externo - Cliente):**
```json
{
  "title": "NotFound",
  "errors": {
    "Domain": ["User with ID 123 not found"],
    "Error ID": ["Contact support with the ID: jkl012mno345"]
  }
}
```

**✅ Exceção:** A mensagem da exceção de domínio **É retornada** pois são mensagens **controladas, amigáveis e seguras** escritas pelos desenvolvedores.

---

### 5. **NotificationMiddleware**
**Responsabilidade:** Processa notificações acumuladas via `INotify` (erros de validação, regras de negócio).

**Quando usar:** Sempre como o **último middleware** (mais interno) do pipeline.

**Exemplo de Log (Interno - Serilog):**
```log
⚠️ [NOTIFY] [CorrelationId: 550e8400-e29b-41d4-a716-446655440000] Notifications Found | StatusCode: 400 | Count: 2 | Messages: Name: Field is required; Email: Invalid email format
```

**Fluxo:**
1. Request passa por todos os middlewares
2. Lógica de aplicação executa e acumula erros no `INotify`
3. NotificationMiddleware verifica se há notificações e retorna erro estruturado

**Response (Externo - Cliente):**
```json
{
  "title": "BadRequest",
  "errors": {
    "Name": ["Field is required"],
    "Email": ["Invalid email format"]
  }
}
```

**✅ Seguro:** As mensagens do `INotify` **devem ser amigáveis** e escritas pelos desenvolvedores. Nunca adicione detalhes técnicos ao Notify.

---

## Configuração do Pipeline

### ⚠️ **Ordem Correta (IMPORTANTE!)**

A ordem dos middlewares é **crítica** para o funcionamento correto:

```csharp
// Program.cs ou Startup.cs

// RECOMENDADO: Configure o Correlation-ID ANTES dos middlewares de exceção
app.UseMiddleware<CorrelationIdMiddleware>(); // ← Adiciona X-Correlation-ID se não existir

// 1º - Catch-all final (fallback para qualquer exceção não tratada)
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2º - HTTP protocol errors (BadHttpRequestException sem JSON)
app.UseMiddleware<HttpProtocolExceptionMiddleware>();

// 3º - JSON parsing errors (JsonException, BadHttpRequestException com JsonException)
app.UseMiddleware<JsonExceptionMiddleware>();

// 4º - Domain exceptions (NotFoundException, BadRequestException, ConflictException)
app.UseMiddleware<DomainExceptionMiddleware>();

// ... outros middlewares da aplicação ...
// app.UseAuthentication();
// app.UseAuthorization();
// app.MapControllers();

// ÚLTIMO - Processa notificações acumuladas
app.UseMiddleware<NotificationMiddleware>();
```

### 📋 **Explicação da Ordem:**

1. **CorrelationIdMiddleware** (opcional mas recomendado) - Garante que toda request tenha um Correlation-ID
2. **GlobalExceptionMiddleware** - Mais externo, captura tudo que "vazar"
3. **HttpProtocolExceptionMiddleware** - Erros de infraestrutura HTTP
4. **JsonExceptionMiddleware** - Erros de parsing JSON
5. **DomainExceptionMiddleware** - Exceções de negócio
6. **NotificationMiddleware** - Mais interno, processa notificações pós-execução

---

## Configuração do Correlation-ID

### 🔧 **Adicionando Correlation-ID Automaticamente**

Se você quiser garantir que **todas as requisições** tenham um Correlation-ID (mesmo que o cliente não envie), use o `CorrelationIdMiddleware`:

```csharp
// Program.cs
app.UseMiddleware<CorrelationIdMiddleware>(); // ← ANTES dos middlewares de exceção
```

### 📤 **Enviando Correlation-ID do Cliente**

Os clientes devem enviar o Correlation-ID no header:

```http
GET /api/users/123 HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json
```

### 🔍 **Buscando Logs por Correlation-ID**

Com o Correlation-ID, você pode rastrear toda a jornada de uma requisição:

**Exemplo de consulta no Elasticsearch/Kibana:**
```
CorrelationId: "550e8400-e29b-41d4-a716-446655440000"
```

Isso retornará **todos os logs** relacionados àquela requisição específica, facilitando debugging e análise de problemas.

---

## Uso com INotify

### Injeção de Dependência

```csharp
// Program.cs
builder.Services.AddScoped<INotify, Notify>();
```

### Exemplo de Uso em um Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly INotify _notify;

    public UsersController(INotify notify)
    {
        _notify = notify;
    }

    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        // Validações de negócio
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _notify.Add("Name: Field is required", 400);
        }

        if (!IsValidEmail(request.Email))
        {
            _notify.Add("Email: Invalid email format", 400);
        }

        // NotificationMiddleware intercepta e retorna erro estruturado
        if (_notify.HasNotify())
        {
            return Ok(); // Ou return; se for void
        }

        // Lógica de criação do usuário
        // ...

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
```

---

## 🔒 Boas Práticas de Segurança

### ❌ **O QUE NUNCA FAZER:**

```csharp
// ❌ ERRADO - Expõe detalhes técnicos
notify.Add($"Error: {exception.Message}", 500);
notify.Add($"StackTrace: {exception.StackTrace}", 500);

// ❌ ERRADO - Expõe caminhos do sistema
errorResponse.AddError("Error", exception.ToString());

// ❌ ERRADO - Expõe tipo da exceção
errorResponse.AddError("Type", exception.GetType().Name);
```

### ✅ **O QUE FAZER:**

```csharp
// ✅ CORRETO - Log interno completo
Log.Error(exception, 
    "⚠️ [ERROR-{ErrorId}] [CorrelationId: {CorrelationId}] Error | Type: {Type} | Message: {Message} | StackTrace: {StackTrace}", 
    errorId, correlationId, exception.GetType().FullName, exception.Message, exception.StackTrace);

// ✅ CORRETO - Response amigável
notify.Add("An error occurred while processing your request", 500);
errorResponse.AddError("System", "An unexpected error occurred. Please try again later");
errorResponse.AddError("Error ID", $"Contact support with the ID: {errorId}");
```

### 📋 **Checklist de Segurança:**

Antes de fazer commit, verifique:

- [ ] ✅ Todos os logs incluem **StackTrace completo**
- [ ] ❌ Nenhuma response retorna **tipo de exceção**
- [ ] ❌ Nenhuma response retorna **StackTrace**
- [ ] ❌ Nenhuma response retorna **caminhos de arquivos**
- [ ] ❌ Nenhuma response retorna **mensagens técnicas de exceções de sistema**
- [ ] ✅ Todas as mensagens ao cliente são **amigáveis e genéricas**
- [ ] ✅ Exceções de domínio têm **mensagens controladas e seguras**
- [ ] ✅ Todos os erros têm **Error-ID** para rastreamento
- [ ] ✅ Todos os logs têm **Correlation-ID**

---

## Exceções de Domínio Customizadas

### ⚠️ **IMPORTANTE: Mensagens de Exceções de Domínio**

As exceções de domínio (NotFoundException, BadRequestException, ConflictException) **RETORNAM suas mensagens ao cliente**.

Portanto, **SEMPRE** escreva mensagens amigáveis e seguras:

### ✅ **CORRETO:**

```csharp
// ✅ Mensagem amigável e segura
throw new NotFoundException("User not found");
throw new NotFoundException($"Product with ID {productId} not found");
throw new ConflictException("Email already registered");
throw new BadRequestException("Invalid date format");
```

### ❌ **ERRADO:**

```csharp
// ❌ Expõe detalhes técnicos
throw new NotFoundException($"User not found in database table Users: {ex.Message}");

// ❌ Expõe estrutura interna
throw new NotFoundException($"Entity User with primary key {id} does not exist in DbContext");

// ❌ Expõe lógica de negócio sensível
throw new BadRequestException($"Validation failed: {validator.GetDetailedReport()}");
```

---

## Classe Helper: ErrorResponseHelper

Centraliza funcionalidades compartilhadas:

### Métodos Públicos

```csharp
// Gera ID único de erro (12 caracteres)
string errorId = ErrorResponseHelper.GenerateErrorId();

// Retorna opções padrão de serialização JSON
JsonSerializerOptions options = ErrorResponseHelper.GetJsonSerializerOptions();

// Escreve resposta JSON no HttpContext
await ErrorResponseHelper.WriteJsonResponseAsync(context, errorResponse, statusCode);

// Retorna título amigável para status code
string title = ErrorResponseHelper.GetErrorTitle(404); // "NotFound"
```

---

## Testes Unitários

Cada middleware possui sua própria suite de testes:

- `GlobalExceptionMiddlewareTests.cs` - 13 testes
- `HttpProtocolExceptionMiddlewareTests.cs` - 8 testes
- `JsonExceptionMiddlewareTests.cs` - 11 testes
- `DomainExceptionMiddlewareTests.cs` - 12 testes
- `NotificationMiddlewareTests.cs` - 7 testes
- `GetFriendlyJsonErrorKeyTests.cs` - 8 testes (migrado para JsonExceptionMiddleware)

### Executar Testes

```bash
dotnet test
```

---

## Migração do Antigo GlobalExceptionMiddleware

### ⚠️ Breaking Changes

O **GlobalExceptionMiddleware** foi **refatorado**:

**Antes (Versão Monolítica):**
```csharp
public GlobalExceptionMiddleware(RequestDelegate next, IWebHostEnvironment env)
```

**Depois (Versão Especializada):**
```csharp
public GlobalExceptionMiddleware(RequestDelegate next)
```

### Passos de Migração

1. **Remover parâmetro `IWebHostEnvironment`:**
   ```csharp
   // Antes
   app.UseMiddleware<GlobalExceptionMiddleware>();
   
   // Depois (continua igual, mas internamente usa apenas RequestDelegate)
   app.UseMiddleware<GlobalExceptionMiddleware>();
   ```

2. **Adicionar middlewares especializados:**
   ```csharp
   app.UseMiddleware<GlobalExceptionMiddleware>();
   app.UseMiddleware<HttpProtocolExceptionMiddleware>();
   app.UseMiddleware<JsonExceptionMiddleware>();
   app.UseMiddleware<DomainExceptionMiddleware>();
   // ... outros middlewares ...
   app.UseMiddleware<NotificationMiddleware>();
   ```

3. **Atualizar testes:**
   - Remover mocks de `IWebHostEnvironment`
   - Migrar testes de JSON para `JsonExceptionMiddlewareTests`
   - Migrar testes de notificações para `NotificationMiddlewareTests`

---

## Benefícios da Arquitetura Especializada

| Aspecto | Middleware Monolítico | Middlewares Especializados |
|---------|------------------------|---------------------------|
| **SRP (SOLID)** | ❌ Múltiplas responsabilidades | ✅ Uma responsabilidade cada |
| **Testabilidade** | ⚠️ Testes complexos e acoplados | ✅ Testes isolados e simples |
| **Manutenibilidade** | ⚠️ Mudanças afetam múltiplos fluxos | ✅ Mudanças isoladas |
| **Reusabilidade** | ⚠️ All-or-nothing | ✅ Composição seletiva |
| **Clareza** | ⚠️ 350+ linhas em um arquivo | ✅ ~80-100 linhas cada |
| **Debugging (Logs Internos)** | ⚠️ Stack traces misturados | ✅ Stack traces completos e rastreáveis |
| **Segurança (Responses)** | ⚠️ Pode expor detalhes técnicos | ✅ Apenas mensagens amigáveis |

---

## FAQ

### 1. Por que não usar um único middleware?
**Resposta:** Violaria o Single Responsibility Principle (SOLID) e dificultaria manutenção e testes.

### 2. A performance não é pior com vários middlewares?
**Resposta:** O overhead é negligenciável (~0.1-0.5ms), mas os ganhos em manutenibilidade compensam amplamente.

### 3. Posso usar apenas alguns middlewares?
**Resposta:** Sim! A arquitetura permite composição seletiva. Exemplo:
```csharp
// API pública - todos os middlewares
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<JsonExceptionMiddleware>();
app.UseMiddleware<DomainExceptionMiddleware>();

// API interna - apenas catch-all
app.UseMiddleware<GlobalExceptionMiddleware>();
```

### 4. Como adicionar um novo tipo de exceção?
**Resposta:** Crie um novo middleware especializado ou adicione ao `DomainExceptionMiddleware`.

### 5. Por que NotificationMiddleware é o último?
**Resposta:** Ele processa notificações **após** a execução da lógica de negócio, verificando se há erros acumulados.

### 6. Por que usar Serilog ao invés de ILogger<T>?
**Resposta:** 
- **Consistência:** Todos os middlewares usam o mesmo framework de logging
- **Structured Logging:** Serilog oferece logging estruturado out-of-the-box
- **Simplicidade:** Não precisa injetar `ILogger<T>` em cada middleware
- **Flexibilidade:** Facilita configuração de sinks (Elasticsearch, Seq, Console, etc.)

### 7. O que acontece se não houver Correlation-ID no header?
**Resposta:** O helper `GetCorrelationId()` retorna `"N/A"`. Recomenda-se usar o `CorrelationIdMiddleware` para gerar automaticamente quando ausente.

### 8. Como configurar o Serilog?
**Resposta:** Exemplo básico no `Program.cs`:
```csharp
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(); // ← Usar Serilog

var app = builder.Build();

// ... configurar middlewares ...

app.Run();
```

### 9. Como os logs aparecem no Elasticsearch?
**Resposta:** Configure o Serilog Elasticsearch sink:
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "mylogs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();
```

### 10. Posso customizar o formato dos logs?
**Resposta:** Sim! Modifique os logs diretamente nos middlewares ou use enrichers do Serilog:
```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();
```

---

## Recursos Adicionais

- [Documentação Microsoft - Middleware ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)

---

## Contribuindo

Para contribuir com melhorias:

1. Crie um branch: `git checkout -b feature/nova-feature`
2. Commit suas mudanças: `git commit -m 'Add nova feature'`
3. Push para o branch: `git push origin feature/nova-feature`
4. Abra um Pull Request

---

## Licença

Copyright (c) Fidelidade. All rights reserved.
