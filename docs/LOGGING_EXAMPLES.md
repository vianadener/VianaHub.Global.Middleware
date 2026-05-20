# Exemplos de Logs dos Middlewares Especializados

Este documento apresenta exemplos reais de logs gerados pelos middlewares especializados em diferentes cenários.

---

## 🎯 **Cenário 1: Requisição Bem-Sucedida (Sem Erros)**

**Request:**
```http
GET /api/users/123 HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000
```

**Logs:** *(Nenhum log de erro, apenas logs informacionais da aplicação)*

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 123,
  "name": "John Doe",
  "email": "john@example.com"
}
```

---

## ❌ **Cenário 2: Exceção de Domínio - Usuário Não Encontrado**

**Request:**
```http
GET /api/users/999 HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440001
```

**Middleware:** `DomainExceptionMiddleware`

**Log:**
```log
[14:32:15 WRN] ⚠️ [ERROR-a1b2c3d4e5f6] [CorrelationId: 550e8400-e29b-41d4-a716-446655440001] Domain Exception | Type: NotFoundException | StatusCode: 404 | Message: User with ID 999 not found
```

**Response:**
```http
HTTP/1.1 404 Not Found
Content-Type: application/json; charset=utf-8

{
  "title": "NotFound",
  "errors": {
    "Domain": ["User with ID 999 not found"],
    "Error ID": ["Contact support with the ID: a1b2c3d4e5f6"]
  }
}
```

---

## 🔧 **Cenário 3: Erro de JSON Malformado**

**Request:**
```http
POST /api/users HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440002
Content-Type: application/json

{
  "name": "Jane Doe",
  "email": "jane@example.com",
}
```
*(Note a vírgula extra no final)*

**Middleware:** `JsonExceptionMiddleware`

**Log:**
```log
[14:35:22 WRN] ⚠️ [ERROR-g7h8i9j0k1l2] [CorrelationId: 550e8400-e29b-41d4-a716-446655440002] JSON Exception | Error: JSON contains an invalid trailing comma | Line: 4 | Position: 1
```

**Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8

{
  "title": "Invalid JSON Format",
  "errors": {
    "Format": ["JSON contains an invalid trailing comma"],
    "Error ID": ["Contact support with the ID: g7h8i9j0k1l2"],
    "Line": ["Line 4"],
    "Position": ["Position 1"]
  }
}
```

---

## 🚫 **Cenário 4: Erro de Protocolo HTTP - Request Body Muito Grande**

**Request:**
```http
POST /api/users HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440003
Content-Type: application/json
Content-Length: 50000000

{ ... payload de 50MB ... }
```

**Middleware:** `HttpProtocolExceptionMiddleware`

**Log:**
```log
[14:38:45 WRN] ⚠️ [ERROR-m3n4o5p6q7r8] [CorrelationId: 550e8400-e29b-41d4-a716-446655440003] HTTP Protocol Exception | StatusCode: 413 | Message: Request body too large
```

**Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8

{
  "title": "BadRequest",
  "errors": {
    "Request": ["Request body too large"],
    "Error ID": ["Contact support with the ID: m3n4o5p6q7r8"]
  }
}
```

---

## ✅ **Cenário 5: Notificações de Validação**

**Request:**
```http
POST /api/users HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440004
Content-Type: application/json

{
  "name": "",
  "email": "invalid-email"
}
```

**Middleware:** `NotificationMiddleware`

**Log:**
```log
[14:42:10 WRN] ⚠️ [NOTIFY] [CorrelationId: 550e8400-e29b-41d4-a716-446655440004] Notifications Found | StatusCode: 400 | Count: 2 | Messages: Name: Field is required; Email: Invalid email format
```

**Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8

{
  "title": "BadRequest",
  "errors": {
    "Name": ["Field is required"],
    "Email": ["Invalid email format"]
  }
}
```

---

## 💥 **Cenário 6: Exceção Não Tratada (Catch-All)**

**Request:**
```http
GET /api/users/crash HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440005
```

**Middleware:** `GlobalExceptionMiddleware`

**Log:**
```log
[14:45:33 ERR] ⚠️ [ERROR-s9t0u1v2w3x4] [CorrelationId: 550e8400-e29b-41d4-a716-446655440005] Unhandled Exception | Type: NullReferenceException | Message: Object reference not set to an instance of an object
   at MyApp.Controllers.UsersController.Get(String id) in /src/Controllers/UsersController.cs:line 42
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   ... (stack trace continua)
```

**Response:**
```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8

{
  "title": "Internal Server Error",
  "errors": {
    "System": ["An unexpected error occurred. Our team has been notified and is working on a solution"],
    "Error ID": ["Contact support with the ID: s9t0u1v2w3x4"]
  }
}
```

---

## 🔄 **Cenário 7: Múltiplos Erros de Validação com Campos Específicos**

**Request:**
```http
POST /api/products HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440006
Content-Type: application/json

{
  "name": "",
  "price": -10,
  "quantity": -5
}
```

**Middleware:** `NotificationMiddleware`

**Log:**
```log
[14:50:15 WRN] ⚠️ [NOTIFY] [CorrelationId: 550e8400-e29b-41d4-a716-446655440006] Notifications Found | StatusCode: 400 | Count: 3 | Messages: Name: Product name is required; Price: Price must be greater than zero; Quantity: Quantity cannot be negative
```

**Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8

{
  "title": "BadRequest",
  "errors": {
    "Name": ["Product name is required"],
    "Price": ["Price must be greater than zero"],
    "Quantity": ["Quantity cannot be negative"]
  }
}
```

---

## 🔍 **Cenário 8: Conflito de Recurso (409)**

**Request:**
```http
POST /api/users HTTP/1.1
Host: api.example.com
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440007
Content-Type: application/json

{
  "email": "existing@example.com",
  "name": "Duplicate User"
}
```

**Middleware:** `DomainExceptionMiddleware`

**Log:**
```log
[14:55:42 WRN] ⚠️ [ERROR-y5z6a7b8c9d0] [CorrelationId: 550e8400-e29b-41d4-a716-446655440007] Domain Exception | Type: ConflictException | StatusCode: 409 | Message: User with email 'existing@example.com' already exists
```

**Response:**
```http
HTTP/1.1 409 Conflict
Content-Type: application/json; charset=utf-8

{
  "title": "Conflict",
  "errors": {
    "Domain": ["User with email 'existing@example.com' already exists"],
    "Error ID": ["Contact support with the ID: y5z6a7b8c9d0"]
  }
}
```

---

## 📊 **Consultando Logs no Elasticsearch/Kibana**

### **Exemplo 1: Buscar todos os erros de um Correlation-ID**

**Query:**
```
CorrelationId: "550e8400-e29b-41d4-a716-446655440002"
```

**Resultado:** Retorna **todos os logs** relacionados àquela requisição específica.

---

### **Exemplo 2: Buscar erros de JSON dos últimos 7 dias**

**Query:**
```
Message: "JSON Exception" AND @timestamp >= now-7d
```

**Resultado:** Lista todos os erros de parsing JSON da última semana.

---

### **Exemplo 3: Buscar exceções não tratadas (500)**

**Query:**
```
Message: "Unhandled Exception" AND Level: Error
```

**Resultado:** Lista todas as exceções capturadas pelo `GlobalExceptionMiddleware`.

---

### **Exemplo 4: Agrupar erros por tipo de exceção**

**Aggregation (Kibana):**
```json
{
  "aggs": {
    "exception_types": {
      "terms": {
        "field": "ExceptionType.keyword",
        "size": 10
      }
    }
  }
}
```

**Resultado:** Top 10 tipos de exceções mais comuns.

---

## 🎯 **Boas Práticas de Logging**

1. **Sempre envie X-Correlation-ID do cliente** para rastreabilidade end-to-end
2. **Use structured logging** - Evite strings concatenadas
3. **Defina níveis apropriados:**
   - `Error` - Exceções não tratadas
   - `Warning` - Exceções de domínio, validações, JSON
   - `Information` - Operações bem-sucedidas
4. **Monitore dashboards** com métricas agregadas de erros
5. **Configure alertas** para picos de erros 500
6. **Retenha logs** por pelo menos 30 dias para análise histórica

---

## 📖 **Recursos Adicionais**

- [Serilog Documentation](https://github.com/serilog/serilog/wiki)
- [Serilog Elasticsearch Sink](https://github.com/serilog-contrib/serilog-sinks-elasticsearch)
- [Kibana Query Language (KQL)](https://www.elastic.co/guide/en/kibana/current/kuery-query.html)
- [Structured Logging Best Practices](https://messagetemplates.org/)
